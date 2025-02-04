// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.

using Microsoft.Performance.SDK.Options;

namespace Microsoft.Performance.SDK.Runtime.Options.Serialization.DTO;

/// <summary>
///     A DTO for <see cref="FieldArrayOption"/>.
/// </summary>
public sealed class FieldArrayPluginOptionDto
    : PluginOptionDto<string[]>
{
}