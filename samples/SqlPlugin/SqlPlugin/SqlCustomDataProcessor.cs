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
using System.Xml;

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
            List<SqlEvent> sqlEvents = ParseXml(progress);

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

        /// <summary>
        ///     Parses the XML file saved in <see cref="filePath"/>.
        ///     <para/>
        ///     The inner workings of this method are not important for understanding
        ///     this sample.
        /// </summary>
        /// <param name="progress">
        ///     <see cref="IProgress{int}"/> to report progress to
        /// </param>
        /// <returns>
        ///     The raw <see cref="SqlEvent"/> data objects that contain
        ///     the events inside the XML file.
        /// </returns>
        private List<SqlEvent> ParseXml(IProgress<int> progress)
        {
            var sqlEvents = new List<SqlEvent>();
            using (FileStream stream = File.OpenRead(this.filePath))
            using (XmlReader reader = XmlReader.Create(stream))
            {
                bool inEvents = false;
                string eventClass = null;

                string textData = string.Empty, applicationName = string.Empty, ntUserName = string.Empty, loginName = string.Empty;
                int? cpu = null, reads = null, writes = null, duration = null, clientProcessId = null, spid = null;
                DateTime? startTime = null, endTime = null;

                while (reader.Read())
                {
                    switch (reader.NodeType)
                    {
                        case XmlNodeType.Element:

                            if (reader.Name == "Events")
                            {
                                inEvents = true;
                            }
                            else if (inEvents && reader.Name == "Event")
                            {
                                eventClass = reader.GetAttribute("name");
                            }
                            else if (eventClass != null && reader.Name == "Column")
                            {
                                switch (reader.GetAttribute("name"))
                                {
                                    case "TextData":
                                        textData = reader.ReadElementContentAsString();
                                        break;
                                    case "ApplicationName":
                                        applicationName = reader.ReadElementContentAsString();
                                        break;
                                    case "NTUserName":
                                        ntUserName = reader.ReadElementContentAsString();
                                        break;
                                    case "LoginName":
                                        loginName = reader.ReadElementContentAsString();
                                        break;
                                    case "CPU":
                                        cpu = reader.ReadElementContentAsInt();
                                        break;
                                    case "Reads":
                                        reads = reader.ReadElementContentAsInt();
                                        break;
                                    case "Writes":
                                        writes = reader.ReadElementContentAsInt();
                                        break;
                                    case "Duration":
                                        duration = reader.ReadElementContentAsInt();
                                        break;
                                    case "ClientProcessID":
                                        clientProcessId = reader.ReadElementContentAsInt();
                                        break;
                                    case "SPID":
                                        spid = reader.ReadElementContentAsInt();
                                        break;
                                    case "StartTime":
                                        startTime = reader.ReadElementContentAsDateTime().ToUniversalTime();
                                        break;
                                    case "EndTime":
                                        endTime = reader.ReadElementContentAsDateTime().ToUniversalTime();
                                        break;
                                }
                            }

                            break;

                        case XmlNodeType.EndElement:

                            if (reader.Name == "Events")
                            {
                                inEvents = false;
                            }
                            else if (inEvents && eventClass != null && reader.Name == "Event")
                            {
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
                                eventClass = null;
                                textData = string.Empty;
                                applicationName = string.Empty;
                                ntUserName = string.Empty;
                                loginName = string.Empty;
                                cpu = null;
                                reads = null;
                                writes = null;
                                duration = null;
                                clientProcessId = null;
                                spid = null;
                                startTime = null;
                                endTime = null;

                                sqlEvents.Add(sqlEvent);
                            }

                            break;
                    }

                    progress.Report((int)(100.0 * stream.Position / stream.Length));
                }
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
