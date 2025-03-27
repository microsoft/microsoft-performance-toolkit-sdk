// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.

using System;
using Microsoft.Performance.SDK.Options.Definitions;
using Microsoft.Performance.SDK.Options.Values;

namespace Microsoft.Performance.SDK.Runtime.Options;

/// <summary>
///     Base class for plugin options.
/// </summary>
/// <typeparam name="T">
///     The type of the option value.
/// </typeparam>
/// <typeparam name="TDef">
///     The type of the <see cref="PluginOptionDefinition"/> that defined
///     this option.
/// </typeparam>
/// <typeparam name="TValue">
///     The type of the <see cref="PluginOptionValue"/> that holds the option's
///     current value.
/// </typeparam>
public abstract class PluginOption<T, TDef, TValue>
    : PluginOption
    where TDef : PluginOptionDefinition<T>
    where TValue : PluginOptionValue<T>
{
    protected PluginOption(TDef definition, TValue value, bool isUsingDefault)
    {
        this.Definition = definition;
        this.Value = value;
        this.IsUsingDefault = isUsingDefault;
    }

    /// <inheritdoc />
    public override Guid Guid => this.Definition.Guid;

    /// <summary>
    ///     Gets the <see cref="PluginOptionDefinition{T}"/> that defined this option.
    /// </summary>
    public TDef Definition
    {
        get;
    }

    /// <summary>
    ///     Gets the <see cref="PluginOptionValue"/> that holds the option's current value.
    /// </summary>
    public TValue Value
    {
        get;
    }

    /// <inheritdoc />
    public override void ApplyDefault()
    {
        UpdateValue(this.Definition.DefaultValue, true);
    }

    /// <summary>
    ///     Sets the value of this option to the given value. This will cause
    ///     <see cref="PluginOption.IsUsingDefault"/> to be <c>false</c>, regardless
    ///     of the value of <paramref name="value"/>.
    /// </summary>
    /// <param name="value">
    ///     The value to set this option to.
    /// </param>
    public void SetValue(T value)
    {
        UpdateValue(value, false);
    }

    private void UpdateValue(T value, bool isDefault)
    {
        this.Value.CurrentValue = value;
        this.IsUsingDefault = isDefault;
    }
}
