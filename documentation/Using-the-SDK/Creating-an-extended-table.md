# Creating an Extended Table

An __extended table__ is a `Table` that leverages data from one or more `DataCooker`s, 
including cookers that may not necessarily be shipped with said `Table`. If a plugin 
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

There are no restrictions on the cookers which your `Table` may depend upon. For 
example, your `Table` can depend solely on cookers defined in the `Table`'s assembly. Or, 
your `Table` can depend on cookers from multiple plugins. As long as you have cooked data, you
can create an extended table.

In short, as long as long as the SDK runtime has loaded
1) Your extended table and
2) All of the data cookers (and their dependencies) your table requires

then your table will be available to use.

# Examples

For some real-world examples of extended tables, see the 
[tables exposed by our LTTng tools](https://github.com/microsoft/Microsoft-Performance-Tools-Linux/tree/develop/LTTngDataExtensions/Tables).

# Video Walkthrough

A video tutorial of making a data processing pipeline and extended table can be found in the [SQL plugin sample](../../samples/SqlPlugin).

# Next Steps

This documentation marks the end of all necessary information to begin creating extensible, well-strucutred 
SDK plugins. For additional resources, you may browse our [samples folder](../../samples).

For more advanced usage of the SDK, please see the following:

- [Overview of Advanced Topics](Advanced/Overview.md)
