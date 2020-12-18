using Microsoft.Performance.SDK;
using Microsoft.Performance.SDK.Extensibility;
using Microsoft.Performance.SDK.Processing;
using SampleCustomDataSource.Tables;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Threading;
using System.Threading.Tasks;

namespace SampleCustomDataSource
{
    //
    // This is a sample Custom Data Processor that processes simple text files.
    //
    // Custom Data Processors are created in Custom Data Sources and are used to actually process the files.
    // An instance of the Processor is created for each set of files you open whereas only one instance of the Custom Data Source is ever created.
    //
    // The data processor is responsible for instantiating the proper tables based on what the user has decided to enable.
    //

    //
    // Derive the CustomDataProcessorBase abstract class.
    //

    public sealed class SimpleCustomDataProcessor
        : CustomDataProcessorBase
    {
        // The files this custom data processor will have to process
        private readonly string[] filePaths;

        // The "processed data": the contents of each file. We store this as a mapping of a filename
        // to a collection of the (timestamp, text) tuples that make up its lines
        private IReadOnlyDictionary<string, IReadOnlyList<Tuple<Timestamp, string>>> fileContent;

        // Used to tell the SDK the time range of the data (if applicable) and any other relevant data for rendering / synchronizing.
        // This gets updated as we process the files
        private DataSourceInfo dataSourceInfo;

        public SimpleCustomDataProcessor(
           string[] filePaths,
           ProcessorOptions options,
           IApplicationEnvironment applicationEnvironment,
           IProcessorEnvironment processorEnvironment,
           IReadOnlyDictionary<TableDescriptor, Action<ITableBuilder, IDataExtensionRetrieval>> allTablesMapping,
           IEnumerable<TableDescriptor> metadataTables)
            : base(options, applicationEnvironment, processorEnvironment, allTablesMapping, metadataTables)
        {
            this.filePaths = filePaths;
        }

        public override DataSourceInfo GetDataSourceInfo()
        {
            return this.dataSourceInfo;   
        }

        protected override Task ProcessAsyncCore(
           IProgress<int> progress,
           CancellationToken cancellationToken)
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
            // (provided in the base class) on your class to see what you should do. For example, a custom data source with
            // many disjoint tables may look at what tables are enabled in order to turn on only specific processors to avoid
            // processing everything if it doesn't have to.
            //
            // In this sample we create a Time Stamp column that can be used for graphing. For more information on graphing, go to "Graphing" on our Wiki. Link can be found in README.md
            //

            Timestamp startTime = Timestamp.MaxValue;
            Timestamp endTime = Timestamp.MinValue;
            DateTime firstEvent = DateTime.MinValue;
            var contentDictionary = new Dictionary<string, IReadOnlyList<Tuple<Timestamp, string>>>();

            //
            // In this sample, we are parsing each file in-memory inside of our ProcessAsyncCore method. It is possible to delegate
            // the task of processing a file to a custom Parser object by extending CustomDataProcessorBaseWithSourceParser
            // instead of CustomDataProcessorBase. See the advanced samples for more information
            //

            // Used to help calculate progress
            int nFiles = this.filePaths.Length;
            var currentFile = 0;

            foreach (var path in this.filePaths)
            {
                var list = new List<Tuple<Timestamp, string>>();
                var content = System.IO.File.ReadAllLines(path);

                // Used to help calculate progress
                int nLines = content.Length;
                var currentLine = 0;

                foreach (var line in content)
                {
                    var items = line.Split(new[] { ',' }, 2);

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
                    var words = items[1];

                    if (timeStamp < startTime)
                    {
                        startTime = timeStamp;
                        firstEvent = time;
                    }

                    if (timeStamp > endTime)
                    {
                        endTime = timeStamp;
                    }

                    list.Add(new Tuple<Timestamp, string>(timeStamp, words));

                    // Reporting progress is optional, but recommended
                    progress.Report(CalculateProgress(currentLine, currentFile, nLines, nFiles));
                    ++currentLine;
                }

                contentDictionary[path] = list.AsReadOnly();
                this.dataSourceInfo = new DataSourceInfo(startTime.ToNanoseconds, endTime.ToNanoseconds, firstEvent.ToUniversalTime());

                progress.Report(CalculateProgress(currentLine, currentFile, nLines, nFiles));
                ++currentFile;
            }

            this.fileContent = new ReadOnlyDictionary<string, IReadOnlyList<Tuple<Timestamp, string>>>(contentDictionary);

            progress.Report(100);
            return Task.CompletedTask;
        }

        protected override void BuildTableCore(
            TableDescriptor tableDescriptor,
            Action<ITableBuilder, IDataExtensionRetrieval> tableType,
            ITableBuilder tableBuilder)
        {
            //
            // Instantiate the table, and pass the tableBuilder to it.
            //

            var type = tableDescriptor.ExtendedData["Type"] as Type;

            if (type != null)
            {
                var table = InstantiateTable(type);
                table.Build(tableBuilder);
            }            
        }

        private TableBase InstantiateTable(Type tableType)
        {
            //
            // This private method is added to activate the given table type and pass in the file content.
            //

            var instance = Activator.CreateInstance(tableType, new[] { this.fileContent, });
            return (TableBase)instance;
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
