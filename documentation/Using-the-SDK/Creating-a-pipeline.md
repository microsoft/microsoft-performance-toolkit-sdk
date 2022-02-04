# Creating a Data-Processing Pipeline

This document assumes you have already created a `ProcessingSource` and your plugin is using the data-processing pipeline plugin framework. For more details, refer to [Creating an SDK Plugin](./Creating-your-plugin.md).

> :exclamation: Before reading this document, please read [the overview of the Data-Processing Pipelines' architecture](../Architecture/The-Data-Processing-Pipeline.md). This tutorial assumes you are familiar with the concepts of `SourceParsers`, `SourceDataCookers`, `CompositeDataCookers`, and `DataOutputs`.

---

The data-processing pipeline plugin framework is centered around a `SourceParser` and one-or-more `DataCookers`. The `CustomDataProcessor` your `ProcessingSource` creates delegates the task of parsing `DataSources` off to a `SourceParser`, who in turn emits __events__ that flow through `DataCookers`. In this framework, `Tables` are responsible for "building themselves" by querying `DataCookers` for their `DataOutputs`.

There are 4 distinct steps in creating a Data-Processing Pipeline:
1. [Creating a SourceParser](#creating-a-sourceparser)
2. [Creating a CustomDataProcessorWithSourceParser](#creating-a-customdataprocessorwithsourceparser)
3. [Linking Your CustomDataProcessor to Your ProcessingSource](#linking-your-customdataprocessor-to-your-processingsource)
4. [Creating a DataCooker](#creating-a-datacooker)

## Creating a SourceParser

To begin, we must create a `SourceParser`. A `SourceParser` parses data from a data source into data that can be 
manipulated by your application. For example, a `SourceParser` may parse an ETW 
`.etl` file into a stream of `Event` objects.

A source parser will inherit from `SourceParserBase`:
````cs
public abstract class SourceParserBase<T, TContext, TKey>
{
    ...
}
````

where
* `T` is the keyed type of the objects being parsed/emitted
* `TContext` is an arbitrary type where you can store metadata about the parsing
* `TKey` is how the data type `T` is keyed.

Let's create a `LineItem` class that will get emitted by the `SourceParser` class we will create. Each `LineItem` is an object representing information about a line in one of the `mydata*.txt` files opened by the user. Since `LineItem` gets emitted, it needs to be an `IKeyedDataType`. We'll use the line number as the key.

```cs
public class LineItem : IKeyedDataType<int>
{
    private int lineNumber;
    ... // other fields

    public LineItem(lineNumber, ...)
    {
        this.lineNumber = lineNumber;
        ...
    }

    public int GetKey()
    {
        return this.lineNumber;
    }
}

```

Source parsers expose an `Id` property that is used to identify itself.
This property is used by `SourceDataCookers` to access the data emitted by the `SourceParser`.

Let's create a `SampleSourceParser` that emits `LineItem` events.

```cs
public sealed class SampleSourceParser
    : SourceParserBase<LineItem, SampleContext, int>
{
    private SampleContext context;
    private IEnumerable<IDataSource> dataSources;
    private DataSourceInfo dataSourceInfo;

    public SampleSourceParser(IEnumerable<IDataSource> dataSources)
    {
        this.context = new SampleContext();

        // Store the datasources so we can parse them later
        this.dataSources = dataSources;
    }

    // The ID of this Parser.
    public override string Id => nameof(SampleSourceParser);

    // Information about the Data Sources being parsed.
    public override DataSourceInfo DataSourceInfo => this.dataSourceInfo;

    public override void ProcessSource(
        ISourceDataProcessor<SampleDataObject, SampleContext, int> dataProcessor,
        ILogger logger,
        IProgress<int> progress, CancellationToken cancellationToken)
    {
        var totalNumberLines = GetLineCount(this.dataSources);
        var linesProcessed = 0;

        foreach (var dataSource in this.dataSources)
        {
            if (!(dataSource is FileDataSource fileDataSource))
            {
                continue;
            }

            using (StreamReader reader = GetStreamReader(fileDataSource))
            {
                while (!reader.EndOfStream)
                {
                    var line = reader.ReadLine();
                    LineItem lineItem = ParseLineItem(line);

                    dataProcessor.ProcessDataElement(lineItem, this.context, cancellationToken);

                    linesProcessed++;
                    progress.Report((double)linesProcessed / (double)totalNumberLines * 100);

                }
            }
        }

        this.dataSourceInfo = new DataSourceInfo(...)
    }
}
```

For brevity, this example refers to several ficticious methods and objects such as `GetLineCount`, `ParseLineItem`, and `SampleContext`. The import part, however, is the call to `dataProcessor.ProcessDataElement`. This method is what emits the `LineItem` we parsed. The emitted event will get sent through the data-processing pipeline into any `SourceCookers` hooked up to this `SourceParser`.

Lastly, the parameters to `DataSourceInfo`'s constructor are omitted here, since their calculations are highly dependant on the actual data being processed. For more help, refer to the [SqlPluginWithProcessingPipeline sample](../../samples/SqlPlugin/SqlPluginWithProcessingPipeline).

## Creating a CustomDataProcessorWithSourceParser

Now that we have a `SourceParser`, we can create a `CustomDataProcessor` that uses it. In this framework, our `CustomDataProcessor` does not do much work. Instead, it delegates the task of parsing our `DataSources` to the `SourceParser` we just created.

Since we're using the data-processing pipeline framework, we need to extend `CustomDataProcessorWithSourceParser<T, TContext, TKey>`. These generic parameters must be the same as the ones used in our `SourceParser`.

```cs
public sealed class SampleProcessor
    : CustomDataProcessorWithSourceParser<SampleDataObject, SampleContext, int>
{
    public SampleProcessor(
        ISourceParser<SampleDataObject, SampleContext, int> sourceParser, 
        ProcessorOptions options,
        IApplicationEnvironment applicationEnvironment, 
        IProcessorEnvironment processorEnvironment) 
        : base(sourceParser, options, applicationEnvironment, processorEnvironment)
    {
    }
}
```

Passing the `sourceParser` parameter down to the base constructor hooks up our `SourceParser` to the data-processing pipeline we're creating. That's all the work we need to do for this `CustomDataProcessor`!

## Linking Your CustomDataProcessor to Your ProcessingSource

Now that we have a finished `CustomDataProcessor`, let's go back to the `ProcessingSource` we created and fill in `CreateProcessorCore`.

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
        var parser = new SampleSourceParser(dataSources);

        return new SampleProcessor(
            parser,
            options,
            this.ApplicationEnvironment,
            processorEnvironment);
   }
}
```

## Creating a DataCooker

`DataCookers` transform data from one type to another. A cooker will transform the 
output from one or more sources, optionally producing new `DataOutput`s for other cookers 
or end user applications to consume.

`SourceCookers` take events emitted from a `SourceParser` to produce `DataOutputs`.

`CompositeCookers` take data from one or more cookers (source or composite) 
to create `DataOutputs`.

Let's create a `SourceDataCooker` that takes events from our `SampleSourceParser`.

```cs
public sealed class SampleSourceCooker
    : SourceDataCooker<LineItem, SampleContext, int>
{
    public static readonly DataCookerPath DataCookerPath = 
        DataCookerPath.ForSource(nameof(SampleSourceParser), nameof(SampleSourceCooker));

    public SampleSourceCooker() 
        : base(DataCookerPath)
    {
        this.CookedLineItems = new List<CookedLineItem>();
    }

    public override string Description => "My awesome SourceCooker!";

    public override ReadOnlyHashSet<int> DataKeys => 
        new ReadOnlyHashSet<int>(new HashSet<int>(new[] { 1, }));

    // Defines a DataOutput
    [DataOutput]
    public List<CookedLineItem> CookedLineItems { get; }

    public override DataProcessingResult CookDataElement(
        LineItem data, 
        SampleContext context, 
        CancellationToken cancellationToken)
    {
        //
        // Process each data element. This method will be called once
        // for each SampleDataObject emitted by the SourceParser.
        //

        var cookedLineItem = TransformLineItem(data);

        this.CookedLineItem.Add(cookedLineItem)

        //
        // Return the status of processing the given data item.
        //
        return DataProcessingResult.Processed;
    }
}

```

This `SourceDataCooker` will transform each `LineObject` emitted by our `SourceParser` into a `CookedLineItem` and expose all of the cooked data through the `CookedLineItems` `DataOutput`.

If we wanted to further cook each `CookedLineItem` into a `FurtherCookedLineItem`, we can create a `CompositeDataCooker`:

```cs
// A CompositeCooker
public sealed class SampleCompositeCooker
    : CookedDataReflector,
      ICompositeDataCookerDescriptor
{
    public static readonly DataCookerPath DataCookerPath =
        DataCookerPath.ForComposite(nameof(SampleCompositeCooker));

    public SampleCompositeCooker()
        : base(DataCookerPath)
    {
        this.FurtherCookedLineItems = new List<FurtherCookedLineItem>();
    }

    public string Description => "Composite Cooker";

    public DataCookerPath Path => DataCookerPath;

    // Defines a DataOutput
    [DataOutput]
    public List<SampleCompositeOutput> FurtherCookedLineItems { get; }

    // Declare all of the cookers that are used by this CompositeCooker.
    public IReadOnlyCollection<DataCookerPath> RequiredDataCookers => new[]
    {
        SampleSourceCooker.DataCookerPath,
    };

    public void OnDataAvailable(IDataExtensionRetrieval requiredData)
    {
        var cookedLineItems = 
            requiredData.QueryOutput<List<CookedLineItem>>(new DataOutputPath(SampleSourceCooker.DataCookerPath, nameof(SampleSourceCooker.CookedLineItems)));

        this.FurtherCookedLineItems = FurtherCookLineItems(cookedLineItems);
    }
}
```

Here, we declare that this `CompositeCooker` depends on data from the `SampleSourceCooker` we created above. __The SDK will ensure that every required cooker has finished processing its data before `CookedDataReflector.OnDataAvailable` is called__.

To get data from a `DataCooker`, the cooker must be __queried__ using the `IDataExtensionRetrieval` passed into `OnDataAvailable`. In the example above, we query `SampleSourceCooker` for its `CookedLineItems` `DataOutput`.


# Next Steps
Now that we've created a data-processing pipeline we can create a `Table` that uses data exposed by our `DataCookers` to build itself. Continue reading at [Using the SDK/Creating an Table](./Creating-a-table.md)