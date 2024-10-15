// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.

using System;

namespace Microsoft.Performance.SDK.Runtime.ColumnVariants;

/// <summary>
///     Represents a column variant.
///     Column variants exist within a tree of different types of column variants, each with
///     their own set of properties and children. To traverse the tree, create
///     an <see cref="IColumnVariantsVisitor"/> and call <see cref="Accept"/> on the root
///     <see cref="IColumnVariant"/> within the tree.
/// </summary>
public interface IColumnVariant
    : IEquatable<IColumnVariant>
{
    /// <summary>
    ///     Accepts the given visitor.
    /// </summary>
    /// <param name="visitor">
    ///     The visitor to accept.
    /// </param>
    void Accept(IColumnVariantsVisitor visitor);
}