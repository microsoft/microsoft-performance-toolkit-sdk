// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.

using System.Collections.Generic;

namespace Microsoft.Performance.SDK.Runtime.Options.Serialization.DTO;

/// <summary>
///     A DTO for serializing and deserializing plugin options.
/// </summary>
public sealed class PluginOptionsDto
{
    /// <summary>
    ///     Gets or initializes the <see cref="BooleanPluginOptionDto"/> instances.
    /// </summary>
    public IReadOnlyCollection<BooleanPluginOptionDto> BooleanOptions { get; init; } = new List<BooleanPluginOptionDto>();

    /// <summary>
    ///     Gets or initializes the <see cref="FieldPluginOptionDto"/> instances.
    /// </summary>
    public IReadOnlyCollection<FieldPluginOptionDto> FieldOptions { get; init; } = new List<FieldPluginOptionDto>();

    /// <summary>
    ///     Gets or initializes the <see cref="FieldArrayPluginOptionDto"/> instances.
    /// </summary>
    public IReadOnlyCollection<FieldArrayPluginOptionDto> FieldArrayOptions { get; init; } = new List<FieldArrayPluginOptionDto>();
}