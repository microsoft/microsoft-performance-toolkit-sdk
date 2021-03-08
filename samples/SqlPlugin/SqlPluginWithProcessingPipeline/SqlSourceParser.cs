// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.

using Microsoft.Performance.SDK;
using Microsoft.Performance.SDK.Extensibility.SourceParsing;
using Microsoft.Performance.SDK.Processing;
using System;
using System.Collections.Generic;
using System.IO;
using System.Threading;
using System.Xml;

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

        public DateTime StartWallClockUtc { get; private set; }

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

        public void ProcessSource(ISourceDataProcessor<SqlEvent, SqlSourceParser, string> dataProcessor,
                                                       ILogger logger,
                                                       IProgress<int> progress,
                                                       CancellationToken cancellationToken)
        {
            using (FileStream stream = File.OpenRead(this.filePath))
            using (XmlReader reader = XmlReader.Create(stream))
            {
                bool inEvents = false;
                string eventClass = null;
                DateTime? traceStartTime = null;
                DateTime? traceEndTime = null;

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

                                        if (!traceStartTime.HasValue)
                                        {
                                            traceStartTime = startTime;
                                            this.StartWallClockUtc = startTime.Value;
                                        }

                                        if (!traceEndTime.HasValue || traceEndTime.Value <= startTime)
                                        {
                                            traceEndTime = startTime;
                                        }
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

                                dataProcessor.ProcessDataElement(sqlEvent, this, cancellationToken);
                            }

                            break;
                    }

                    progress.Report((int)(100.0 * stream.Position / stream.Length));
                }

                if (traceStartTime.HasValue && traceEndTime.HasValue)
                {
                    var traceStartTimestamp = Timestamp.FromNanoseconds(traceStartTime.Value.Ticks * 100);
                    var traceEndTimestamp = Timestamp.FromNanoseconds(traceEndTime.Value.Ticks * 100);

                    this.dataSourceInfo = new DataSourceInfo(0, (traceEndTimestamp - traceStartTimestamp).ToNanoseconds, traceStartTime.Value);
                }
            }         
        }
    }
}
