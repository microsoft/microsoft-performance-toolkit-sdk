// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.

using System;
using Microsoft.Performance.SDK.Options;

namespace Microsoft.Performance.SDK.Runtime.Options.Serialization.DTO;

/// <summary>
///     Base class for <see cref="PluginOption"/> DTOs.
/// </summary>
public abstract class PluginOptionDto
{
    public Guid Guid { get; init; } = Guid.Empty;

    public bool IsDefault { get; init; } = true;
}