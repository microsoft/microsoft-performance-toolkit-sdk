// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.

using Microsoft.Performance.SDK.Processing;
using Microsoft.Performance.SDK.Runtime.ColumnVariants.Registrar;
using Microsoft.Performance.SDK.Runtime.ColumnVariants.TreeNodes;
using Microsoft.Performance.SDK.Runtime.ColumnVariants.TreeNodes.Visitors;

namespace Microsoft.Performance.SDK.Runtime.ColumnBuilding.Processors;

/// <summary>
///     Responsible for registering built column variants with a table builder.
/// </summary>
internal class BuiltColumnVariantsRegistrant
    : IColumnVariantsProcessor
{
    private readonly IDataColumn baseColumn;
    private readonly ColumnVariantsRegistrar registrar;
    private readonly ColumnVariantsExtractor columnVariantsExtractor = new();

    public BuiltColumnVariantsRegistrant(
        IDataColumn baseColumn,
        ColumnVariantsRegistrar registrar)
    {
        this.baseColumn = baseColumn;
        this.registrar = registrar;
    }

    public void ProcessColumnVariants(IColumnVariantsTreeNode variantsTreeNodes)
    {
        registrar.SetVariantsTreeRoot(baseColumn, variantsTreeNodes);
        var foundVariants = columnVariantsExtractor.ExtractVariants(variantsTreeNodes);
        registrar.SetVariants(baseColumn, foundVariants);
    }
}