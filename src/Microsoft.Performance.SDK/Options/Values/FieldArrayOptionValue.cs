// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.

using System.Collections.Generic;

namespace Microsoft.Performance.SDK.Options.Values;

/// <summary>
///     A <see cref="PluginOptionValue{T}"/> for an ordered collection of string fields.
/// </summary>
public sealed class FieldArrayOptionValue
    : PluginOptionValue<IReadOnlyList<string>>
{
}
