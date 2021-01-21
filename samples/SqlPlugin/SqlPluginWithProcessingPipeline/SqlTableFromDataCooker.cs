// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.

using Microsoft.Performance.SDK.Extensibility;
using Microsoft.Performance.SDK.Processing;
using System;
using System.Collections.Generic;

namespace SqlPluginWithProcessingPipeline
{
    [Table]
    public class SqlTableFromDataCooker
    {
        public static TableDescriptor TableDescriptor => new TableDescriptor(
            Guid.Parse("{9DA1A713-D34B-49F6-AE32-A4D1B8738004}"),
            "SQL Trace Events from Data Cooker",
            "SQL Trace recorded with SQL Server Profiler",
            "SQL",
            // New: hook up table to our data cooker
            requiredDataCookers: new List<DataCookerPath> { SqlPluginConstants.CookerPath }); 

        private static readonly ColumnConfiguration EventClassColumn = new ColumnConfiguration(
            new ColumnMetadata(new Guid("{0A8E1C32-F314-42FC-80C3-F60435BB364B}"), "EventClass", "The event class."),
            new UIHints { Width = 150 });

        private static readonly ColumnConfiguration TextDataColumn = new ColumnConfiguration(
            new ColumnMetadata(new Guid("{D10EE878-C898-4C2C-8536-DDC76F7005BD}"), "TextData", "The text data."),
            new UIHints { Width = 150 });

        private static readonly ColumnConfiguration RelativeTimestampColumn = new ColumnConfiguration(
            new ColumnMetadata(new Guid("{AF511183-6E41-4CAB-8A09-109444533CF2}"), "Time", "The time relative to start of trace."),
            new UIHints { Width = 80 });

        //
        // Required. The SDK will invoke this method once all data cookers and processors
        // have processed their data
        //
        public static void BuildTable(ITableBuilder tableBuilder, IDataExtensionRetrieval tableData)
        {
            //
            // STEP 0 (new): Get data from cooker
            //
            var sqlEvents = tableData.QueryOutput<IReadOnlyList<SqlEventWithRelativeTimestamp>>(
                new DataOutputPath(SqlPluginConstants.CookerPath, nameof(SqlDataCooker.SqlEventsWithRelativeTimestamps)));

            //
            // Everything else is the same as before
            //

            var baseProjection = Projection.Index(sqlEvents);

            var eventClassProjection = baseProjection.Compose(x => x.EventClass);
            var textDataProjection = baseProjection.Compose(x => x.TextData);
            var relativeTimestampProjection = baseProjection.Compose(x => x.RelativeTimestamp);

            var tableConfig = new TableConfiguration("SQL Trace Events by Event Class")
            {
                Columns = new[]
              {
                    EventClassColumn,
                    TableConfiguration.PivotColumn,
                    TableConfiguration.LeftFreezeColumn,
                    TextDataColumn,
                    TableConfiguration.GraphColumn,
                    TableConfiguration.RightFreezeColumn,
                    RelativeTimestampColumn
                },
                Layout = TableLayoutStyle.GraphAndTable
            };

            tableConfig.AddColumnRole(ColumnRole.StartTime, RelativeTimestampColumn.Metadata.Guid);

            tableBuilder.AddTableConfiguration(tableConfig)
                .SetDefaultTableConfiguration(tableConfig)
                .SetRowCount(sqlEvents.Count)
                .AddColumn(EventClassColumn, eventClassProjection)
                .AddColumn(TextDataColumn, textDataProjection)
                .AddColumn(RelativeTimestampColumn, relativeTimestampProjection);
        }
    }
}
