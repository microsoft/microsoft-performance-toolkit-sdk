// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.

using Microsoft.Performance.SDK.Processing;
using Microsoft.Performance.SDK.Processing.ColumnBuilding;
using Microsoft.Performance.SDK.Runtime.ColumnVariants.TreeNodes.Visitors;

namespace Microsoft.Performance.SDK.Runtime.ColumnVariants.TreeNodes;

/// <summary>
///     Represents a column variant that is one of many mutually exclusive modes.
/// </summary>
public sealed class ModeColumnVariantsTreeNode
    : IColumnVariantsTreeNode
{
    public ModeColumnVariantsTreeNode(
        ColumnVariantIdentifier modeIdentifier,
        IDataColumn modeColumn,
        IColumnVariantsTreeNode subVariantsTreeNode)
    {
        ModeIdentifier = modeIdentifier;
        ModeColumn = modeColumn;
        SubVariantsTreeNode = subVariantsTreeNode;
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
    public IColumnVariantsTreeNode SubVariantsTreeNode { get; }

    /// <inheritdoc />
    public void Accept(IColumnVariantsTreeNodesVisitor treeNodesVisitor)
    {
        treeNodesVisitor.Visit(this);
    }

    /// <inheritdoc />
    public bool IsEquivalentTree(IColumnVariantsTreeNode other)
    {
        return ReferenceEquals(this, other) || (other is ModeColumnVariantsTreeNode otherT && IsEquivalentTree(otherT));
    }

    private bool IsEquivalentTree(ModeColumnVariantsTreeNode other)
    {
        return ModeIdentifier.Equals(other.ModeIdentifier)
               && this.SubVariantsTreeNode.IsEquivalentTree(other.SubVariantsTreeNode);
    }
}