// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.

namespace Microsoft.Performance.SDK.Runtime.ColumnVariants;

/// <summary>
///     Exposes a toggle for enabling/disabling a <see cref="IColumnVariant"/>.
///     This <see cref="IColumnVariant"/> has no associated projection.
/// </summary>
public sealed class ModesToggleColumnVariant
    : IColumnVariant
{
    public ModesToggleColumnVariant(string toggleText, IColumnVariant modes)
    {
        ToggleText = toggleText;
        Modes = modes;
    }

    /// <summary>
    ///     Gets the text to display for the toggle.
    /// </summary>
    public string ToggleText { get; }

    /// <summary>
    ///     Gets the <see cref="IColumnVariant"/> to use when the toggle is enabled.
    /// </summary>
    public IColumnVariant Modes { get; }

    /// <inheritdoc/>
    public void Accept(IColumnVariantsVisitor visitor)
    {
        visitor.Visit(this);
    }

    private bool Equals(ModesToggleColumnVariant other)
    {
        return ToggleText == other.ToggleText && Equals(Modes, other.Modes);
    }

    /// <inheritdoc/>
    public bool Equals(IColumnVariant other)
    {
        return ReferenceEquals(this, other) || other is ModesToggleColumnVariant otherT && Equals(otherT);
    }

    /// <inheritdoc/>
    public override bool Equals(object obj)
    {
        return ReferenceEquals(this, obj) || obj is IColumnVariant other && Equals(other);
    }

    /// <inheritdoc/>
    public override int GetHashCode()
    {
        return HashCodeUtils.CombineHashCodeValues(
            this.ToggleText?.GetHashCode() ?? 0,
            this.Modes?.GetHashCode() ?? 0);
    }
}