// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.

using Microsoft.Performance.SDK.Runtime.ColumnVariants.TreeNodes.Visitors;

namespace Microsoft.Performance.SDK.Runtime.ColumnVariants.TreeNodes;

/// <summary>
///     Represents a null column variant.
/// </summary>
public sealed class NullColumnVariantsTreeNode
    : IColumnVariantsTreeNode
{
    private NullColumnVariantsTreeNode()
    {
    }

    /// <summary>
    ///     Gets the singleton instance of <see cref="NullColumnVariantsTreeNode" />.
    /// </summary>
    public static NullColumnVariantsTreeNode Instance { get; } = new();

    /// <inheritdoc />
    public bool IsEquivalentTree(IColumnVariantsTreeNode other)
    {
        return ReferenceEquals(this, other) || (other is NullColumnVariantsTreeNode otherT && IsEquivalentTree(otherT));
    }

    /// <inheritdoc />
    public void Accept(IColumnVariantsTreeNodesVisitor treeNodesVisitor)
    {
        treeNodesVisitor.Visit(this);
    }

    private bool IsEquivalentTree(NullColumnVariantsTreeNode _)
    {
        return true;
    }
}