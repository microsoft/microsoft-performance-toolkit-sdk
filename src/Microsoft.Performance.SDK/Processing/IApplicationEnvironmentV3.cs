// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.

using System;
using Microsoft.Performance.SDK.Options;
using Microsoft.Performance.SDK.Options.Values;

namespace Microsoft.Performance.SDK.Processing;

/// <summary>
///     Extends <see cref="IApplicationEnvironmentV2"/> to provide additional functionality.
/// </summary>
[Obsolete("This interface will be removed in version 2.0 of the SDK. It is OK to use this interface in version 1.x of the SDK.")]
public interface IApplicationEnvironmentV3
    : IApplicationEnvironmentV2
{
    /// <summary>
    ///     Attempts to get the plugin option of type <typeparamref name="T"/> with the given GUID.
    /// </summary>
    /// <param name="optionGuid">
    ///     The <see cref="Guid"/> of the option to get.
    /// </param>
    /// <param name="option">
    ///     The found option if this method returns <c>true</c>; <c>null</c> otherwise.
    /// </param>
    /// <typeparam name="T">
    ///     The concrete type of the <see cref="PluginOption"/> to get. Must be a subclass of <see cref="PluginOption"/>.
    /// </typeparam>
    /// <returns>
    ///     <c>true</c> if the option was found; <c>false</c> otherwise.
    /// </returns>
    bool TryGetPluginOption<T>(Guid optionGuid, out T option) where T : PluginOptionValue;
}
