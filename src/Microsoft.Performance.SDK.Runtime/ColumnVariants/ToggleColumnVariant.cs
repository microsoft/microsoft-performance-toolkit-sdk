// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.

using Microsoft.Performance.SDK.Processing;
using Microsoft.Performance.SDK.Processing.ColumnBuilding;

namespace Microsoft.Performance.SDK.Runtime.ColumnVariants;

/// <summary>
///     Represents a column variant that can be toggled on or off.
/// </summary>
public sealed class ToggleableColumnVariantsTreeNode
    : IColumnVariantsTreeNode
{
    public ToggleableColumnVariantsTreeNode(
        ColumnVariantIdentifier identifier,
        IDataColumn toggledColumn,
        IColumnVariantsTreeNode subVariantsTreeNode)
    {
        Identifier = identifier;
        SubVariantsTreeNode = subVariantsTreeNode;
        ToggledColumn = toggledColumn;
    }

    /// <summary>
    ///     Gets the identifier for the column variant.
    /// </summary>
    public ColumnVariantIdentifier Identifier { get; }

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
        return Identifier.Equals(other.Identifier)
               && this.SubVariantsTreeNode.IsEquivalentTree(other.SubVariantsTreeNode);
    }
}