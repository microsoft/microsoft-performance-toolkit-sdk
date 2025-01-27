// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.

namespace Microsoft.Performance.SDK.Runtime.Options.Serialization.DTO;

public sealed class FieldPluginOptionDto
    : PluginOptionDto
{
    public string Value { get; init; } = string.Empty;
}