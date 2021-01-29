# Creating an Extended Table

An __extended table__ is a `Table` that leverages data from one or more `DataCooker`s, 
including cookers that may not neccesarilly be shipped with said `Table`. If a plugin 
exposes data through cookers, then you can author an extended table to leverage said data.

At a bare minimum, you must create a new class for your extended table.
This class must have the following items:
1) The `Table` attribute
2) A static `TableDescriptor` property
3) A static `BuildTable` method.
4) Declarations for the cooker(s) that provide the data that the `Table` is using.
    - These may be either `RequiresCookerAttribute`s or be declared in the `TableDescriptor`.

````cs

    // Denotes that this class exposes a Table
    [Table]

    //
    // One or more RequiresCooker attributes specifying the
    // Cookers that this Table uses for data. Alternatively,
    // you may use the 'requiredDataCookers' parameter of the
    // TableDescriptor constructor.
    //
    [RequiresCooker("Cooker/Path")]
    public class SampleExtendedTable
    {
        //
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
                });
            );

        //
        // This method, with this exact signature, is required so that the runtime can 
        // build your table once all cookers have processed their data.
        //
        public static void BuildTable(
            ITableBuilder tableBuilder,
            IDataExtensionRetrieval requiredData
        )
        {
            //
            // Query cooked data from requiredData, and use the
            // tableBuilder to build the table.
            //
        }
    }
````

There is no restriction on the cookers on which your `Table` may depend. You
may author your own cookers leveraging cookers from many different plugins,
and have your `Table` depend on that. As long as you have cooked data, you
can create an extended table.

As long as the DLLs that contain
* Your extended table and
* All of the data cookers (and their dependencies) your table requires
are loaded into the SDK runtime, then your Table will be available for use.

# Examples

For some real-world examples of extended tables, see the 
[tables exposed by our LTTng tools](https://github.com/microsoft/Microsoft-Performance-Tools-Linux/tree/develop/LTTngDataExtensions/Tables).

# Video Walkthrough

A video tutorial of making a data processing pipeline and extended table can be found in the [SQL plugin sample](../../samples/SqlPlugin).

# Next Steps

This documentation marks the end of all necessary information to begin creating extensible, well-strucutred 
SDK plugins. For additional resources, you may browse our [samples folder](../../samples).

Our team is working on creating additional documentation and samples for more advanced features of the SDK.