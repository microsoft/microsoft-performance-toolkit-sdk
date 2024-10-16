// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.

using Microsoft.Performance.SDK.Processing.ColumnBuilding;

namespace Microsoft.Performance.SDK.Runtime.ColumnVariants;

/// <summary>
///     Represents a node in the tree which exposes all variants of a column.
///     Concrete column variants are exposed within a tree of concrete <see cref="IColumnVariantsTreeNode"/> types
///     that each have their own set of properties and children. To traverse the tree, create
///     an <see cref="IColumnVariantsTreeNodesVisitor"/> and call <see cref="Accept"/> on the root
///     <see cref="IColumnVariantsTreeNode"/> within the tree.
/// </summary>
public interface IColumnVariantsTreeNode
{
    /// <summary>
    ///     Accepts the given visitor.
    /// </summary>
    /// <param name="treeNodesVisitor">
    ///     The visitor to accept.
    /// </param>
    void Accept(IColumnVariantsTreeNodesVisitor treeNodesVisitor);

    /// <summary>
    ///     Determines if the given tree rooted at <paramref name="other"/> is equivalent to this one.
    ///     Trees are equivalent if they have the same structure and expose equivalent column variants,
    ///     where exposed variants are considered equal if their <see cref="ColumnVariantIdentifier"/>s
    ///     are equal. The concrete projections/data columns are not compared.
    /// </summary>
    /// <param name="other"></param>
    /// <returns></returns>
    bool IsEquivalentTree(IColumnVariantsTreeNode other);
}