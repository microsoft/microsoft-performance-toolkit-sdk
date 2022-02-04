# Creating a Table

This document assumes you have already created a `ProcessingSource` and data-processing pipeline. For more details, refer to [Creating an SDK Plugin](./Creating-a-pipeline.md).

---

A `Table` leverages data from one or more `DataCooker`s, 
including cookers that may not necessarily be shipped with said `Table`, to build itself. If a plugin 
exposes data through cookers, then you can author a table to leverage said data.

Creating a table involves two keys steps:
* [Declaring the Table](#declaring-the-table)
* [Integrating the Table with our Data-Processing Pipeline](#integrating-the-table-with-our-data-processing-pipeline)

## Declaring the Table

For our `Table` to work, it must be discovered by the SDK runtime. To do this, the runtime will look for classes that:
 - Are public and concrete (not `abstract`)
 - Are decorated with `TableAttribute`
 - Exposes a `static public` property named "TableDescriptor" of type `TableDescriptor`

Let's define a table `WordTable` that will eventually have one row for each distinct word in the `DataSources` processed.

```cs
[Table]                      
public sealed class WordTable
{
   public static TableDescriptor TableDescriptor => 
      new TableDescriptor(
         Guid.Parse("{E122471E-25A6-4F7F-BE6C-E62774FD0410}"), // The GUID must be unique across all tables
         "Word Stats",                                         // The Table must have a name
         "Statistics for words",                               // The Table must have a description
         "Words");                                             // A category is optional. It useful for grouping 
                                                               // different types of tables in the SDK Driver's UI.
}
```

## Integrating the Table with our Data-Processing Pipeline

Our `WordTable` is going to receive data from `DataCookers` and then use that data to build itself. To accomplish this we must
1) Declare for the cooker(s) that provide the data that the `Table` is using.
    - These may be either `RequiresCookerAttribute`s or be declared in the `TableDescriptor`.
2) Add a `static void BuildTable` method that uses cooked data to build itself through an `ITableBuilder`

```cs
[Table]
[RequiresCompositeCooker(nameof(SampleCompositeCooker)]
public sealed class WordTable
{
    public static TableDescriptor TableDescriptor => 
        new TableDescriptor(
            Guid.Parse("{E122471E-25A6-4F7F-BE6C-E62774FD0410}"),
            "Word Stats",
            "Statistics for words",
            "Words",
            requiredDataCookers: new List<DataCookerPath>
            {
                //
                // We can also list required data cookers here
                // instead of using the RequiresCompositeCooker attribute above
                //
            });

    //
    // This method, with this exact signature, is required so that the runtime can 
    // build your table once all cookers have processed their data.
    //
    public static void BuildTable(
        ITableBuilder tableBuilder,
        IDataExtensionRetrieval requiredData
    )
    {
        var data = 
            requiredData.QueryOutput<List<FurtherCookedLineItem>>(new DataOutputPath(SampleCompositeCooker.DataCookerPath, nameof(SampleSourceCooker.FurtherCookedLineItems)));

        //
        // Build the table using the above data and the ITableBuilder parameter
    }
}
```

For documentation on interacting with an `ITableBuilder,` refer to [Building a Table](./Building-a-table.md).

There are no restrictions on the cookers which your `Table` may depend upon. For 
example, your `Table` can depend solely on cookers defined in the `Table`'s assembly. Or, 
your `Table` can depend on cookers from multiple plugins. As long as you have cooked data, you
can create a table.

In short, as long as the SDK runtime has loaded your table and all of the data cookers (and their dependencies) your table requires your table will be available to use.

With the `WordTable` finished, our plugin is done and ready to use!

# Examples

For some real-world examples of tables, see the 
[tables exposed by our LTTng tools](https://github.com/microsoft/Microsoft-Performance-Tools-Linux/tree/develop/LTTngDataExtensions/Tables).

# Video Walkthrough

A video tutorial of making a data-processing pipeline and table can be found in the [SQL plugin sample](../../samples/SqlPlugin).

# Next Steps

This documentation marks the end of all necessary information to begin creating extensible, well-structured 
SDK plugins. For additional resources, you may browse our [samples folder](../../samples).

For more advanced usage of the SDK, please see the following:

- [Overview of Advanced Topics](Advanced/README.md)
