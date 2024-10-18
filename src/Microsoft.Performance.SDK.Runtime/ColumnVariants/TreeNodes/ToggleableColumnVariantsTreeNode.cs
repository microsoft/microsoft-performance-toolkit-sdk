// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.

using Microsoft.Performance.SDK.Processing;
using Microsoft.Performance.SDK.Processing.ColumnBuilding;
using Microsoft.Performance.SDK.Runtime.ColumnVariants.TreeNodes.Visitors;

namespace Microsoft.Performance.SDK.Runtime.ColumnVariants.TreeNodes;

/// <summary>
///     Represents a column variant that can be toggled on or off.
/// </summary>
public sealed class ToggleableColumnVariantsTreeNode
    : IColumnVariantsTreeNode
{
    public ToggleableColumnVariantsTreeNode(
        ColumnVariantDescriptor descriptor,
        IDataColumn toggledColumn,
        IColumnVariantsTreeNode subVariantsTreeNode)
    {
        Descriptor = descriptor;
        SubVariantsTreeNode = subVariantsTreeNode;
        ToggledColumn = toggledColumn;
    }

    /// <summary>
    ///     Gets the identifier for the column variant.
    /// </summary>
    public ColumnVariantDescriptor Descriptor { get; }

    /// <summary>
    ///     Gets the sub-variant that represents nested variants of this mode.
    /// </summary>
    public IColumnVariantsTreeNode SubVariantsTreeNode { get; }

    /// <summary>
    ///     Gets the column that the toggled on state represents.
    /// </summary>
    public IDataColumn ToggledColumn { get; }

    /// <inheritdoc />
    public void Accept(IColumnVariantsTreeNodesVisitor treeNodesVisitor)
    {
        treeNodesVisitor.Visit(this);
    }

    /// <inheritdoc />
    public bool IsEquivalentTree(IColumnVariantsTreeNode other)
    {
        return ReferenceEquals(this, other) ||
               (other is ToggleableColumnVariantsTreeNode otherT && IsEquivalentTree(otherT));
    }

    private bool IsEquivalentTree(ToggleableColumnVariantsTreeNode other)
    {
        return Descriptor.Equals(other.Descriptor)
               && this.SubVariantsTreeNode.IsEquivalentTree(other.SubVariantsTreeNode);
    }
}