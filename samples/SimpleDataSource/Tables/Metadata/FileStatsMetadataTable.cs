using Microsoft.Performance.SDK;
using Microsoft.Performance.SDK.Processing;
using System;
using System.Collections.Generic;
using System.Linq;

namespace SampleCustomDataSource.Tables.Metadata
{
    //
    // This is a sample Metadata table for .txt files
    // Metadata tables are used to expose information about the data being processed, not the actual data being processed.
    // Metadata could be "number of events in the file," "file size," or any other number of things that describes the data being processed.
    // In this sample table, we expose three columns: File Name, Line Count and Word Count.
    //

    //
    // In order for the CustomDataSourceBase to understand your metadata table, 
    // pass in isMetadataTable = true in the TableDescriptor constructor for this table.
    //

    [Table]             
    //
    // Have the MetadataTable inherit the custom TableBase class
    //
    public sealed class FileStatsMetadataTable
        : TableBase
    {
        public FileStatsMetadataTable(IReadOnlyDictionary<string, IReadOnlyList<Tuple<Timestamp, string>>> lines)
            : base(lines)
        {
        }

        public static TableDescriptor TableDescriptor = new TableDescriptor(
            Guid.Parse("{40AF86E5-0DF8-47B1-9A01-1D6C3529B75B}"), // The GUID must be unique across all tables
            "File Stats",                                         // The MetadataTable must have a name
            "Statistics for text files",                          // The MetadataTable must have a description
            TableDescriptor.DefaultCategory,                      // A category is optional. It useful for grouping different types of tables in the viewer's UI.
            true);                                                // Marks this table as a metadata table

        //
        // Declare columns here. You can do this using the ColumnConfiguration class. 
        // It is possible to declaratively describe the table configuration as well. Please refer to our Advanced Topics Wiki for more information.
        //
        // The Column metadata describes each column in the table. 
        // Each column must have a unique GUID and a unique name. The GUID must be unique globally, but the name only unique within the table.
        //
        // The UIHints provides some hints to a viewer (such as WPA) on how to render the column. 
        // In this sample, we are simply saying to allocate at least 80 units of width.
        //

        private static readonly ColumnConfiguration FileNameColumn = new ColumnConfiguration(
            new ColumnMetadata(new Guid("{2604E009-F47D-4A22-AA4F-B148D1C26553}"), "File Name", "The name of the file."),
            new UIHints { Width = 80 });

        private static readonly ColumnConfiguration LineCountColumn = new ColumnConfiguration(
           new ColumnMetadata(new Guid("{C499AF57-64D1-47A9-8550-CF24D6C9615D}"), "Line Count", "The number of lines in the file."),
           new UIHints { Width = 80 });

        private static readonly ColumnConfiguration WordCountColumn = new ColumnConfiguration(
           new ColumnMetadata(new Guid("{3669E90A-DC8F-4972-A5D3-3E13AFDF5DB7}"), "Word Count", "The number of words in the file."),
           new UIHints { Width = 80 });

        public override void Build(ITableBuilder tableBuilder)
        {
            //
            // Implement your columns here. 
            // Columns are implemented via Projections, which are simply functions that map a row index to a data point.
            //

            //
            // File name projection is simply a map of an index into the dictionary key collection
            // Use Projection.Index() to get this projection
            //

            var fileNames = this.Lines.Keys.ToArray();
            var fileNameProjection = Projection.Index(fileNames.AsReadOnly());

            //
            // Line count is the size of the list containing the files lines.
            // Simply compose file name projection with another projection
            // that grabs the count of the lines in the file
            //

            var lineCountProjection = fileNameProjection.Compose(x => this.Lines[x].Count);

            //
            // Word count is the sum of all words in all of the lines. 
            // This projection can be done by composing with a split on whitespace, and summing the results. 
            // This can be expensive, so a cached projection is used that caches the results.
            //

            var wordCountProjection = Projection.CacheOnFirstUse(
                fileNames.Length,
                fileNameProjection.Compose(x => this.Lines[x].SelectMany(y => y.Item2.Split((char[])null, StringSplitOptions.RemoveEmptyEntries)).Count()));

            //
            //  Use the table builder to build the table. Simply set the row count (we have one row per file) and then add the columns using AddColumn
            //

            tableBuilder.SetRowCount(fileNames.Length)
                .AddColumn(FileNameColumn, fileNameProjection)
                .AddColumn(LineCountColumn, lineCountProjection)
                .AddColumn(WordCountColumn, wordCountProjection);
        }
    }
}
