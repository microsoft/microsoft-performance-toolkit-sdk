// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.

using System.Threading.Tasks;
using Microsoft.Performance.SDK.Runtime.Options.Serialization.DTO;

namespace Microsoft.Performance.SDK.Runtime.Options.Serialization.Loading;

public sealed class NullPluginOptionsLoader
    : IPluginOptionsLoader
{
    public static NullPluginOptionsLoader Instance { get; } = new NullPluginOptionsLoader();

    private NullPluginOptionsLoader()
    {
    }

    public Task<PluginOptionsDto> TryLoadAsync()
    {
        return Task.FromResult<PluginOptionsDto>(null);
    }
}