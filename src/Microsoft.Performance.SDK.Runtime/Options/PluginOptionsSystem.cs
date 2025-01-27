// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.

using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.Performance.SDK.Processing;
using Microsoft.Performance.SDK.Runtime.Options.Serialization;
using Microsoft.Performance.SDK.Runtime.Options.Serialization.Loading;
using Microsoft.Performance.SDK.Runtime.Options.Serialization.Saving;

namespace Microsoft.Performance.SDK.Runtime.Options;

public sealed class PluginOptionsSystem
{
    private static readonly PluginOptionsRegistryToDtoConverter optionsRegistryToDtoConverter = new();

    public static PluginOptionsSystem CreateForFile(string filePath)
    {
        var loader = new FilePluginOptionsLoader(filePath);
        var saver = new FilePluginOptionsSaver(filePath);
        var registry = new PluginOptionsRegistry();

        return new PluginOptionsSystem(loader, saver, registry);
    }

    public static PluginOptionsSystem CreateUnsaved()
    {
        var loader = NullPluginOptionsLoader.Instance;
        var saver = NullPluginOptionsSaver.Instance;
        var registry = new PluginOptionsRegistry();

        return new PluginOptionsSystem(loader, saver, registry);
    }

    public PluginOptionsSystem(
        IPluginOptionsLoader loader,
        IPluginOptionsSaver saver,
        PluginOptionsRegistry registry)
    {
        Loader = loader;
        Saver = saver;
        Registry = registry;
    }

    public IPluginOptionsLoader Loader { get; }

    public IPluginOptionsSaver Saver { get; }

    public PluginOptionsRegistry Registry { get; }

    public void RegisterOptionsFrom(params IProcessingSource[] processingSources)
    {
        RegisterOptionsFrom((IEnumerable<IProcessingSource>)processingSources);
    }

    public void RegisterOptionsFrom(IEnumerable<IProcessingSource> processingSources)
    {
        this.Registry.RegisterFrom(new ProcessingSourcePluginOptionsProvider(processingSources.ToList()));
    }

    public async Task TryLoadAsync()
    {
        var dto = await this.Loader.TryLoadAsync();

        if (dto != null)
        {
            this.Registry.UpdateFromDto(dto);
        }
    }

    public Task<bool> TrySave()
    {
        var dto = optionsRegistryToDtoConverter.ConvertToDto(this.Registry);
        return this.Saver.TrySave(dto);
    }
}