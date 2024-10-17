// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.

using System;
using System.Linq;
using Microsoft.Performance.SDK.Processing;
using Microsoft.Performance.SDK.Runtime.ColumnVariants.Registrar;
using Microsoft.Performance.SDK.Runtime.ColumnVariants.TreeNodes;
using Microsoft.Performance.SDK.Runtime.ColumnVariants.TreeNodes.Visitors;

namespace Microsoft.Performance.SDK.Runtime.ColumnBuilding.Processors;

/// <summary>
///     Responsible for registering built column variants with a table builder.
/// </summary>
public class BuiltColumnVariantsRegistrant
    : IColumnVariantsProcessor
{
    private readonly IDataColumn baseColumn;
    private readonly ColumnVariantsRegistrar registrar;
    private readonly ColumnVariantsExtractor columnVariantsExtractor = new();
    private readonly ColumnVariantsGuidsChecker columnVariantsGuidsChecker = new();

    public BuiltColumnVariantsRegistrant(
        IDataColumn baseColumn,
        ColumnVariantsRegistrar registrar)
    {
        this.baseColumn = baseColumn;
        this.registrar = registrar;
    }

    public void ProcessColumnVariants(IColumnVariantsTreeNode variantsTreeNodes)
    {
        var duplicates = columnVariantsGuidsChecker.FindDuplicateGuids(variantsTreeNodes);
        if (duplicates.Any())
        {
            throw new InvalidOperationException($"The following variant GUIDs were registered multiple times: {string.Join(", ", duplicates)}");
        }

        registrar.SetVariantsTreeRoot(baseColumn, variantsTreeNodes);
        var foundVariants = columnVariantsExtractor.ExtractVariants(variantsTreeNodes);
        registrar.SetVariants(baseColumn, foundVariants);
    }
}