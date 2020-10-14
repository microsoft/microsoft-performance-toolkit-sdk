using System;
using System.IO;
using System.Threading;
using Microsoft.Performance.SDK;
using Microsoft.Performance.SDK.Processing;
using Microsoft.Performance.SDK.Extensibility;
using Microsoft.Performance.SDK.Extensibility.SourceParsing;
using Newtonsoft.Json;

namespace DataExtensionsSample
{
    /// <summary>
    /// This class parses a file. In doing so it passes data elements from the file into the runtime for further
    /// processing by source data cookers. Also during parsing, it fills in the DataSourceInfo property which
    /// describes this source.
    /// </summary>
    public class CustomSourceParser
        : SourceParserBase<SampleEvent, ISampleEventContext, string>
        , ISampleEventContext
    {
        /// <summary>
        /// Uniquely identifies this source parser.
        /// </summary>
        public const string SourceId = "SampleParser";

        private DataSourceInfo dataSourceInfo;
        private readonly string filePath;

        /// <summary>
        /// This constructor take a path to the file to be parsed and stores that path for later use.
        /// </summary>
        /// <param name="filePath">Path to the file to be parsed.</param>
        public CustomSourceParser(string filePath)
        {
            this.filePath = filePath;
        }

        public uint EventNumber { get; private set; }

        /// <inheritdoc />
        public override string Id => SourceId;

        /// <inheritdoc />
        public override DataSourceInfo DataSourceInfo => this.dataSourceInfo;

        /// <inheritdoc />
        public override void ProcessSource(
            ISourceDataProcessor<SampleEvent, ISampleEventContext, string> dataProcessor,
            ILogger logger,
            IProgress<int> progress,
            CancellationToken cancellationToken)
        {
            try
            {
                ParseFile(dataProcessor, logger, progress, cancellationToken);
            }
            catch (DataExtensionsSampleException e)
            {
                logger.Fatal($"{e.Message}");
                throw;
            }

            progress.Report(100);
        }

        private enum ReadStatus
        {
            NotStarted,
            StartObject,
            NameRecordCountProperty,
            NameWallClockProperty,
            NameDataProperty,
            DataRecords,
            StopArray,
            StopObject
        }

        private void ParseFile(
            ISourceDataProcessor<SampleEvent, ISampleEventContext, string> dataProcessor, 
            ILogger logger, 
            IProgress<int> progress,
            CancellationToken cancellationToken)
        {
            using (var fileReader = File.OpenText(this.filePath))
            {
                using (var reader = new JsonTextReader(fileReader))
                {
                    ReadStatus status = ReadStatus.NotStarted;
                    int recordCount = 0;
                    DateTime wallClock = default(DateTime);

                    var serializer = new JsonSerializer {NullValueHandling = NullValueHandling.Ignore};

                    while (reader.Read())
                    {
                        cancellationToken.ThrowIfCancellationRequested();

                        if (reader.TokenType == JsonToken.StartObject)
                        {
                            if (status != ReadStatus.NotStarted)
                            {
                                throw new DataExtensionsSampleException("Corrupted Source: Found a StartObject token unexpectedly.");
                            }
                            status = ReadStatus.StartObject;
                            continue;
                        }

                        if (reader.TokenType == JsonToken.PropertyName)
                        {
                            if (status == ReadStatus.StartObject)
                            {
                                if (StringComparer.CurrentCultureIgnoreCase.Equals(reader.Value, "DataRecordCount"))
                                {
                                    var countValue = reader.ReadAsInt32();
                                    if (!countValue.HasValue)
                                    {
                                        throw new DataExtensionsSampleException("Corrupted Source: Invalid DataRecordCount value.");
                                    }

                                    recordCount = countValue.Value;
                                    status = ReadStatus.NameRecordCountProperty;
                                    continue;
                                }

                                throw new DataExtensionsSampleException($"Corrupted Source: Found an unexpected property name - {reader.Value}.");
                            }

                            if (status == ReadStatus.NameRecordCountProperty)
                            {
                                if (StringComparer.CurrentCultureIgnoreCase.Equals(reader.Value, "WallClock"))
                                {
                                    reader.Read();
                                    wallClock = serializer.Deserialize<DateTime>(reader);
                                    status = ReadStatus.NameWallClockProperty;
                                    continue;
                                }

                                throw new DataExtensionsSampleException($"Corrupted Source: Found an unexpected property name - {reader.Value}.");
                            }

                            if (status != ReadStatus.NameWallClockProperty)
                            {
                                throw new DataExtensionsSampleException("Corrupted Source: Missing WallClock property.");
                            }

                            if (!StringComparer.CurrentCultureIgnoreCase.Equals(reader.Value, "Data"))
                            {
                                throw new DataExtensionsSampleException($"Corrupted Source: Found an unexpected property name - {reader.Value}");
                            }

                            status = ReadStatus.NameDataProperty;
                            continue;
                        }

                        if (reader.TokenType == JsonToken.StartArray)
                        {
                            if (status != ReadStatus.NameDataProperty)
                            {
                                throw new DataExtensionsSampleException("Corrupted Source: Found a StartArray token unexpectedly.");
                            }

                            Timestamp startTime = Timestamp.Zero;
                            Timestamp endTime = Timestamp.Zero;

                            for (int x = 0; x < recordCount; x++)
                            {
                                this.EventNumber++;

                                reader.Read();
                                var dataElement = serializer.Deserialize<SampleEvent>(reader);

                                if(x == 0)
                                {
                                    startTime = dataElement.Timestamp;
                                }
                                else if(x == recordCount - 1)
                                {
                                    endTime = dataElement.Timestamp;
                                }

                                var result = dataProcessor.ProcessDataElement(dataElement, this, cancellationToken);
                                if(result == DataProcessingResult.CorruptData)
                                {
                                    throw new DataExtensionsSampleException($"Corrupted Event: Event number {this.EventNumber}");
                                }
                            }

                            this.dataSourceInfo = new DataSourceInfo(startTime.ToNanoseconds, endTime.ToNanoseconds, wallClock);

                            status = ReadStatus.DataRecords;
                            continue;
                        }

                        if (reader.TokenType == JsonToken.EndArray)
                        {
                            if (status != ReadStatus.DataRecords)
                            {
                                throw new DataExtensionsSampleException("Corrupted Source: Found an EndArray token unexpectedly.");
                            }

                            status = ReadStatus.StopArray;
                            continue;
                        }

                        if (reader.TokenType == JsonToken.EndObject)
                        {
                            if (status != ReadStatus.StopArray)
                            {
                                throw new DataExtensionsSampleException("Corrupted Source: Found an EndObject token unexpectedly.");
                            }

                            status = ReadStatus.StopObject;
                            return;
                        }

                        throw new DataExtensionsSampleException($"Corrupted Source: Read an unexpected token type: {reader.TokenType}.");
                    }
                }
            }
        }
    }
}