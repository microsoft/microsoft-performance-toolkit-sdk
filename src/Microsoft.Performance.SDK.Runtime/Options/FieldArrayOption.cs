// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.

using System.Collections.Generic;
using Microsoft.Performance.SDK.Options.Definitions;
using Microsoft.Performance.SDK.Options.Values;

namespace Microsoft.Performance.SDK.Runtime.Options;

/// <summary>
///     A <see cref="PluginOption"/> for an ordered collection of string fields.
/// </summary>
public sealed class FieldArrayOption
    : PluginOption<IReadOnlyList<string>, FieldArrayOptionDefinition, FieldArrayOptionValue>
{
    public FieldArrayOption(FieldArrayOptionDefinition definition, FieldArrayOptionValue value, bool isUsingDefault)
        : base(definition, value, isUsingDefault)
    {
    }
}
