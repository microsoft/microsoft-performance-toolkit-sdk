// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.

using Microsoft.Performance.SDK.Processing;
using Microsoft.Performance.SDK.Processing.ColumnBuilding;

namespace Microsoft.Performance.SDK.Runtime.ColumnVariants;

/// <summary>
///     Represents a column variant that can be toggled on or off.
/// </summary>
public sealed class ToggleableColumnVariant
    : IColumnVariant
{
    public ToggleableColumnVariant(
        ColumnVariantIdentifier identifier,
        IDataColumn toggledColumn,
        IColumnVariant subVariant)
    {
        Identifier = identifier;
        SubVariant = subVariant;
        ToggledColumn = toggledColumn;
    }

    /// <summary>
    ///     Gets the identifier for the column variant.
    /// </summary>
    public ColumnVariantIdentifier Identifier { get; }

    /// <summary>
    ///     Gets the sub-variant that represents nested variants of this mode.
    /// </summary>
    public IColumnVariant SubVariant { get; }

    /// <summary>
    ///     Gets the column that the toggled on state represents.
    /// </summary>
    public IDataColumn ToggledColumn { get; }

    /// <inheritdoc/>
    public void Accept(IColumnVariantsVisitor visitor)
    {
        visitor.Visit(this);
    }

    private bool Equals(ToggleableColumnVariant other)
    {
        return this.Identifier.Equals(other.Identifier)
               && Equals(SubVariant, other.SubVariant);
    }

    /// <inheritdoc/>
    public bool Equals(IColumnVariant other)
    {
        return ReferenceEquals(this, other) || other is ToggleableColumnVariant otherT && Equals(otherT);
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
            this.Identifier?.GetHashCode() ?? 0,
            this.SubVariant?.GetHashCode() ?? 0);
    }
}