using System;
using System.Collections.Generic;
using DataExtensionsSample.OutputTypes;
using DataExtensionsSample.SourceDataCookers;
using Microsoft.Performance.SDK.Extensibility;
using Microsoft.Performance.SDK.Processing;

namespace DataExtensionsSample.Tables
{
    [Table]
    [PrebuiltConfigurationsFilePath("Resources\\ProcessTableConfiguration.json")]
    public class ProcessesTable
    {
        public static TableDescriptor TableDescriptor = new TableDescriptor(
            Guid.Parse("{EE2FA823-C447-4324-969C-BC8AE215B713}"),
            "Processes",
            "Processes",
            requiredDataCookers:new List<DataCookerPath> { ProcessDataCooker.CookerPath });

        private static readonly ColumnConfiguration processNameColumnConfig =
            new ColumnConfiguration(
                new ColumnMetadata(new Guid("{7C382588-735D-4450-91A5-F4DF6BD4E42A}"), "Process Name"),
                new UIHints { Width = 80, });

        private static readonly ColumnConfiguration processIdColumnConfig =
            new ColumnConfiguration(
                new ColumnMetadata(new Guid("{54B42F78-B2E2-49B0-B5D1-F066399908DB}"), "Pid"),
                new UIHints { Width = 80, });

        private static readonly ColumnConfiguration startTimeColumnConfig =
            new ColumnConfiguration(
                new ColumnMetadata(new Guid("{8361CAAD-CF84-4265-9F8C-8AD52528E945}"), "Start Time"),
                new UIHints { Width = 80, });

        private static readonly ColumnConfiguration stopTimeColumnConfig =
            new ColumnConfiguration(
                new ColumnMetadata(new Guid("{20DDEF6C-CDB9-4A5E-97E1-23AB7E5A60A8}"), "Stop Time"),
                new UIHints { Width = 80, });

        public static void BuildTable(ITableBuilder tableBuilder, IDataExtensionRetrieval tableData)
        {
            var processInfo = tableData.QueryOutput<ProcessInfo>(
                new DataOutputPath(ProcessDataCooker.CookerPath, "ProcessInfo"));

            if(processInfo == null)
            {
                // no process data elements were processed by the data cooker
                return;
            }

            if (processInfo.ProcessActivity.Count == 0)
            {
                return;
            }

            var table = tableBuilder.SetRowCount(processInfo.ProcessActivity.Count);
            var processActivityProjection = Projection.CreateUsingFuncAdaptor((i) => processInfo.ProcessActivity[i]);

            table.AddColumn(processNameColumnConfig, processActivityProjection.Compose(pa => pa.ProcessName));
            table.AddColumn(processIdColumnConfig, processActivityProjection.Compose(pa => pa.ProcessId));
            table.AddColumn(startTimeColumnConfig, processActivityProjection.Compose(pa => pa.StartTime));
            table.AddColumn(stopTimeColumnConfig, processActivityProjection.Compose(pa => pa.StopTime));
        }
    }
}
