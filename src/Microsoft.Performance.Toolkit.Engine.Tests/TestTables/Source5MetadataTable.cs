﻿// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.

using System;
using System.Collections.Generic;
using Microsoft.Performance.SDK.Extensibility;
using Microsoft.Performance.SDK.Processing;
using Microsoft.Performance.Toolkit.Engine.Tests.TestCookers.Composites;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace Microsoft.Performance.Toolkit.Engine.Tests.TestTables
{
    [Table]
    public static class Source5MetadataTable
    {
        public static TableDescriptor TableDescriptor => new TableDescriptor(
            Guid.Parse("{708B0564-F9BA-49FA-A87C-3B25C5C7C84C}"),
            "Source5 Internal Table",
            "Used by the Engine Tests to test an internal table",
            "Engine",
            isMetadataTable: true,
            requiredDataCookers: new[] { Composite2Cooker.DataCookerPath });

        public static readonly ColumnConfiguration ColumnOne =
                new ColumnConfiguration(new ColumnMetadata(Guid.NewGuid(), "Key"));

        public static readonly ColumnConfiguration ColumnTwo =
            new ColumnConfiguration(new ColumnMetadata(Guid.NewGuid(), "Source5Count"));

        public static void BuildTable(ITableBuilder tableBuilder, IDataExtensionRetrieval tableData)
        {
            Assert.IsTrue(tableData.TryQueryOutput(Composite2Cooker.DataOutputPath, out List<Composite2Output> result));

            var resultProjection = Projection.Index(result);
            var keyProjection = resultProjection.Project(GetKey);
            var source5CountProjection = resultProjection.Project(GetSource5Count);

            tableBuilder.SetRowCount(result.Count)
                .AddColumn(ColumnOne, keyProjection)
                .AddColumn(ColumnTwo, source5CountProjection);
        }

        private static int GetKey(Composite2Output output)
        {
            return output.Key;
        }

        private static int GetSource5Count(Composite2Output output)
        {
            return output.Source5Count;
        }
    }
}
