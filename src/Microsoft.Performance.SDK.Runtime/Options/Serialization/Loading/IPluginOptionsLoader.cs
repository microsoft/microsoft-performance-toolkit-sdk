// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.

using System.Threading.Tasks;
using Microsoft.Performance.SDK.Runtime.Options.Serialization.DTO;

namespace Microsoft.Performance.SDK.Runtime.Options.Serialization.Loading;

/// <summary>
///     Represents a class that can load a <see cref="PluginOptionsDto"/> instance.
/// </summary>
public interface IPluginOptionsLoader
{
    /// <summary>
    ///     Attempts to load a <see cref="PluginOptionsDto"/> instance.
    /// </summary>
    /// <returns>
    ///     The loaded <see cref="PluginOptionsDto"/> instance, or <c>null</c> if the load failed.
    /// </returns>
    Task<PluginOptionsDto> TryLoadAsync();
}