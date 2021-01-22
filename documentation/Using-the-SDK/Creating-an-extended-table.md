# Creating an Extended Table

An Extended Table is a Table that leverages data from a Plugin 
that may not neccesarilly be shipped with said Plugin. Extended tables are
only able to leverage data from Cookers. If a Plugin exposes data through Cookers,
then you can author an Extended table to leverage said Data.

At a bare minimum, you must create a new class for your Extended Table.
This class __MUST__ have the following items:
1) The Table attribute
2) A static Table Descriptor property
3) A static BuildTable method.
4) Declarations for the Cooker(s) that provide the data that the Table is using.
    - May be either **RequiresCookerAttribute**s or declared in the TableDescriptor (2).

````cs

    // Denotes that this class exposes a Table
    //
    [Table]
    // One or more RequiresCooker attributes specifying the
    // Cookers that this Table uses for data. Alternatively,
    // you may use the 'requiredDataCookers' parameter of the
    // TableDescriptor constructor.
    //
    [RequiresCooker("Cooker/Path")]
    public class SampleExtendedTable
    {
        // This property is required to define your Table. This
        // tells the runtime that a Table is available, and that
        // any Cookers needed by the Table are to be scheduled for
        // execution.
        //
        public static readonly TableDescriptor TableDescriptor =
            new TableDescriptor(
                // Table ID
                // Table Name
                // Table Description,
                // Table Category
                requiredDataCookers: new List<DataCookerPath>
                {
                    // Paths to the Cookers that are needed for
                    // this table. This is not needed if you are
                    // using the RequiresCooker attributes.
                    //
                });
            );

        // This method is required so that the runtime can build your
        // table.
        //
        public static void BuildTable(
            ITableBuilder tableBuilder,
            IDataExtensionRetrieval tableData
        )
        {
            // Query cooked data from dataRetrieval, and use the
            // tableBuilder to build the table.
            //
        }
    }
````

There is no restriction on the Cookers on which your Table may depend. You
may author your own Cookers leveraging Cookers from many different plugins,
and have your Table depend on that. As long as you have Cooked Data, you
can create an Extended Table.

As long as the DLL with your Extended Tables is available to WPA or the
Engine, then your Table will be available for use.

# Examples

For some real-world examples of Extended Tables, see the [Tables](https://github.com/microsoft/Microsoft-Performance-Tools-Linux/tree/develop/LTTngDataExtensions/Tables)
exposed by our LTTng tools.