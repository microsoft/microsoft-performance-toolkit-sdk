// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.

namespace Microsoft.Performance.SDK.Runtime.ColumnVariants;

/// <summary>
///     Represents a null column variant.
/// </summary>
public sealed class NullColumnVariant
    : IColumnVariant
{
    private NullColumnVariant()
    {
    }

    /// <summary>
    ///     Gets the singleton instance of <see cref="NullColumnVariant"/>.
    /// </summary>
    public static NullColumnVariant Instance { get; } = new();

    private bool Equals(NullColumnVariant other)
    {
        return true;
    }

    /// <inheritdoc/>
    public override bool Equals(object obj)
    {
        return ReferenceEquals(this, obj) || obj is NullColumnVariant other && Equals(other);
    }

    /// <inheritdoc/>
    public override int GetHashCode()
    {
        return 42;
    }

    /// <inheritdoc/>
    public bool Equals(IColumnVariant other)
    {
        return ReferenceEquals(this, other) || other is NullColumnVariant otherT && Equals(otherT);
    }

    /// <inheritdoc/>
    public void Accept(IColumnVariantsVisitor visitor)
    {
        visitor.Visit(this);
    }
}