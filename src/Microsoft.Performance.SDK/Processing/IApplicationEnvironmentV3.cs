// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.

using System;
using Microsoft.Performance.SDK.Options;

namespace Microsoft.Performance.SDK.Processing;

/// <summary>
///     Extends <see cref="IApplicationEnvironmentV2"/> to provide additional functionality.
/// </summary>
[Obsolete("This interface will be removed in version 2.0 of the SDK. It is OK to use this interface in version 1.x of the SDK.")]
public interface IApplicationEnvironmentV3
    : IApplicationEnvironmentV2
{
    bool TryGetPluginOption<T>(Guid optionGuid, out T option) where T : PluginOption;
}