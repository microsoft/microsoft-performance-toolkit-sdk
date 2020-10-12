using System;
using System.Collections.Generic;
using System.Linq;
using Microsoft.Performance.SDK.Extensibility;
using Microsoft.Performance.SDK.Processing;
using DataExtensionsSample.SourceDataCookers;

namespace DataExtensionsSample.MetadataTables
{
    [Table]
    [RequiresCooker(EventStatsCooker.CookerPathAsString)]
    public class EventStatsTable
    {
        public static TableDescriptor TableDescriptor = new TableDescriptor(
            Guid.Parse("E6B3FE19-7E1F-456C-AFC8-25A998B01F53"),
            "Event Stats",
            "Event Stats",
            TableDescriptor.DefaultCategory,
            true);

        private static readonly ColumnConfiguration EventNameConfiguration = new ColumnConfiguration(
            new ColumnMetadata(new Guid("{690910AF-0451-40AC-BF5C-15B75B326795}"), "Event Name", "Event Name"),
            new UIHints { Width = 200, });

        private static readonly ColumnConfiguration CountConfiguration = new ColumnConfiguration(
            new ColumnMetadata(new Guid("{026AFD7B-4CF6-4768-92D4-3EAD936CC345}"), "Count"),
            new UIHints { Width = 80, TextAlignment = TextAlignment.Left, });

        public static void BuildTable(ITableBuilder tableBuilder, IDataExtensionRetrieval requiredData)
        {
            var eventCounts = requiredData.QueryOutput<IReadOnlyDictionary<string, ulong>>(
                new DataOutputPath(EventStatsCooker.CookerPath, "EventCounts"));
            ITableBuilderWithRowCount table = tableBuilder.SetRowCount(eventCounts.Count);

            IReadOnlyList<string> eventNames = eventCounts.Keys.ToList();

            var eventNameProjection = Projection.CreateUsingFuncAdaptor(x => eventNames[x]);
            var eventCountProjection = eventNameProjection.Compose(eventName => eventCounts[eventName]);

            table.AddColumn(
                new BaseDataColumn<string>(
                    EventNameConfiguration,
                    eventNameProjection));

            table.AddColumn(
                new BaseDataColumn<ulong>(
                    CountConfiguration,
                    eventCountProjection));
        }
    }
}
