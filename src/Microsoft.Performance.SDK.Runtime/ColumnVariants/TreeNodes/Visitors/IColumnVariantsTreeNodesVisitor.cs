// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.

namespace Microsoft.Performance.SDK.Runtime.ColumnVariants.TreeNodes.Visitors;

/// <summary>
///     Represents a visitor for concrete <see cref="IColumnVariantsTreeNode"/> objects.
/// </summary>
public interface IColumnVariantsTreeNodesVisitor
{
    /// <summary>
    ///     Called when visiting a <see cref="NullColumnVariantsTreeNode"/>.
    /// </summary>
    /// <param name="nullColumnVariantsTreeNode">
    ///     The <see cref="NullColumnVariantsTreeNode"/> being visited.
    /// </param>
    void Visit(NullColumnVariantsTreeNode nullColumnVariantsTreeNode);

    /// <summary>
    ///     Called when visiting a <see cref="ToggleableColumnVariantsTreeNode"/>.
    /// </summary>
    /// <param name="toggleableColumnVariantsTreeNode">
    ///     The <see cref="ToggleableColumnVariantsTreeNode"/> being visited.
    /// </param>
    void Visit(ToggleableColumnVariantsTreeNode toggleableColumnVariantsTreeNode);

    /// <summary>
    ///     Called when visiting a <see cref="ModesToggleColumnVariantsTreeNode"/>.
    /// </summary>
    /// <param name="modesToggle">
    ///     The <see cref="ModesToggleColumnVariantsTreeNode"/> being visited.
    /// </param>
    void Visit(ModesToggleColumnVariantsTreeNode modesToggle);

    /// <summary>
    ///     Called when visiting a <see cref="ModesColumnVariantsTreeNode"/>.
    /// </summary>
    /// <param name="modesColumnVariantsTreeNode">
    ///     The <see cref="ModesColumnVariantsTreeNode"/> being visited.
    /// </param>
    void Visit(ModesColumnVariantsTreeNode modesColumnVariantsTreeNode);

    /// <summary>
    ///     Called when visiting a <see cref="ModeColumnVariantsTreeNode"/>.
    /// </summary>
    /// <param name="modeColumnVariantsTreeNode">
    ///     The <see cref="ModeColumnVariantsTreeNode"/> being visited.
    /// </param>
    void Visit(ModeColumnVariantsTreeNode modeColumnVariantsTreeNode);

}