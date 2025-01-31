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
    internal PluginOption()
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
    ///     value it was set to. This means that this value will be <c>true</c> even if the option's value is
    ///     manually set to its default value.
    /// </summary>
    internal bool IsUsingDefault { get; private protected set; }

    /// <summary>
    ///     Applies the default value to this option.
    /// </summary>
    internal abstract void ApplyDefault();

    /// <summary>
    ///     Raises the <see cref="OptionChanged"/> event.
    /// </summary>
    protected void RaiseOptionChanged()
    {
        this.OptionChanged?.Invoke(this, EventArgs.Empty);
    }

    /// <inheritdoc />
    public abstract PluginOption CloneT();

    /// <inheritdoc />
    public object Clone()
    {
        return CloneT();
    }
}