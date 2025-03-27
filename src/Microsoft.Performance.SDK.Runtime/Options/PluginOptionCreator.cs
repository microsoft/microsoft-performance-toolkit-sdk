// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.

using Microsoft.Performance.SDK.Options.Definitions;
using Microsoft.Performance.SDK.Options.Values;

namespace Microsoft.Performance.SDK.Runtime.Options;

/// <summary>
///     Creates <see cref="PluginOption"/> instances based on
///     <see cref="PluginOptionDefinition"/>s.
/// </summary>
internal class PluginOptionCreator
    : IPluginOptionDefinitionVisitor
{
    /// <summary>
    ///     Gets the last created <see cref="PluginOption"/> based on the last
    ///     <see cref="PluginOptionDefinition"/> that was visited.
    /// </summary>
    public PluginOption LastCreatedOption { get; private set; }

    public void Visit(BooleanOptionDefinition option)
    {
        this.LastCreatedOption = new BooleanOption(option, new BooleanOptionValue() { CurrentValue = option.DefaultValue }, true);
    }

    public void Visit(FieldOptionDefinition option)
    {
        this.LastCreatedOption = new FieldOption(option, new FieldOptionValue() { CurrentValue = option.DefaultValue }, true);
    }

    public void Visit(FieldArrayOptionDefinition option)
    {
        this.LastCreatedOption = new FieldArrayOption(option, new FieldArrayOptionValue() { CurrentValue = option.DefaultValue }, true);
    }
}
