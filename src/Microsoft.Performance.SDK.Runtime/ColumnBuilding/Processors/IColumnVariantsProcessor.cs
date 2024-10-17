// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.

using Microsoft.Performance.SDK.Processing.ColumnBuilding;
using Microsoft.Performance.SDK.Runtime.ColumnVariants.TreeNodes;

namespace Microsoft.Performance.SDK.Runtime.ColumnBuilding.Processors;

/// <summary>
///     Represents an object that processes built column variants.
/// </summary>
public interface IColumnVariantsProcessor
{
    /// <summary>
    ///     Processes the given column variants. This method will be called
    ///     with the root <see cref="IColumnVariantsTreeNode"/> of a column variants tree
    ///     once the column variants have been built.
    /// </summary>
    /// <param name="variantsTreeNodes">
    ///     The root <see cref="IColumnVariantsTreeNode"/> of a column variants tree.
    /// </param>
    /// <remarks>
    ///     This method is invoked during <see cref="IColumnBuilder.Commit"/>.
    /// </remarks>
    void ProcessColumnVariants(IColumnVariantsTreeNode variantsTreeNodes);
}