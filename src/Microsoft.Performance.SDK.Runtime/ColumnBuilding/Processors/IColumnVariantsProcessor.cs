// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.

using Microsoft.Performance.SDK.Processing.ColumnBuilding;
using Microsoft.Performance.SDK.Runtime.ColumnVariants;

namespace Microsoft.Performance.SDK.Runtime.ColumnBuilding.Processors;

/// <summary>
///     Represents an object that processes built column variants.
/// </summary>
internal interface IColumnVariantsProcessor
{
    /// <summary>
    ///     Processes the given column variants. This method will be called
    ///     with the root <see cref="IColumnVariant"/> of a column variants tree
    ///     once the column variants have been built.
    /// </summary>
    /// <param name="variants">
    ///     The root <see cref="IColumnVariant"/> of a column variants tree.
    /// </param>
    /// <remarks>
    ///     This method is invoked during <see cref="IColumnBuilder.Build"/>.
    /// </remarks>
    void ProcessColumnVariants(IColumnVariant variants);
}