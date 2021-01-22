# Creating a Simple Custom Data Source

A Custom Data Source (CDS) uses the SDK to create a data processing pipeline for any data sources, typically files, 
that the CDS supports. Notably, a CDS acts as the entry point for the SDK runtime to discover and utilizes these data 
pipelines. An SDK Plugin may contain more than one CDS.

A standard CDS will utilize a custom data processor (CDP) to process the data sources that the SDK runtime provides it.

A plugin may also define tables that get utilized by viewers such as Windows Performance Analyzer (WPA) to display the 
processed data in an organized, interactable collection of rows. Tables are discovered and passed into CDPs by the SDK 
runtime.

The SDK also supports advanced data processing pipelines through data cookers and extensions, but these topics are 
covered in the advanced documentaton. Refer to [Overview](../Overview.md) for more information.

## Corresponding Sample

Refer to /samples/SimpleDataSource for source code that implements the steps outlined in this file.

## Requirements of a Simple Data Source

The following are required for a Simple Data Source:

1. Create a public class that implements the abstract class `CustomDataSourceBase`. This is your custom data source 
   (CDS).
2. Create a public class that implements the abstract class `CustomDataProcessorBase`. This your custom data processor 
   (CDP).
3. Create one or more data tables classes. These classes must:
   - Be public and static.
   - Be decorated with `TableAttribute`.
   - Expose a static public field or property named TableDescriptor of type `TableDescriptor` which provides information
     about the table. If these requirements are not met, the SDK runtime will not be able to find and pass your tables 
     to your CDP.

## Implementing a Custom Data Source Class

1. Create a public class that extends the abstract class `CustomDataSourceBase`. Note that the class is decorated with 
   two attributes: `CustomDataSourceAttribute` and `FileDataSourceAttribute`. The former is used by the SDK runtime to 
   locate Custom Data Sources. The latter identifies a type of data source that the CDS can consume.

   ```
   [CustomDataSource(
      "{F73EACD4-1AE9-4844-80B9-EB77396781D1}",  // The GUID must be unique for your Custom Data Source. You can use 
                                                // Visual Studio's Tools -> Create Guid… tool to create a new GUID
      "Simple Data Source",                      // The Custom Data Source MUST have a name
      "A data source to count words!")]          // The Custom Data Source MUST have a description
   [FileDataSource(
      ".txt",                                    // A file extension is REQUIRED
      "Text files")]                             // A description is OPTIONAL. The description is what appears in the 
                                                // file open menu to help users understand what the file type actually 
                                                // is. 
   public class SimpleCustomDataSource : CustomDataSourceBase
   {
   }
   ```

2. Overwrite the SetApplicationEnvironmentCore method. The IApplicationEnvironment parameter is stored in the base 
   class' ApplicationEnvironment property.

      **Note**: This method will be made virtual in the base class in the future as nothing needs to be done here

   ```
   protected override void SetApplicationEnvironmentCore(IApplicationEnvironment applicationEnvironment)
   {
   }
   ```

3. Overwrite the CreateProcessorCore method. When the SDK needs to process files your CDS supports, it will obtain an 
   instance of your CDP by calling this method. This is also where your CDS learns about the files, passed as 
   `IDataSource`s, that it will need to process.

   ```
   protected override ICustomDataProcessor CreateProcessorCore(
      IEnumerable<IDataSource> dataSources,
      IProcessorEnvironment processorEnvironment,
      ProcessorOptions options)
   {
      //
      // Create a new instance of a class implementing ICustomDataProcessor here to process the specified data 
      // sources.
      // Note that you can have more advanced logic here to create different processors if you would like based 
      // on the file, or any other criteria.
      // You are not restricted to always returning the same type from this method.
      //

      return new SimpleCustomDataProcessor(
            dataSources.Select(x => x.GetUri().LocalPath).ToArray(),
            options,
            this.applicationEnvironment,
            processorEnvironment,
            this.AllTables,
            this.MetadataTables);
   }
   ```
4. Overwrite the IsFileSupportedCore method. This is where your class will determine if a given file contains data 
   appropriate to your CDS. For example, if your CDS consumes .xml files, not all .xml files will be valid for your
   CDS. Use this method as an opportunity to filter out the files that aren't consumable by this CDS.  
   :warning: This method will be changed before 1.0 release - more details to follow.
   ```
   protected override bool IsFileSupportedCore(string path)
   {
      //
      // This method is called for every file whose filetype matches the one declared in the FileDataSource attribute. It may be useful
      // to peek inside the file to truly determine if you can support it, especially if your CDS supports a common
      // filetype like .txt or .csv.
      // For this sample, we'll always return true for simplicity.
      //

      return true;
   }
   ```

## Implementing the Custom Data Processor Class

1. Create a public class that extends the abstract class `CustomDataProcessorBase`.

   ```
   public sealed class SimpleCustomDataProcessor
      : CustomDataProcessorBase
   {
   }
   ```

