// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.

using Microsoft.Performance.SDK;
using Microsoft.Performance.SDK.Extensibility.SourceParsing;
using Microsoft.Performance.SDK.Processing;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading;
using System.Xml.Linq;

namespace SqlPluginWithProcessingPipeline
{
    public class SqlSourceParser
            : ISourceParser<SqlEvent, SqlSourceParser, string>
    {
        // XML file to be parsed. For this demo, we assume we only have one data source.
        // For a full implementation, this should be a collection of all file paths given
        // to the Custom Data Source
        private readonly string filePath;

        // Information about this data source the SDK requires for building tables
        private DataSourceInfo dataSourceInfo;

        public SqlSourceParser(string filePath)
        {
            this.filePath = filePath;
        }

        public DataSourceInfo DataSourceInfo => this.dataSourceInfo;

        public string Id => SqlPluginConstants.ParserId;

        public Type DataElementType => typeof(SqlEvent);

        public Type DataContextType => typeof(SqlSourceParser);

        public Type DataKeyType => typeof(string);

        // For this example, we do not need to parse more than once
        public int MaxSourceParseCount => 1;

        public void PrepareForProcessing(bool allEventsConsumed, IReadOnlyCollection<string> requestedDataKeys)
        {
            // NOOP here; process everything :)
        }

        public void ProcessSource(ISourceDataProcessor<SqlEvent, SqlSourceParser, string> dataProcessor, ILogger logger, IProgress<int> progress, CancellationToken cancellationToken)
        {
            XNamespace ns = SqlPluginConstants.SqlXmlNamespace;

            var xmlDocument = XDocument.Parse(File.ReadAllText(this.filePath));

            var events = xmlDocument.Element(ns + "TraceData").Element(ns + "Events").Elements(ns + "Event");

            // DIFFERENT: filter out and set datacontext from start and end events here instead of after parsing
            var startDateTime = DateTime.Parse(events.First().Element(ns + "Column").Value).ToUniversalTime();
            var startTimestamp = Timestamp.FromNanoseconds(startDateTime.Ticks * 100);

            var endDateTime = DateTime.Parse(events.Last().Element(ns + "Column").Value).ToUniversalTime();
            var endTimestamp = Timestamp.FromNanoseconds(endDateTime.Ticks * 100);

            this.dataSourceInfo = new DataSourceInfo(0, (endTimestamp - startTimestamp).ToNanoseconds, startDateTime);

            foreach (var traceEvent in events.Except(new XElement[] { events.First(), events.Last() }))
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

                // DIFFERENT: we process the data elements here instead of passing them to tables.
                // The SDK handles passing data SQL events to cookers/tables
                dataProcessor.ProcessDataElement(sqlEvent, this, cancellationToken);
            }

            progress.Report(100);
        }
    }
}
