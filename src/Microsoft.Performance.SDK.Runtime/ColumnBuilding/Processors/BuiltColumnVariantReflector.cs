// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.

using Microsoft.Performance.SDK.Runtime.ColumnVariants.TreeNodes;

namespace Microsoft.Performance.SDK.Runtime.ColumnBuilding.Processors;

/// <summary>
///     Exposes the <see cref="IColumnVariantsTreeNode"/> that was passed to the processor.
/// </summary>
internal readonly struct BuiltColumnVariantReflector
    : IColumnVariantsProcessor
{
    /// <summary>
    ///     Initializes a new instance of the <see cref="BuiltColumnVariantReflector"/>
    /// </summary>
    public BuiltColumnVariantReflector()
    {
        this.Output = NullColumnVariantsTreeNode.Instance;
    }

    /// <summary>
    ///     The <see cref="IColumnVariantsTreeNode"/> that was last passed to this processor.
    /// </summary>
    public IColumnVariantsTreeNode Output { get; private set; }

    public void ProcessColumnVariants(IColumnVariantsTreeNode variantsTreeNodes)
    {
        this.Output = variantsTreeNodes;
    }
}