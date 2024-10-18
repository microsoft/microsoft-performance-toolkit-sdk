// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.

using Microsoft.Performance.SDK.Extensibility;
using Microsoft.Performance.SDK.Processing;
using System;
using Microsoft.Performance.SDK.Processing.ColumnBuilding;

namespace Microsoft.Performance.Toolkit.Engine.Tests.TestTables
{
    [Table]
    public static class TableWithColumnVariants
    {
        public static TableDescriptor TableDescriptor => new TableDescriptor(
            Guid.Parse("462B517A-3BB0-49B2-9E07-4F51F90C8FBF"),
            "Test Table With Column Variants",
            "Used by the Engine Tests to test table with columns that contain variants.",
            "Engine");

        public static readonly ColumnVariantDescriptor VariantDescriptor =
            new ColumnVariantDescriptor(Guid.NewGuid(), "Toggled Variant");

        public static readonly IProjection<int, bool> VariantProjection =
            Projection.Constant<int, bool>(true);

        public static readonly ColumnConfiguration ColumnOne =
                new ColumnConfiguration(new ColumnMetadata(Guid.NewGuid(), "One"));

        public static readonly ColumnConfiguration ColumnTwo =
            new ColumnConfiguration(new ColumnMetadata(Guid.NewGuid(), "Two"));

        public static void BuildTable(ITableBuilder tableBuilder, IDataExtensionRetrieval tableData)
        {
            tableBuilder.SetRowCount(1)
                .AddColumnWithVariants(ColumnOne, Projection.Constant(1), builder =>
                {
                    builder
                        .WithToggle(VariantDescriptor, VariantProjection)
                        .Commit();
                })
                .AddColumn(ColumnTwo, Projection.Constant(2));
        }
    }
}
