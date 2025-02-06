// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.

using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.Performance.SDK.Options;
using Microsoft.Performance.SDK.Processing;
using Microsoft.Performance.SDK.Runtime.Options.Serialization;
using Microsoft.Performance.SDK.Runtime.Options.Serialization.Loading;
using Microsoft.Performance.SDK.Runtime.Options.Serialization.Saving;

namespace Microsoft.Performance.SDK.Runtime.Options;

/// <summary>
///     Represents a system for managing plugin options.
/// </summary>
public sealed class PluginOptionsSystem
{
    private static readonly PluginOptionsRegistryToDtoConverter optionsRegistryToDtoConverter = new();

    /// <summary>
    ///     Creates a new instance of <see cref="PluginOptionsSystem"/> that persists options to a file.
    /// </summary>
    /// <param name="filePath">
    ///     The path to the file to which options will be saved and loaded.
    /// </param>
    /// <param name="loggerFactory">
    ///     A factory for creating loggers.
    /// </param>
    /// <returns>
    ///     A new instance of <see cref="PluginOptionsSystem"/> that persists options to a file.
    /// </returns>
    public static PluginOptionsSystem CreateForFile(
        string filePath,
        Func<Type, ILogger> loggerFactory)
    {
        var loader = new FilePluginOptionsLoader(filePath, loggerFactory(typeof(FilePluginOptionsLoader)));
        var saver = new FilePluginOptionsSaver(filePath, loggerFactory(typeof(FilePluginOptionsSaver)));
        var registry = new PluginOptionsRegistry(loggerFactory(typeof(PluginOptionsRegistry)));

        return new PluginOptionsSystem(loader, saver, registry);
    }

    /// <summary>
    ///     Creates a new instance of <see cref="PluginOptionsSystem"/> that does not persist options.
    /// </summary>
    /// <param name="loggerFactory">
    ///     A factory for creating loggers.
    /// </param>
    /// <returns>
    ///     A new instance of <see cref="PluginOptionsSystem"/> that does not persist options.
    /// </returns>
    public static PluginOptionsSystem CreateUnsaved(Func<Type, ILogger> loggerFactory)
    {
        var loader = new NullPluginOptionsLoader();
        var saver = new NullPluginOptionsSaver();
        var registry = new PluginOptionsRegistry(loggerFactory(typeof(PluginOptionsRegistry)));

        return new PluginOptionsSystem(loader, saver, registry);
    }

    public static PluginOptionsSystem CreateInMemory(Func<Type, ILogger> loggerFactory)
    {
        var repository = new InMemoryPluginOptionsDtoRepository();
        var registry = new PluginOptionsRegistry(loggerFactory(typeof(PluginOptionsRegistry)));

        return new PluginOptionsSystem(repository, repository, registry);
    }

    /// <summary>
    ///     Initializes a new instance of the <see cref="PluginOptionsSystem"/> class.
    /// </summary>
    /// <param name="loader">
    ///     The loader to use to load persisted options.
    /// </param>
    /// <param name="saver">
    ///     The saver to use to save options.
    /// </param>
    /// <param name="registry">
    ///     The registry to use to manage options.
    /// </param>
    public PluginOptionsSystem(
        IPluginOptionsLoader loader,
        IPluginOptionsSaver saver,
        PluginOptionsRegistry registry)
    {
        Loader = loader;
        Saver = saver;
        Registry = registry;
    }

    /// <summary>
    ///     Gets the <see cref="IPluginOptionsLoader"/> to use to load persisted options.
    /// </summary>
    public IPluginOptionsLoader Loader { get; }

    /// <summary>
    ///     Gets the <see cref="IPluginOptionsSaver"/> to use to save options.
    /// </summary>
    public IPluginOptionsSaver Saver { get; }

    /// <summary>
    ///     Gets the <see cref="PluginOptionsRegistry"/> to use to manage options.
    /// </summary>
    public PluginOptionsRegistry Registry { get; }

    /// <summary>
    ///     Registers <see cref="PluginOption"/> provided by <see cref="IProcessingSource"/> instances to the
    ///     <see cref="Registry"/>.
    /// </summary>
    /// <param name="processingSources">
    ///     The <see cref="IProcessingSource"/> instances to register options from.
    /// </param>
    public void RegisterOptionsFrom(params IProcessingSource[] processingSources)
    {
        RegisterOptionsFrom((IEnumerable<IProcessingSource>)processingSources);
    }

    /// <summary>
    ///     Registers <see cref="PluginOption"/> provided by <see cref="IProcessingSource"/> instances to the
    ///     <see cref="Registry"/>.
    /// </summary>
    /// <param name="processingSources">
    ///     The <see cref="IProcessingSource"/> instances to register options from.
    /// </param>
    public void RegisterOptionsFrom(IEnumerable<IProcessingSource> processingSources)
    {
        this.Registry.RegisterFrom(new ProcessingSourcePluginOptionsProvider(processingSources.ToList()));
    }

    /// <summary>
    ///     Attempts to update all <see cref="PluginOption"/> instances registered to the <see cref="Registry"/> from the persisted options loaded by
    ///     the <see cref="Loader"/>.
    /// </summary>
    /// <returns>
    ///     <c>true</c> if the persisted options were loaded and the <see cref="Registry"/> was updated; <c>false</c>
    /// </returns>
    public async Task<bool> TryLoadAsync()
    {
        var dto = await this.Loader.TryLoadAsync();

        if (dto == null)
        {
            return false;
        }
        this.Registry.UpdateFromDto(dto);
        return true;
    }

    /// <summary>
    ///     Attempts to save the <see cref="PluginOption"/> instances registered to the <see cref="Registry"/> using
    ///     the <see cref="Saver"/>. Previously saved options, as loaded by the <see cref="Loader"/> during this method call,
    ///     will still be included in the saved options. If an option in the <see cref="Registry"/> was previously saved,
    ///     its value will be updated after the save.
    /// </summary>
    /// <returns>
    ///     <c>true</c> if the <see cref="Registry"/> was saved; <c>false</c> otherwise.
    /// </returns>
    /// <remarks>
    ///     This method is not atomic with respect to merging previously saved options. Concurrent modifications to the
    ///     backing storage of the <see cref="Loader"/> will result in the loss of those modifications.
    /// </remarks>
    public async Task<bool> TrySaveCurrentRegistry()
    {
        var newDto = optionsRegistryToDtoConverter.ConvertToDto(this.Registry);
        var oldDto = await this.Loader.TryLoadAsync();
        return await this.Saver.TrySaveAsync(oldDto.UpdateTo(newDto));
    }
}
