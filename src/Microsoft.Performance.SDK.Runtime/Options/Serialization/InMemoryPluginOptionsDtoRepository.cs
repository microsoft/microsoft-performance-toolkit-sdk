// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.

using System.Threading.Tasks;
using Microsoft.Performance.SDK.Runtime.Options.Serialization.DTO;
using Microsoft.Performance.SDK.Runtime.Options.Serialization.Loading;
using Microsoft.Performance.SDK.Runtime.Options.Serialization.Saving;

namespace Microsoft.Performance.SDK.Runtime.Options.Serialization;

/// <summary>
///     A <see cref="IPluginOptionsSaver"/> and <see cref="IPluginOptionsLoader"/> that saves and loads options in memory.
/// </summary>
public class InMemoryPluginOptionsDtoRepository
    : IPluginOptionsSaver,
      IPluginOptionsLoader
{
    private PluginOptionsDto lastSavedOptions;

    /// <inheritdoc />
    public Task<bool> TrySaveAsync(PluginOptionsDto optionsDto)
    {
        this.lastSavedOptions = optionsDto;
        return Task.FromResult(true);
    }

    /// <inheritdoc />
    public Task<PluginOptionsDto> TryLoadAsync()
    {
        return Task.FromResult(this.lastSavedOptions);
    }
}
