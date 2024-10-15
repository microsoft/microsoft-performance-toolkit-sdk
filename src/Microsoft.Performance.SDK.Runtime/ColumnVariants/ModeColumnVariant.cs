// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.

using Microsoft.Performance.SDK.Processing;
using Microsoft.Performance.SDK.Processing.ColumnBuilding;

namespace Microsoft.Performance.SDK.Runtime.ColumnVariants;

/// <summary>
///     Represents a column variant that is one of many mutually exclusive modes.
/// </summary>
public sealed class ModeColumnVariant
    : IColumnVariant
{
    public ModeColumnVariant(
        ColumnVariantIdentifier modeIdentifier,
        IDataColumn modeColumn,
        IColumnVariant subVariant)
    {
        ModeIdentifier = modeIdentifier;
        ModeColumn = modeColumn;
        SubVariant = subVariant;
    }

    /// <summary>
    ///     Gets the identifier for the mode.
    /// </summary>
    public ColumnVariantIdentifier ModeIdentifier { get; }

    /// <summary>
    ///     Gets the column that this mode represents.
    /// </summary>
    public IDataColumn ModeColumn { get; }

    /// <summary>
    ///     Gets the sub-variant that represents nested variants of this mode.
    /// </summary>
    public IColumnVariant SubVariant { get; }

    /// <inheritdoc/>
    public void Accept(IColumnVariantsVisitor visitor)
    {
        visitor.Visit(this);
    }

    private bool Equals(ModeColumnVariant other)
    {
        return this.ModeIdentifier.Equals(other.ModeIdentifier)
               && Equals(SubVariant, other.SubVariant);
    }

    /// <inheritdoc/>
    public bool Equals(IColumnVariant other)
    {
        return ReferenceEquals(this, other) || other is ModeColumnVariant otherT && Equals(otherT);
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
            this.ModeIdentifier?.GetHashCode() ?? 0,
            this.SubVariant?.GetHashCode() ?? 0);
    }
}