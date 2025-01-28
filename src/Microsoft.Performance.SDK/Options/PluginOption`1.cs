// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.

namespace Microsoft.Performance.SDK.Options;

/// <summary>
///     Base class for plugin options.
/// </summary>
/// <typeparam name="T">
///     The type of the option value.
/// </typeparam>
public abstract class PluginOption<T>
    : PluginOption
{
    private readonly T defaultValue;

    private bool isApplyingDefault;

    private T currentValue;

    /// <summary>
    ///     Initializes a new instance of the <see cref="PluginOption{T}"/> class.
    /// </summary>
    internal PluginOption()
    {
        this.IsUsingDefault = true;
    }

    /// <summary>
    ///     Gets or initializes the default value of the option. This value is required to be initialized.
    /// </summary>
    public required T DefaultValue
    {
        get
        {
            return this.defaultValue;
        }
        init
        {
            this.defaultValue = value;
            this.currentValue = value;
        }
    }

    /// <summary>
    ///     Gets the current value of the option.
    /// </summary>
    public T CurrentValue
    {
        get
        {
            return this.currentValue;
        }
        internal set
        {
            this.IsUsingDefault = isApplyingDefault;

            this.currentValue = value;
            RaiseOptionChanged();
        }
    }

    /// <inheritdoc />
    internal override void ApplyDefault()
    {
        this.isApplyingDefault = true;
        this.CurrentValue = this.DefaultValue;
        this.isApplyingDefault = false;
    }
}