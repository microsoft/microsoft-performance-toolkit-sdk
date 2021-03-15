// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.

using Microsoft.Performance.SDK.Extensibility;
using Microsoft.Performance.SDK.Processing;
using System;

namespace Microsoft.Performance.Toolkit.Engine.Tests.TestTables
{
    [Table]
    public static class SingleRowTable
    {
        public static TableDescriptor TableDescriptor => new TableDescriptor(
            Guid.Parse("{999FF8B2-01DA-4994-ABF6-066AC1F78D92}"),
            "Single Row Test Table",
            "Used by the Engine Tests to test table with a single row",
            "Engine",
            requiredDataCookers: Array.Empty<DataCookerPath>());

        public static readonly ColumnConfiguration ColumnOne =
                new ColumnConfiguration(new ColumnMetadata(Guid.NewGuid(), "One"));

        public static readonly ColumnConfiguration ColumnTwo =
            new ColumnConfiguration(new ColumnMetadata(Guid.NewGuid(), "Two"));

        public static readonly ColumnConfiguration ColumnThree =
            new ColumnConfiguration(new ColumnMetadata(Guid.NewGuid(), "Three"));

        public static void BuildTable(ITableBuilder tableBuilder, IDataExtensionRetrieval tableData)
        {
            tableBuilder.SetRowCount(1)
                .AddColumn(ColumnOne, Projection.Constant(1))
                .AddColumn(ColumnTwo, Projection.Constant(2))
                .AddColumn(ColumnThree, Projection.Constant(3));
        }
    }
}
