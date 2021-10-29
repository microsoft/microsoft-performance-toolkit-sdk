// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.

using System;
using System.Collections.Generic;
using Microsoft.Performance.SDK.Extensibility;
using Microsoft.Performance.SDK.Processing;
using Microsoft.Performance.Toolkit.Engine.Tests.TestCookers.Composites;
using Microsoft.Performance.Toolkit.Engine.Tests.TestCookers.Source5;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace Microsoft.Performance.Toolkit.Engine.Tests.TestTables
{
    [Table]
    public class Source5InternalTableNoBuildAction
    {
        public static TableDescriptor TableDescriptor => new TableDescriptor(
            Guid.Parse("{2C115535-882D-4B98-991A-E82527688E9C}"),
            "Source5 Internal Table No Build Action",
            "Used by the Engine Tests to test an internal table with no build action",
            "Engine",
            isInternalTable: true,
            requiredDataCookers: new[] { Source5DataCooker.DataCookerPath });

        public static readonly ColumnConfiguration ColumnOne =
                new ColumnConfiguration(new ColumnMetadata(Guid.NewGuid(), "Id"));

        public static readonly ColumnConfiguration ColumnTwo =
            new ColumnConfiguration(new ColumnMetadata(Guid.NewGuid(), "Value"));

        // This name is not the default to ensure that the build action won't be found automatically.
        public static void BuildTableAction(ITableBuilder tableBuilder, IDataExtensionRetrieval tableData, int x)
        {
            // Just a double check that we're getting extra data that we expect. Honestly, I don't know how this would fail.
            Assert.AreEqual(Source5DataSource.BuildActionInt, x);

            // This table didn't require the composite cooker, and its data shouldn't be available.
            Assert.IsFalse(tableData.TryQueryOutput(Composite2Cooker.DataOutputPath, out List<Composite2Output> result));

            Assert.IsTrue(tableData.TryQueryOutput(
                new DataOutputPath(Source5DataCooker.DataCookerPath, nameof(Source5DataCooker.Objects)),
                out List<Source5DataObject> objects));

            var resultProjection = Projection.Index(objects);
            var keyProjection = resultProjection.Project(GetId);
            var source5CountProjection = resultProjection.Project(GetValue);

            tableBuilder.SetRowCount(objects.Count)
                .AddColumn(ColumnOne, keyProjection)
                .AddColumn(ColumnTwo, source5CountProjection);
        }

        private static int GetId(Source5DataObject output)
        {
            return output.Id;
        }

        private static string GetValue(Source5DataObject output)
        {
            return output.Data;
        }
    }
}
