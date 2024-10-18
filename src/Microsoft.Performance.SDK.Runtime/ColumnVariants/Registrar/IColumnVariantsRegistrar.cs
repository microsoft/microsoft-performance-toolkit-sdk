// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.

using Microsoft.Performance.SDK.Processing;
using Microsoft.Performance.SDK.Processing.ColumnBuilding;
using Microsoft.Performance.SDK.Runtime.ColumnVariants.TreeNodes;
using Microsoft.Performance.SDK.Runtime.ColumnVariants.TreeNodes.Visitors;

namespace Microsoft.Performance.SDK.Runtime.ColumnVariants.Registrar;

/// <summary>
///     Exposes variants of columns registered during table building.
/// </summary>
public interface IColumnVariantsRegistrar
{
    /// <summary>
    ///     Attempts to get the root of the variants tree for the given column, if one exists.
    /// </summary>
    /// <param name="baseColumn">
    ///     The column for which to get the variants tree root.
    /// </param>
    /// <param name="variantsTreeNodes">
    ///     The root of the variants tree for the given column, if one exists. If found, this root can be
    ///     used with an <see cref="IColumnVariantsTreeNodesVisitor"/> to find every concrete
    ///     <see cref="ColumnVariantDescriptor"/> of the given column.
    /// </param>
    /// <returns>
    ///     <c>true</c> if the variants tree root was found; <c>false</c> otherwise.
    /// </returns>
    bool TryGetVariantsTreeRoot(
        IDataColumn baseColumn,
        out IColumnVariantsTreeNode variantsTreeNodes);
}