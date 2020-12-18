using Microsoft.Performance.SDK;
using Microsoft.Performance.SDK.Processing;
using System;
using System.Collections.Generic;
using System.Linq;

namespace SampleCustomDataSource.Tables
{
    //
    // This is a sample regular table that counts the characters in each word of a .txt file.
    // It also provides a column (word) that can be grouped on.
    //

    //
    // Add a Table attribute in order for the CustomDataSourceBase to understand your table.
    // 

    [Table]                      

    //
    // Have the MetadataTable inherit the custom TableBase class
    //

    public sealed class WordTable
        : TableBase
    {
        public WordTable(IReadOnlyDictionary<string, IReadOnlyList<Tuple<Timestamp,string>>> lines)
            : base(lines)
        {
        }

        public static TableDescriptor TableDescriptor => new TableDescriptor(
            Guid.Parse("{E122471E-25A6-4F7F-BE6C-E62774FD0410}"), // The GUID must be unique across all tables
            "Word Stats",                                         // The Table must have a name
            "Statistics for words",                               // The Table must have a description
            "Words");                                             // A category is optional. It useful for grouping different types of tables in the viewer's UI.

        //
        // Declare columns here. You can do this using the ColumnConfiguration class. 
        // It is possible to declaratively describe the table configuration as well. Please refer to our Advanced Topics Wiki page for more information.
        //
        // The Column metadata describes each column in the table. 
        // Each column must have a unique GUID and a unique name. The GUID must be unique globally; the name only unique within the table.
        //
        // The UIHints provides some hints to a viewer (such as WPA) on how to render the column. 
        // In this sample, we are simply saying to allocate at least 80 units of width.
        //

        private static readonly ColumnConfiguration FileColumn = new ColumnConfiguration(
            new ColumnMetadata(new Guid("{91C69594-4079-4478-B1A0-3FEC01641D8D}"), "File", "The file containing the word."),
            new UIHints { Width = 80 });

        private static readonly ColumnConfiguration WordColumn = new ColumnConfiguration(
           new ColumnMetadata(new Guid("{A669FC83-BF61-4604-8BB2-44E66FCA7062}"), "Word", "The actual word."),
           new UIHints { Width = 80 });

        private static readonly ColumnConfiguration CharacterCountColumn = new ColumnConfiguration(
           new ColumnMetadata(new Guid("{E3056A08-D44D-4CD6-8158-503BDAEF899C}"), "Character Count", "The number of characters in the word."),
           new UIHints { Width = 80 });

        private static readonly ColumnConfiguration TimeColumn = new ColumnConfiguration(
         new ColumnMetadata(new Guid("{54B4A016-9F78-4BAE-A0AB-AFDFBF33C3F1}"), "Time", "The time when the word is written to the file."),
         new UIHints { Width = 80 });

        public override void Build(ITableBuilder tableBuilder)
        {
            //
            // Implement your columns here. 
            // Columns are implemented via Projections, which are simply functions that map a row index to a data point.
            //

            //
            // Pre-process the data to get a list of <filename, time, word> tuples.
            //

            var allWords = new List<Tuple<string, Timestamp, string>>();
            foreach(var kvp in this.Lines)
            {
                foreach (var tuple in kvp.Value.SelectMany(x => x.Item2.Split((char[])null, StringSplitOptions.RemoveEmptyEntries).Select(y => new Tuple<Timestamp, string>(x.Item1, y))))
                {
                    allWords.Add(Tuple.Create(kvp.Key, tuple.Item1, tuple.Item2));
                }
            }

            //
            // Use Projection.Index() to get a base projection from row index to a <filename, time, word> tuple.
            //

            var baseProjection = Projection.Index(allWords);

            //
            // Create projection for each column by composing the base projection with another projection that maps to the data point as needed.
            //

            var fileProjection = baseProjection.Compose(x => x.Item1);
            var wordProjection = baseProjection.Compose(x => x.Item3);
            var charCountProjection = baseProjection.Compose(x => x.Item3.Length);
            var timeProjection = baseProjection.Compose(x => x.Item2);

            //
            // Table Configurations describe how your table should be presented to the user: 
            // the columns to show, what order to show them, which columns to aggregate, and which columns to graph. 
            // You may provide a number of columns in your table, but only want to show a subset of them by default so as not to overwhelm the user. 
            // The table configuration class also exposes four (4) columns that viewers can recognize: Pivot Column, Graph Column, Left Freeze Column, Right Freeze Column
            // For more information about what these columns do, go to "Advanced Topics" -> "Table Configuration" in our Wiki. Link can be found in README.md
            //

            var config = new TableConfiguration("Word Time")
            {
                Columns = new[]
              {
                    WordColumn,
                    TableConfiguration.PivotColumn,
                    FileColumn,
                    CharacterCountColumn,
                    TableConfiguration.GraphColumn,
                    TimeColumn,
                },
                Layout = TableLayoutStyle.GraphAndTable,
            };

            //
            //
            //  Use the table builder to build the table. 
            //  Add and set table configuration if applicable.
            //  Then set the row count (we have one row per word) and then add the columns using AddColumn.
            //

            tableBuilder.AddTableConfiguration(config)
                .SetDefaultTableConfiguration(config)
                .SetRowCount(allWords.Count)
                .AddColumn(FileColumn, fileProjection)
                .AddColumn(WordColumn, wordProjection)
                .AddColumn(CharacterCountColumn, charCountProjection)
                .AddColumn(TimeColumn, timeProjection);
        }
    }
}
