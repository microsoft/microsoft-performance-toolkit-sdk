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
    Task<PluginOptionsDto> TryLoadAsync();
}