using Microsoft.Performance.SDK;
using Microsoft.Performance.SDK.Processing;
using System;
using System.Collections.Generic;

namespace SampleCustomDataSource.Tables
{
    //
    // This is a sample base class for all regular and metadata tables in your project which helps simplify management of them.
    // 
    // A table is a logical collection of similar data points. 
    // Things like CPU Usage, Thread Lifetimes, File I/O, etc. can be mapped to tables.
    //
    // The SDK does not have an explicit table interface so as to give you flexibility in how you implement your tables.
    // All that matters is that you have some way of getting the data out of the data files and into the ITableBuilder passed into
    // the BuildTable method on your CustomDataProcessor.
    //

    public abstract class TableBase
    {
        protected TableBase(IReadOnlyDictionary<string, IReadOnlyList<Tuple<Timestamp,string>>> lines)
        {
            this.Lines = lines;
        }

        //
        // In this sample we are going to assume the files will fit in memory,
        // and so we will make sure all tables have access to the collection of lines in the file.
        //

        public IReadOnlyDictionary<string, IReadOnlyList<Tuple<Timestamp, string>>> Lines { get; }

        //
        // All tables will need some way to build themselves via the ITableBuilder interface. This method is
        // what gets called by our custom data processor to populate the ITableBuilder with columns specific
        // for the table being created. It is NOT called directly by the SDK.
        //

        public abstract void Build(ITableBuilder tableBuilder);
    }
}
