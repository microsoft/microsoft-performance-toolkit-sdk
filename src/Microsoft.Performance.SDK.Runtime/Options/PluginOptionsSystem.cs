// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.

using System.Threading.Tasks;
using Microsoft.Performance.SDK.Runtime.Options.Serialization;
using Microsoft.Performance.SDK.Runtime.Options.Serialization.Loading;
using Microsoft.Performance.SDK.Runtime.Options.Serialization.Saving;

namespace Microsoft.Performance.SDK.Runtime.Options;

public sealed class PluginOptionsSystem
{
    private static readonly PluginOptionsRegistryToDtoConverter optionsRegistryToDtoConverter = new();

    PluginOptionsSystem CreateForFile(string filePath)
    {
        var loader = new FilePluginOptionsLoader(filePath);
        var saver = new FilePluginOptionsSaver(filePath);
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

    IPluginOptionsLoader Loader { get; }

    IPluginOptionsSaver Saver { get; }

    PluginOptionsRegistry Registry { get; }

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