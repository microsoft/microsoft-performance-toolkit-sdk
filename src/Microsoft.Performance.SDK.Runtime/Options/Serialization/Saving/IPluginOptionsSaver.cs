// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.

using System.Threading.Tasks;
using Microsoft.Performance.SDK.Runtime.Options.Serialization.DTO;

namespace Microsoft.Performance.SDK.Runtime.Options.Serialization.Saving;

/// <summary>
///     Represents a class that can save a <see cref="PluginOptionsDto"/> instance.
/// </summary>
public interface IPluginOptionsSaver
{
    /// <summary>
    ///     Attempts to save the given <see cref="PluginOptionsDto"/> instance.
    /// </summary>
    /// <param name="optionsDto">
    ///     The <see cref="PluginOptionsDto"/> to save.
    /// </param>
    /// <returns>
    ///     <c>true</c> if the <see cref="PluginOptionsDto"/> was saved; <c>false</c> otherwise.
    /// </returns>
    Task<bool> TrySaveAsync(PluginOptionsDto optionsDto);
}