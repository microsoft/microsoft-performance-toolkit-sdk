// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.

using Microsoft.Performance.SDK.Options.Definitions;

namespace Microsoft.Performance.SDK.Options.Values;

/// <summary>
///     Base class for plugin option values.
/// </summary>
/// <typeparam name="T">
///     The type of the option value.
/// </typeparam>
public abstract class PluginOptionValue<T>
    : PluginOptionValue
{
    private T currentValue;

    /// <summary>
    ///     Initializes a new instance of the <see cref="PluginOptionValue"/> class.
    /// </summary>
    private protected PluginOptionValue()
    {
    }

    /// <summary>
    ///     Gets the current value of the option.
    /// </summary>
    /// <remarks>
    ///     Note that this value may be equivalent to the <see cref="PluginOptionDefinition{T}.DefaultValue"/> defined
    ///     for this plugin option regardless of whether the value has been modified/manually set. Do not use the value
    ///     of this property to determine whether the option is using its default value.
    /// </remarks>
    public T CurrentValue
    {
        get => this.currentValue;
        internal set
        {
            this.currentValue = value;
            InvokeOptionChanged();
        }
    }
}
