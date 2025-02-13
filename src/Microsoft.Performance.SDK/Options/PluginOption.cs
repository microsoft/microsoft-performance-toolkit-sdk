// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.

using System;

namespace Microsoft.Performance.SDK.Options;

/// <summary>
///     Base class for plugin options.
/// </summary>
public abstract class PluginOption
    : ICloneable<PluginOption>
{
    /// <summary>
    ///     Raised when this option's value changes.
    /// </summary>
    public event EventHandler OptionChanged;

    /// <summary>
    ///     Initializes a new instance of the <see cref="PluginOption"/> class.
    /// </summary>
    private protected  PluginOption()
    {
    }

    /// <summary>
    ///     Gets or initializes the <see cref="Guid"/> of the option. This value is required to be
    ///     initialized.
    /// </summary>
    public required Guid Guid { get; init; }

    /// <summary>
    ///     Gets or initializes the human-readable category of the option. This value is required to be initialized.
    /// </summary>
    public required string Category { get; init; }

    /// <summary>
    ///     Gets or initializes the human-readable name of the option. This value is required to be initialized.
    /// </summary>
    public required string Name { get; init;  }

    /// <summary>
    ///     Gets or initializes the human-readable description of the option. This value is required to be initialized.
    /// </summary>
    public required string Description { get; init; }

    /// <summary>
    ///     Gets a value indicating whether this option is using its default value. This value will be <c>true</c>
    ///     if the option's value is manually set outside of <see cref="ApplyDefault"/>, regardless of which
    ///     value it was set to. This means that this value will be <c>false</c> even if the option's value is
    ///     manually set to its default value.
    /// </summary>
    public bool IsUsingDefault { get; private protected set; }

    /// <summary>
    ///     Applies the default value to this option.
    /// </summary>
    public abstract void ApplyDefault();

    /// <summary>
    ///     Raises the <see cref="OptionChanged"/> event.
    /// </summary>
    protected void RaiseOptionChanged()
    {
        try
        {
            this.OptionChanged?.Invoke(this, EventArgs.Empty);
        }
        catch
        {
            // We catch all exceptions here because we don't want to crash the app just because
            // a plugin is throwing exceptions. The SDK's state is not impacted by an exception here,
            // since all that matters (the plugin's option value has been updated to the new value)
            // isn't affected. A badly designed plugin may not in an unknown state here, but that's not
            // the SDK's responsibility to address.

            return;
        }
    }

    /// <inheritdoc />
    public abstract PluginOption CloneT();

    /// <inheritdoc />
    public object Clone()
    {
        return CloneT();
    }

    public override string ToString()
    {
        return $"{this.Name} ({this.Guid})";
    }
}
