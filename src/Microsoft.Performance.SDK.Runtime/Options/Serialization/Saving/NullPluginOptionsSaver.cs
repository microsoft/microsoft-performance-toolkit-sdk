// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.

using System.Threading.Tasks;
using Microsoft.Performance.SDK.Runtime.Options.Serialization.DTO;

namespace Microsoft.Performance.SDK.Runtime.Options.Serialization.Saving;

public sealed class NullPluginOptionsSaver
    : IPluginOptionsSaver
{
    public static NullPluginOptionsSaver Instance { get; } = new NullPluginOptionsSaver();

    private NullPluginOptionsSaver()
    {
    }

    public Task<bool> TrySave(PluginOptionsDto optionsDto)
    {
        return Task.FromResult(true);
    }
}