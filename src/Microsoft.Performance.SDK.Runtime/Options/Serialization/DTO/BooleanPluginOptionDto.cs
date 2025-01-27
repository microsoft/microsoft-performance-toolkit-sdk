// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.

namespace Microsoft.Performance.SDK.Runtime.Options.Serialization.DTO;

public sealed class BooleanPluginOptionDto
    : PluginOptionDto
{
    public bool Value { get; init; } = false;
}