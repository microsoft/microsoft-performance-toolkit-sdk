// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.

using System.Collections.Generic;
using System.Linq;

namespace Microsoft.Performance.SDK.Runtime.ColumnVariants;

/// <summary>
///     Exposes a set of mutually exclusive <see cref="IColumnVariant"/> modes.
///     This <see cref="IColumnVariant"/> has no associated projection.
/// </summary>
public sealed class ModesColumnVariant
    : IColumnVariant
{
    public ModesColumnVariant(
        IReadOnlyCollection<IColumnVariant> modes,
        int defaultModeIndex)
    {
        Modes = modes;
        DefaultModeIndex = defaultModeIndex;
    }

    /// <summary>
    ///     Gets the set of mutually exclsuive modes.
    /// </summary>
    public IReadOnlyCollection<IColumnVariant> Modes { get; }

    /// <summary>
    ///     Gets the index within <see cref="Modes"/> of the default mode.
    /// </summary>
    public int DefaultModeIndex { get; }

    /// <inheritdoc/>
    public void Accept(IColumnVariantsVisitor visitor)
    {
        visitor.Visit(this);
    }

    private bool Equals(ModesColumnVariant other)
    {
        if (this.Modes == null)
        {
            return other.Modes == null;
        }

        if (other.Modes == null)
        {
            return false;
        }

        return Enumerable.SequenceEqual(this.Modes, other.Modes) &&
               this.DefaultModeIndex == other.DefaultModeIndex;
    }

    /// <inheritdoc/>
    public bool Equals(IColumnVariant other)
    {
        return ReferenceEquals(this, other) || other is ModesColumnVariant otherT && Equals(otherT);
    }

    /// <inheritdoc/>
    public override bool Equals(object obj)
    {
        return ReferenceEquals(this, obj) || obj is IColumnVariant other && Equals(other);
    }

    /// <inheritdoc/>
    public override int GetHashCode()
    {
        int result = this.DefaultModeIndex.GetHashCode();

        if (this.Modes != null)
        {
            foreach (IColumnVariant mode in this.Modes)
            {
                result = HashCodeUtils.CombineHashCodeValues(result, mode?.GetHashCode() ?? 0);
            }
        }

        return result;
    }
}