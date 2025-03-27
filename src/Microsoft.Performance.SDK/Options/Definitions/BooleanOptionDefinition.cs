// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.

using Microsoft.Performance.SDK.Options.Values;

namespace Microsoft.Performance.SDK.Options.Definitions;

/// <summary>
///     A <see cref="PluginOptionDefinition{T}"/> for a boolean value.
/// </summary>
/// <remarks>
///     This <see cref="PluginOptionDefinition"/> will have values exposed
///     by a <see cref="BooleanOptionValue"/>.
/// </remarks>
public sealed class BooleanOptionDefinition
    : PluginOptionDefinition<bool>
{
}
