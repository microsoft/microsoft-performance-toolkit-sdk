// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.

using Microsoft.Performance.SDK;
using Microsoft.Performance.SDK.Extensibility.SourceParsing;
using Microsoft.Performance.SDK.Processing;
using System;
using System.Collections.Generic;
using System.Threading;

namespace SimplePlugin
{
    public class WordSourceParser
        : ISourceParser<WordEvent, WordSourceParser, string>
    {
        private readonly string[] filePaths;

        public WordSourceParser(string[] fileNames)
        {
            this.filePaths = fileNames;
        }

        // Used to tell the SDK the time range of the data (if applicable) and any other relevant data for rendering / synchronizing.
        // This gets updated as we process the files
        private DataSourceInfo dataSourceInfo;

        public DataSourceInfo DataSourceInfo => this.dataSourceInfo;

        public string Id => Constants.ParserId;

        public Type DataElementType => typeof(WordEvent);

        public Type DataContextType => typeof(WordSourceParser);

        public Type DataKeyType => typeof(string);

        // For this example, we do not need to parse more than once
        public int MaxSourceParseCount => 1;

        public void PrepareForProcessing(bool allEventsConsumed, IReadOnlyCollection<string> requestedDataKeys)
        {
            // NOOP here; process everything :)
        }

        public void ProcessSource(ISourceDataProcessor<WordEvent, WordSourceParser, string> dataProcessor, ILogger logger, IProgress<int> progress, CancellationToken cancellationToken)
        {
            //
            // This is where you add your own logic to process the data into a format for your tables.
            //
            // In this sample, our tables will operate on a collection of lines in the file
            // so we read all of the lines of the file and store them into a backing dictionary field.
            //
            // ProcessAsync is also where you would determine the information necessary for GetDataSourceInfo().
            // In this sample, we take down the start and stop timestamps from the files
            //
            // Note: if you must do processing based on which tables are enabled, you would check the EnabledTables property
            // (provided in the base class) on your class to see what you should do. For example, a processing source with
            // many disjoint tables may look at what tables are enabled in order to turn on only specific processors to avoid
            // processing everything if it doesn't have to.
            //

            // Timestamp relative to the first event time from all lines
            Timestamp startTime = Timestamp.MaxValue;

            // Timestamp relative to the last event from all lines
            Timestamp endTime = Timestamp.MinValue;

            // The time of the first event from all lines
            DateTime firstEvent = DateTime.MinValue;

            // The final processed data we are building, with correct (relative) Timestamp values
            var relativeContentDictionary = new Dictionary<string, IReadOnlyList<Tuple<Timestamp, string>>>();

            // Used to help calculate progress
            int nFiles = this.filePaths.Length;
            var currentFile = 0;

            //
            // In this sample, we are parsing each file in-memory inside of our ProcessAsyncCore method. It is possible to delegate
            // the task of processing a file to a custom Parser object by extending CustomDataProcessorBaseWithSourceParser
            // instead of CustomDataProcessorBase. See the advanced samples for more information.
            //

            foreach (var path in this.filePaths)
            {
                var content = System.IO.File.ReadAllLines(path);

                // Used to help calculate progress
                int nLines = content.Length;
                var currentLine = 0;

                foreach (var line in content)
                {
                    var items = line.Split(new[] { ',' }, 2);

                    //
                    // Validate input. Any exceptions thrown while processing data sources bubbled up to the caller
                    // (outside the SDK) who asked the data sources to be processed.
                    //

                    if (items.Length < 2)
                    {
                        throw new InvalidOperationException("File line cannot be split to two sub-strings");
                    }

                    DateTime time;
                    if (!DateTime.TryParse(items[0], out time))
                    {
                        throw new InvalidOperationException("Time cannot be pasred to DateTime format");
                    }

                    var timeStamp = Timestamp.FromNanoseconds(time.Ticks * 100);
                    var words = items[1].Split(" ", StringSplitOptions.RemoveEmptyEntries);

                    if (timeStamp < startTime)
                    {
                        startTime = timeStamp;
                        firstEvent = time;
                    }

                    if (timeStamp > endTime)
                    {
                        endTime = timeStamp;
                    }

                    foreach (var word in words)
                    {
                        dataProcessor.ProcessDataElement(new WordEvent(word, timeStamp, path), this, cancellationToken);
                    }         

                    // Reporting progress is optional, but recommended
                    progress.Report(CalculateProgress(currentLine, currentFile, nLines, nFiles));
                    ++currentLine;
                }

                progress.Report(CalculateProgress(currentLine, currentFile, nLines, nFiles));
                ++currentFile;
            }

            // startTime is calculated from firstEvent in the above for loop, so our first event timestamp
            // will always be 0.
            this.dataSourceInfo = new DataSourceInfo(0, (endTime - startTime).ToNanoseconds, firstEvent.ToUniversalTime());

            progress.Report(100);
        }

        private int CalculateProgress(int currentLine, int currentFile, int nLines, int nFiles)
        {
            double completedFilesWeight = (double)currentFile;

            double completedLinesWeight = (double)currentLine / nLines;

            double percentComplete = (completedFilesWeight + completedLinesWeight) / nFiles;
            Console.WriteLine(percentComplete);
            return (int)(percentComplete * 100.0);
        }
    }
}
