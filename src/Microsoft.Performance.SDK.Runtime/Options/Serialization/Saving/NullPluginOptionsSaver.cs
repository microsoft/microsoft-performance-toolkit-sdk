// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.

using System.Threading.Tasks;
using Microsoft.Performance.SDK.Runtime.Options.Serialization.DTO;

namespace Microsoft.Performance.SDK.Runtime.Options.Serialization.Saving;

/// <summary>
///     A <see cref="IPluginOptionsSaver"/> that always returns <c>false</c>.
/// </summary>
public sealed class NullPluginOptionsSaver
    : IPluginOptionsSaver
{
    /// <inheritdoc />
    public Task<bool> TrySaveAsync(PluginOptionsDto optionsDto)
    {
        return Task.FromResult(false);
    }
}