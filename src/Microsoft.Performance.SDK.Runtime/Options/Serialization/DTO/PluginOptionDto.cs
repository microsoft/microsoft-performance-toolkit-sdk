// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.

using System;

namespace Microsoft.Performance.SDK.Runtime.Options.Serialization.DTO;

public abstract class PluginOptionDto
{
    public Guid Guid { get; init; } = Guid.Empty;

    public bool IsDefault { get; init; } = true;
}