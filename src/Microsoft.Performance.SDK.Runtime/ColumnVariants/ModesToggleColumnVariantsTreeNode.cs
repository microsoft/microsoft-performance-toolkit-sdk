// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.

namespace Microsoft.Performance.SDK.Runtime.ColumnVariants;

/// <summary>
///     Exposes a toggle for enabling/disabling a <see cref="IColumnVariantsTreeNode" />.
///     This <see cref="IColumnVariantsTreeNode" /> has no associated projection.
/// </summary>
public sealed class ModesToggleColumnVariantsTreeNode
    : IColumnVariantsTreeNode
{
    public ModesToggleColumnVariantsTreeNode(string toggleText, IColumnVariantsTreeNode modes)
    {
        ToggleText = toggleText;
        Modes = modes;
    }

    /// <summary>
    ///     Gets the text to display for the toggle.
    /// </summary>
    public string ToggleText { get; }

    /// <summary>
    ///     Gets the <see cref="IColumnVariantsTreeNode" /> to use when the toggle is enabled.
    /// </summary>
    public IColumnVariantsTreeNode Modes { get; }

    /// <inheritdoc />
    public void Accept(IColumnVariantsTreeNodesVisitor treeNodesVisitor)
    {
        treeNodesVisitor.Visit(this);
    }

    /// <inheritdoc />
    public bool IsEquivalentTree(IColumnVariantsTreeNode other)
    {
        return ReferenceEquals(this, other) ||
               (other is ModesToggleColumnVariantsTreeNode otherT && IsEquivalentTree(otherT));
    }

    private bool IsEquivalentTree(ModesToggleColumnVariantsTreeNode other)
    {
        return ToggleText == other.ToggleText && this.Modes.IsEquivalentTree(other.Modes);
    }
}