2. Create a constructor that calls into the base class.

   ```
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
   ```


3. Implement `ProcessAsyncCore`. This method will be called to process data sources passed into the Custom Data 
   Processor. Typically the data in the data source is parsed and converted from some raw form into something more 
   relevant and easily accessible to the processor.
   
   In this simple sample, a comma-delimited text file is parsed into event structures and stored for later use. In a 
   more realistic case, processing would probably be broken down into smaller units. For example, there might be logic 
   for parsing operating system processes and making that data queryable by time and or memory layout.

   ```
   protected override Task ProcessAsyncCore(
      IProgress<int> progress,
      CancellationToken cancellationToken)
   {
      ...
   }
   ```

4. Override the GetDataSourceInfo method. `DataSourceInfo` provides the driver some information about the data source
   to provide a better user experience. It is expected that this method will not be called before 
   `ProcessAsync`/`ProcessAsyncCore` because the data necessary to create a `DataSourceInfo` object might not be
   available beforehand.

   ```
   public override DataSourceInfo GetDataSourceInfo()
   {
         return this.dataSourceInfo;   
   }
   ```

5. Override the BuildTableCore method. This method is responsible for instantiating a given table. This method is called for each table identified by the SDK runtime as part of this plugin (see more on this in the 'Create Tables' section). The table to build is identified by a `TableDescriptor`, passed in as a parameter to this method. If the CDP isn't interested in the given table, it may return immediately.

   ```
   protected override void BuildTableCore(
      TableDescriptor tableDescriptor,
      Action<ITableBuilder, IDataExtensionRetrieval> buildTableAction,
      ITableBuilder tableBuilder)
   {
   }
   ```
   
   How a table is created is left up to the plugin author. The sample for this documentation uses reflection to instantiate an instance of the table class.

   After the table object is created, the `ITableBuilder` parameter is used to populate the table with columns and row data. In this example, the build logic exists in a `Build` method on the table, which is called by this method.

## Create tables

Simple custom data sources provide tables as output. A table is set of columns and rows grouped together to provide output for related data. So tables must be created to have any output.

Here are the requirements for a table to be discovered by the SDK runtime:
   - Be public, static, and concrete.
   - Be decorated with `TableAttribute`.
   - Expose a static public field or property named TableDescriptor of type `TableDescriptor` which provides information
     about the table. If these requirements are not met, the SDK runtime will not be able to find and pass your tables 
     to your CDP.

Here's an example from the simple example:

```
[Table]                      
public sealed class WordTable
   : TableBase
{
   public static TableDescriptor TableDescriptor => new TableDescriptor(
      Guid.Parse("{E122471E-25A6-4F7F-BE6C-E62774FD0410}"), // The GUID must be unique across all tables
      "Word Stats",                                         // The Table must have a name
      "Statistics for words",                               // The Table must have a description
      "Words");                                             // A category is optional. It useful for grouping 
                                                            // different types of tables in the viewer's UI.
}
```

Sometime after `ProcessAsyncCore` finishes (at least for typical SDK runtime consumers who ask the SDK to build tables 
only *after* asking it to process sources), the SDK runtime will call `BuildTableCore` on your CDP for each table 
"hooked up" to it. This method receives the `TableDescriptor` of the table it must create, and is responsible for 
populating the  `ITableBuilder` with the columns that make up the table being constructed. This is most easily done by 
delegating the work of populating the `ITableBuilder` to an instance of the class/table described by the 
`TableDescriptor`.

Note that tables are "hooked up" to CDPs automatically by the SDK. **Every class that meets the requirements listed above 
is automatically hooked up to any CDPs declared in that table's assembly.**

In the sample code, the `WordTable` has a `Build` method that is called by `SimpleCustomDataProcessor`. The key code in this method uses the `ITableBuilder` to generate the table.

```
public override void Build(ITableBuilder tableBuilder)
{
   ...

   tableBuilder.AddTableConfiguration(config)
         .SetDefaultTableConfiguration(config)
         .SetRowCount(allWords.Count)
         .AddColumn(FileColumn, fileProjection)
         .AddColumn(WordColumn, wordProjection)
         .AddColumn(CharacterCountColumn, charCountProjection)
         .AddColumn(TimeColumn, timeProjection);
}
```

The `TableConfiguration` passed into `ITableBuilder.AddTableConfiguration` and `ITableBuilder.SetDefaultConfiguration` details how the table should appear.

`ITableBuilder.SetRowCount` establishes the number of rows for the table, and returns an `ITableBuilderWithRowCount`.

Call `ITableBuilderWithRowCount.AddColumn` to establish a column on the table. Each column requires a `ColumnConfiguration` to describe the column, and an `IProjection<,>` which allows table cell data to be retrieved for a given row number.