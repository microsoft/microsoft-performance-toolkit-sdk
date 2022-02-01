# Creating a Simple SDK Plugin

This document assumes you have already created a `ProcessingSource` and your plugin is using the simple plugin framework. For more details, refer to [Creating an SDK Plugin](./Creating-your-plugin.md).

---

The simple plugin framework is centered around the `CustomDataProcessor` class. Your plugin must create a concrete implementation of the abstract `CustomDataProcessor` class provided by the SDK.

In this framework, your `CustomDataProcessor` is responsible for
1. Processing the `DataSource(s)` opened by a user that your `ProcessingSource` supports (i.e. the `DataSource(s)` that passed the `IsDataSourceSupportedCore` check)
2. Building all of the `Tables` your `ProcessingSource` discovers

> :information_source: When using the simple plugin framework, a `Table` is referred to as a `Simple Table`.

This document is outlined into 4 distinct steps
* [Creating a CustomDataProcessor](#creating-a-customdataprocessor)
* [Creating a Simple Table](#creating-a-simple-table)
* [Building Your Simple Table](#building-your-simple-table)
* [Linking Your CustomDataProcessor to Your ProcessingSource](#linking-your-customdataprocessor-to-your-processingsource)

## Creating a CustomDataProcessor

1. Create a public class that extends the abstract class `CustomDataProcessor`.

   ```cs
   public sealed class SimpleCustomDataProcessor
      : CustomDataProcessor
   {
   }
   ```

2. Create a constructor that calls into the base class.

   ```cs
   public sealed class SimpleCustomDataProcessor
      : CustomDataProcessor
   {
      private string[] filePaths;

      public SimpleCustomDataProcessor(
         string[] filePaths,
         ProcessorOptions options,
         IApplicationEnvironment applicationEnvironment,
         IProcessorEnvironment processorEnvironment)
         : base(options, applicationEnvironment, processorEnvironment)
      {
         //
         // Store the file paths for all of the data sources this processor will eventually 
         // need to process in a field for later
         //

         this.filePaths = filePaths;
      }
   }
   ```

   This example assumes that we're using our `SimpleCustomDataProcessor` with the `ProcessingSource` created in [Creating an SDK Plugin](./Creating-your-plugin.md) that advertises support for `.txt` files beginning with `mydata`. Because of this, we are passing in the `filePaths` for all of the `mydata*.txt` files opened by the user.


3. Implement `ProcessAsyncCore`. This method will be called to process data sources passed into your `CustomDataProcessor`. Typically the data in the data source is parsed and converted from some raw form into something more 
   relevant and easily accessible to the processor.
   
   In this example, we're calling a  `ParseFiles` method that converts lines in each file opened to ficticious `LineItem` objects. In a 
   more realistic case, processing would probably be broken down into smaller units. For example, there might be logic 
   for parsing operating system processes and making that data queryable by time and or memory layout.

   This method is also typically where `this.dataSourceInfo` would be set (see below).

   ```cs
   public sealed class SimpleCustomDataProcessor
      : CustomDataProcessor
   {
      private string[] filePaths;
      private LineItem[] lineItems;

      public SimpleCustomDataProcessor(...) : base(...)
      {
         ...
      }

      protected override Task ProcessAsyncCore(
         IProgress<int> progress,
         CancellationToken cancellationToken)
      {
         this.lineItems = ParseFiles(this.filePaths, progress, cancellationToken);
      }
   }
   ```

   We pass `progress` and `cancellationToken` into the ficticious `ParseFiles` method so it can use them. It is good practice to report parsing progress back to the `IProgress<int>` passed into `ProcessAsyncCore`. For example, `ParseFiles` could begin by quickly getting a combined line count of all files being processed, and then report what % of lines have been processed after each line of a file is parsed.

4. Override the `GetDataSourceInfo` method. `DataSourceInfo` provides the driver some information about the data source
   to provide a better user experience. It is expected that this method will not be called before 
   `ProcessAsyncCore` because the data necessary to create a `DataSourceInfo` object might not be
   available beforehand.

   A `DataSourceInfo` contains three important pieces of information:
   - How long, in nanoseconds, did the first event occur relative to the start of the sources being processed
   - The duration, in nanoseconds, of the sources being processed
   - The UTC wallclock time of the start of the sources being processed

   ```cs
   public sealed class SimpleCustomDataProcessor
      : CustomDataProcessor
   {
      private string[] filePaths;
      private LineItem[] lineItems;
      private DataSourceInfo dataSourceInfo;

      public SimpleCustomDataProcessor(...) : base(...)
      {
         ...
      }

      protected override Task ProcessAsyncCore(
         IProgress<int> progress,
         CancellationToken cancellationToken)
      {
         ...

         this.dataSourceInfo = new DataSourceInfo(...)
      }

      public override DataSourceInfo GetDataSourceInfo()
      {
            return this.dataSourceInfo;   
      }
   }
   ```

   The parameters to `DataSourceInfo`'s constructor typically are created while parsing `DataSources`. For more help, refer to the [SimpleDataSource sample](../../samples/SimpleDataSource/README.md).

Our `SimpleCustomDataProcessor` is now ready to process the `DataSources` opened by a user. The last step is ensuring our `SimpleCustomDataProcessor` can build tables discovered by our `ProcessingSource`. However, before implementing this, we must first create a `Table`.

## Creating a Simple Table

For our `SimpleCustomDataProcessor` to build a table, the table must first be discovered by our `ProcessingSource`. By default, a `ProcessingSource` will "discover" each class defined in its assembly that meet the following criteria:
 - The class is public and concrete (not `abstract`)
 - The class is decorated with `TableAttribute`
 - The class exposes a `static public` property named "TableDescriptor" of type `TableDescriptor`

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

## Building Your Simple Table

Now that we have a `Table` defined, we can override the `BuildTableCore` method of our `CustomDataProcessor`. This method is responsible for instantiating a given table. This method is 
called for each table discovered by our `ProcessingSource`. 
The table to build is identified by the `TableDescriptor` passed in as a parameter to this method. If the CDP isn't interested 
in the given table, it may return immediately.

To build a `Table`, the `CustomDataProcessor` uses the `ITableBuilder` passed into `BuildTableCore`. Typically, the task of interacting with the `ITableBuilder` and building the `Table` is delegated to the `Table`'s class.

```cs
public sealed class SimpleCustomDataProcessor
   : CustomDataProcessor
{
   private string[] filePaths;
   private LineItem[] lineItems;
   private DataSourceInfo dataSourceInfo;

   public SimpleCustomDataProcessor(...) : base(...)
   {
      ...
   }

   protected override Task ProcessAsyncCore(
      IProgress<int> progress,
      CancellationToken cancellationToken)
   {
      ...
   }

   public override DataSourceInfo GetDataSourceInfo()
   {
      ...
   }

   protected override void BuildTableCore(
      TableDescriptor tableDescriptor,
      ITableBuilder tableBuilder)
   {
      switch (tableDescriptor.Guid)
      {
         case var g when (g == WordTable.TableDescriptor.Guid):
            new WordTable(this.lineItems).Build(tableBuilder);
            break;
         default:
            break;
      }
   }
}
```

In this plugin framework, how a table is created is left up to the plugin author. In this example, we are using pattern matching to determine the `Table` attempting to be built. When we're asked to build the `WordTable`, we first create a new instance that has a reference to all of the parsed `LineItem` objects and then ask that instance to build itself with the `tableBuilder` parameter.
   
Here, we are calling a ficticious `Build` method on our `WordTable`. For documentation on interacting with an `ITableBuilder,` refer to [Building a Table](./Building-a-table.md).

## Linking Your CustomDataProcessor to Your ProcessingSource

Now that our `CustomDataProcessor` is finished and we have a `Table` to build, the final step is linking our `SimpleCustomDataProcessor` to `MyProcessingSource`.

```cs
[ProcessingSource(...)]
[FileDataSource(...)]
public class MyProcessingSource : ProcessingSource
{
   public MyProcessingSource() : base()
   {
   }

   protected override bool IsDataSourceSupportedCore(IDataSource dataSource)
   {
      ...
   }

   protected override ICustomDataProcessor CreateProcessorCore(
      IEnumerable<IDataSource> dataSources,
      IProcessorEnvironment processorEnvironment,
      ProcessorOptions options)
   {
      return new SimpleCustomDataProcessor(
         dataSources.Select(ds => ds as FileDataSource).Select(fds => fds.FullPath),
         options,
         this.ApplicationEnvironment,
         processorEnvironment);
   }
}
```

With `CreateProcessorCore` implemented, our plugin is done and ready to use!

# Video Walkthrough

A video tutorial of making a simple SDK plugin can be found in the [SQL plugin sample](../../samples/SqlPlugin).

# Next Steps

Now that we've seen how to create a simple SDK plugin, let's see how we could have created this same plugin with the data-processing pipeline framework. Continue reading at [Using the SDK/Creating a Data-Processing Pipeline](./Creating-a-pipeline.md)
