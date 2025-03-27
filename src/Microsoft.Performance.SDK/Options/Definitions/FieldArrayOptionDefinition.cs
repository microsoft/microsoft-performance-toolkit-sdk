// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.

using System.Collections.Generic;
using Microsoft.Performance.SDK.Options.Values;

namespace Microsoft.Performance.SDK.Options.Definitions;

/// <summary>
///     A <see cref="PluginOptionDefinition{T}"/> for an ordered collection of string fields.
/// </summary>
/// <remarks>
///     This <see cref="PluginOptionDefinition"/> will have values exposed
///     by a <see cref="FieldArrayOptionValue"/>.
/// </remarks>
public sealed class FieldArrayOptionDefinition
    : PluginOptionDefinition<IReadOnlyList<string>>
{
}
