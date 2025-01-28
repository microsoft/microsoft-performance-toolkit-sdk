// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.

using Microsoft.Performance.SDK.Options;

namespace Microsoft.Performance.SDK.Runtime.Options.Serialization.DTO;

/// <summary>
///     Base class for <see cref="PluginOption"/> DTOs.
/// </summary>
public abstract class PluginOptionDto<T>
    : PluginOptionDto
{
    /// <summary>
    ///     Gets or initializes the value of the option.
    /// </summary>
    public T Value { get; init; }
}