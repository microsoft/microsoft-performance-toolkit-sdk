// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.

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
