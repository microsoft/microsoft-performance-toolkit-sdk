// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.

using System.Collections.Generic;

namespace Microsoft.Performance.SDK.Runtime.Options.Serialization.DTO;

public sealed class PluginOptionsDto
{
    public IReadOnlyCollection<BooleanPluginOptionDto> BooleanOptions { get; init; } = new List<BooleanPluginOptionDto>();

    public IReadOnlyCollection<FieldPluginOptionDto> FieldOptions { get; init; } = new List<FieldPluginOptionDto>();

    public IReadOnlyCollection<FieldArrayPluginOptionDto> FieldArrayOptions { get; init; } = new List<FieldArrayPluginOptionDto>();
}