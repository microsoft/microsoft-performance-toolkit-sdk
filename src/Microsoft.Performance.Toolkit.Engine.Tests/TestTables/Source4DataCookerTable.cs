// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.

using Microsoft.Performance.SDK.Extensibility;
using Microsoft.Performance.SDK.Processing;
using Microsoft.Performance.Toolkit.Engine.Tests.TestCookers.Source4;
using System;
using System.Collections.Generic;
using System.Text;

namespace Microsoft.Performance.Toolkit.Engine.Tests.TestTables
{
    [Table]
    public static class Source4DataCookerTable
    {
        public static TableDescriptor TableDescriptor => new TableDescriptor(
            Guid.Parse("{8BE4C87A-CC9A-441A-8173-67414D4C4F68}"),
            "Source4DataCookerTable",
            "Used by the Engine Tests to test a table with a source data cooker",
            "Engine",
            requiredDataCookers: new[] { Source4DataCooker.DataCookerPath });

        public static readonly ColumnConfiguration ColumnOne =
                new ColumnConfiguration(new ColumnMetadata(Guid.NewGuid(), "key"));

        public static readonly ColumnConfiguration ColumnTwo =
            new ColumnConfiguration(new ColumnMetadata(Guid.NewGuid(), "id"));

        public static readonly ColumnConfiguration ColumnThree =
            new ColumnConfiguration(new ColumnMetadata(Guid.NewGuid(), "data"));

        public static void BuildTable(ITableBuilder tableBuilder, IDataExtensionRetrieval tableData)
        {
            var data = tableData.QueryOutput<List<Source4DataObject>>(new DataOutputPath(Source4DataCooker.DataCookerPath, nameof(Source4DataCooker.Objects)));

            var projectedData = Projection.Index(data);

            tableBuilder.SetRowCount(data.Count)
                .AddColumn(ColumnOne, projectedData.Compose(x => x.Key))
                .AddColumn(ColumnTwo, projectedData.Compose(x => x.Id))
                .AddColumn(ColumnThree, projectedData.Compose(x => x.Data));
        }
    }
}
