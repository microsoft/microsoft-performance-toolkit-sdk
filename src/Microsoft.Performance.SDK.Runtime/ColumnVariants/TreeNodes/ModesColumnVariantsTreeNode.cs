// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.

using System.Collections.Generic;
using System.Linq;
using Microsoft.Performance.SDK.Runtime.ColumnVariants.TreeNodes.Visitors;

namespace Microsoft.Performance.SDK.Runtime.ColumnVariants.TreeNodes;

/// <summary>
///     Exposes a set of mutually exclusive <see cref="IColumnVariantsTreeNode" /> modes.
///     This <see cref="IColumnVariantsTreeNode" /> has no associated projection.
/// </summary>
public sealed class ModesColumnVariantsTreeNode
    : IColumnVariantsTreeNode
{
    public ModesColumnVariantsTreeNode(
        IReadOnlyCollection<IColumnVariantsTreeNode> modes,
        int defaultModeIndex)
    {
        Modes = modes;
        DefaultModeIndex = defaultModeIndex;
    }

    /// <summary>
    ///     Gets the set of mutually exclsuive modes.
    /// </summary>
    public IReadOnlyCollection<IColumnVariantsTreeNode> Modes { get; }

    /// <summary>
    ///     Gets the index within <see cref="Modes" /> of the default mode.
    /// </summary>
    public int DefaultModeIndex { get; }

    /// <inheritdoc />
    public void Accept(IColumnVariantsTreeNodesVisitor treeNodesVisitor)
    {
        treeNodesVisitor.Visit(this);
    }

    /// <inheritdoc />
    public bool IsEquivalentTree(IColumnVariantsTreeNode other)
    {
        return ReferenceEquals(this, other) || (other is ModesColumnVariantsTreeNode otherT && IsEquivalentTree(otherT));
    }

    private bool IsEquivalentTree(ModesColumnVariantsTreeNode other)
    {
        if (Modes == null)
        {
            return other.Modes == null;
        }

        if (other.Modes == null)
        {
            return false;
        }

        return this.Modes.Count == other.Modes.Count &&
               this.Modes
                    .Zip(other.Modes, (a, b) => (a, b))
                    .All(pair => pair.a.IsEquivalentTree(pair.b)) &&
               DefaultModeIndex == other.DefaultModeIndex;
    }
}