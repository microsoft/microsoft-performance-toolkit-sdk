// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.

using Microsoft.Performance.SDK.Options.Definitions;
using Microsoft.Performance.SDK.Options.Values;

namespace Microsoft.Performance.SDK.Runtime.Options;

/// <summary>
///     A <see cref="PluginOption"/> for a boolean value.
/// </summary>
public sealed class BooleanOption
    : PluginOption<bool, BooleanOptionDefinition, BooleanOptionValue>
{
    public BooleanOption(
        BooleanOptionDefinition definition,
        BooleanOptionValue value,
        bool isUsingDefault)
        : base(definition, value, isUsingDefault)
    {
    }
}
