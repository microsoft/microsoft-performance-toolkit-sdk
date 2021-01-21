// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.

using Microsoft.Performance.SDK;
using Microsoft.Performance.SDK.Extensibility;
using Microsoft.Performance.SDK.Processing;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.IO;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using System.Xml.Linq;

namespace SqlPlugin
{
    public class SqlCustomDataProcessor
        : CustomDataProcessorBase
    {
        // XML file to be parsed. For this demo, we assume we only have one data source.
        // For a full implementation, this should be a collection of all file paths given
        // to the Custom Data Source
        private readonly string filePath;

        // Information about this data source the SDK requires for building tables
        // This will get set in ProcessAsyncCore
        private DataSourceInfo dataSourceInfo;

        // The data we care about
        private ReadOnlyCollection<SqlEventWithRelativeTimestamp> sqlEvents;

        /// <summary>
        ///     This constructor takes
        ///         1) The path to the file this processor must process
        ///         2) The parameters to the base class' constructor
        /// </summary>
        public SqlCustomDataProcessor(string filePath,
                                      ProcessorOptions options,
                                      IApplicationEnvironment applicationEnvironment,
                                      IProcessorEnvironment processorEnvironment,
                                      IReadOnlyDictionary<TableDescriptor, Action<ITableBuilder, IDataExtensionRetrieval>> allTablesMapping,
                                      IEnumerable<TableDescriptor> metadataTables)
            : base(options, applicationEnvironment, processorEnvironment, allTablesMapping, metadataTables)
        {
            this.filePath = filePath;
        }

        public override DataSourceInfo GetDataSourceInfo()
        {
            return this.dataSourceInfo;
        }

        protected override Task ProcessAsyncCore(
           IProgress<int> progress,
           CancellationToken cancellationToken)
        {
            // Parse XML for all events
            List<SqlEvent> sqlEvents = ParseXml();

            // Get start and end event DateTimes and convert them to Timestamps
            var startEvent = sqlEvents.First();
            var startTimestamp = Timestamp.FromNanoseconds(startEvent.StartTime.Ticks * 100);

            var endEvent = sqlEvents.Last();
            var endTimestamp = Timestamp.FromNanoseconds(endEvent.StartTime.Ticks * 100);

            // Filter out the trace start and stop events
            sqlEvents = sqlEvents.Except(new SqlEvent[] { startEvent, endEvent }).ToList();

            // Add relative time property to each event
            var sqlEventsWithRelativeTimestamp = sqlEvents.Select(sqlEvent =>
            {
                var offset = sqlEvent.StartTime.Subtract(startEvent.StartTime);
                var relativeTimestamp = Timestamp.FromNanoseconds(offset.Ticks * 100);
                return new SqlEventWithRelativeTimestamp(sqlEvent, relativeTimestamp);
            }).ToList();

            // Set fields from data
            this.dataSourceInfo = new DataSourceInfo(0, (endTimestamp - startTimestamp).ToNanoseconds, startEvent.StartTime);
            this.sqlEvents = new ReadOnlyCollection<SqlEventWithRelativeTimestamp>(sqlEventsWithRelativeTimestamp);

            // Indicate success
            progress.Report(100);
            return Task.CompletedTask;
        }

        private List<SqlEvent> ParseXml()
        {
            XNamespace ns = SqlPluginConstants.SqlXmlNamespace;

            var xmlDocument = XDocument.Parse(File.ReadAllText(this.filePath));

            var events = xmlDocument.Element(ns + "TraceData").Element(ns + "Events").Elements(ns + "Event");

            var sqlEvents = new List<SqlEvent>();
            foreach (var traceEvent in events)
            {
                string textData = string.Empty, applicationName = string.Empty, ntUserName = string.Empty, loginName = string.Empty;
                int? cpu = null, reads = null, writes = null, duration = null, clientProcessId = null, spid = null;
                DateTime? startTime = null, endTime = null;

                string eventClass = traceEvent.Attribute("name").Value;

                var columns = traceEvent.Elements(ns + "Column");
                foreach (var col in columns)
                {
                    switch (col.Attribute("name").Value)
                    {
                        case "TextData":
                            textData = col.Value;
                            break;
                        case "ApplicationName":
                            applicationName = col.Value;
                            break;
                        case "NTUserName":
                            ntUserName = col.Value;
                            break;
                        case "LoginName":
                            loginName = col.Value;
                            break;
                        case "CPU":
                            cpu = int.Parse(col.Value);
                            break;
                        case "Reads":
                            reads = int.Parse(col.Value);
                            break;
                        case "Writes":
                            writes = int.Parse(col.Value);
                            break;
                        case "Duration":
                            duration = int.Parse(col.Value);
                            break;
                        case "ClientProcessID":
                            clientProcessId = int.Parse(col.Value);
                            break;
                        case "SPID":
                            spid = int.Parse(col.Value);
                            break;
                        case "StartTime":
                            startTime = DateTime.Parse(col.Value).ToUniversalTime();
                            break;
                        case "EndTime":
                            endTime = DateTime.Parse(col.Value).ToUniversalTime();
                            break;
                    }
                }

                var sqlEvent = new SqlEvent(eventClass,
                                            textData,
                                            applicationName,
                                            ntUserName,
                                            loginName,
                                            cpu,
                                            reads,
                                            writes,
                                            duration,
                                            clientProcessId,
                                            spid,
                                            startTime.Value,
                                            endTime);

                sqlEvents.Add(sqlEvent);
            }

            return sqlEvents;
        }

        protected override void BuildTableCore(TableDescriptor tableDescriptor,
                                               Action<ITableBuilder, IDataExtensionRetrieval> createTable,
                                               ITableBuilder tableBuilder)
        {
            //
            // Normally, we would use the TableDescriptor to figure out which Table needs to be created.
            // For this example, we will have only 1 table, so we can call methods on it directly
            //

            var table = new SqlTable(this.sqlEvents);
            table.Build(tableBuilder);
        }
    }
}
