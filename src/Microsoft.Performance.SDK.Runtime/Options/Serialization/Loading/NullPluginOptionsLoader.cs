// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.

using System.Threading.Tasks;
using Microsoft.Performance.SDK.Runtime.Options.Serialization.DTO;

namespace Microsoft.Performance.SDK.Runtime.Options.Serialization.Loading;

/// <summary>
///     A <see cref="IPluginOptionsLoader"/> that always returns <c>null</c>.
/// </summary>
public sealed class NullPluginOptionsLoader
    : IPluginOptionsLoader
{
    /// <inheritdoc />
    public Task<PluginOptionsDto> TryLoadAsync()
    {
        return Task.FromResult<PluginOptionsDto>(null);
    }
}