// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.

using Microsoft.Performance.SDK.Options.Definitions;
using Microsoft.Performance.SDK.Options.Values;

namespace Microsoft.Performance.SDK.Runtime.Options;

/// <summary>
///     A <see cref="PluginOption"/> for a single string field.
/// </summary>
public sealed class FieldOption
    : PluginOption<string, FieldOptionDefinition, FieldOptionValue>
{
    public FieldOption(FieldOptionDefinition definition, FieldOptionValue value, bool isUsingDefault)
        : base(definition, value, isUsingDefault)
    {
    }
}
