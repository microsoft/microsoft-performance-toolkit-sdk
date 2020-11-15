# Plug-in Creation for Performance SDK

This document will introduce two paths for creating Performance Toolkit SDK (SDK) plug-ins:

1) Simple Data Source
2) Extensible Data Source

## Overview

These two options offer a developer some flexibility in creating a plug-in.
Below are some points of comparison.

### Complexity

A Simple Data Source requires less code to get started.

An Extensible Data Source fits into a framework that requires additional code at the start of the project.

### Data Accessibility

Both options expose Tables which are requested when processing a data source.

An Extensible Data Source offers programmatic access to data through Data Cookers (see below).

Coming Soon: Both options support programmatic data access at the Table level.

#### Data Cookers

A Data Cooker consumes **raw data**, performs some processing, and exposes **cooked data**. This processing happens once during the lifetime of the Data Cooker and the cooked data does not change.

There are two types of data cookers:

1) Source Data Cookers
2) Composite Data Cookers

#### Source Data Cookers

These Data Cookers consume data from a single Custom Data Source. They take part in processing of a data source.

#### Composite Data Cookers

These Data Cookers may consume data from Source Data Cookers or other Composite Data Cookers. They may target Source Data Cookers from multiple Custom Data Sources. Composite Data Cookers process data after source processing is complete.

### Extensibility

Tables may not be added to a simple data source outside of its image (DLL).

Extensible Data Sources optionally allow new Tables and Data Cookers to be created in other images.

## Implement a Simple Data Source

Sample source code: ./SimpleDataSource

To create a simple data source, perform the following:

1) Create a public class that implements the abstract class CustomDataSourceBase.
2) Create a public class that implements the abstract class CustomDataProcessorBase.
3) Create one or more data tables classes. These classes must:
   - Be public and static.
   - Be decorated with TableAttribute.
   - Expose a static public field or property named TableDescriptor of type TableDescriptor which provides information about the table.

The custom data source class will create a new instance of the custom data processor class when ProcessAsyncCore is called.
The custom data processor will a data source when ProcessAsyncCore is called.
Finally, the custom data processor class will create instances of the tables when BuildTableCore is called.

## Implement an Extensible Data Source

Sample source code: ./DataSourceWithProcessingAndExtensions/Sample

As with a Simple Data Source, an Extensible Data Source needs to create a public class that implements the CustomDataSourceBase abstract class.

An Extensible Data Source fits into a framework for processing a data source and exposing the data through a set of Data Cookers and Tables. To integrate into this framework, the Extensible Data Source needs to define three types that will be used as generics throughout:

Parameter|Description
:---:|:---
*T*|The type used to package event data for this Extensible Data Source. Note that this type must implement IKeyedDataType<TKey>.
*TContext*|The type used to package contextual data for an event for this Extensible Data Source.
*TKey*|The type used to identify events for distribution to Source Data Cookers.

IKeyedDataType<TKey> inherits from IComparable<TKey> and has one additional method: TKey GetKey(). The framework uses the key to determine if an event should be distributed to a Source Data Cooker for further processing.

Similar to the Simple Data Source, an Extensible Data Source will need a Data Processor. Rather than implementing the base class CustomDataProcessorBase directly, the Data Processor will implement CustomDataProcessorBaseWithSourceParser<T, TContext, TKey>. The additional functionality is implemented by the base class after the Source Parser is passed into the constructor. The Source Parser is a new class required by an Extensible Data Source, and is described in some detail below.

Finally, implement any Data Cookers and Tables to expose data from the data source. More on these below.

### Source Parser

A Source Parser is responsible for parsing a data source into individual records or events and submitting these events for further processing by Source Data Cookers.

The Source Parser is responsible for packaging each parsed event (type *T*) and any additional context (*TContext*) about the event for consumption by Source Data Cookers that interact with this parser.

To create a Source Parser, define a public class that inherits from SourceParserBase<T, TContext, TKey>.

The framework will call ProcessSource on the parser, passing in an ISourceDataProcessor<T, TContext, TKey> parameter. For each event the parser produces, it will call ISourceDataProcessor<T, TContext, TKey>.ProcessDataElement, passing in the event and event context (and a cancellation token). This event will be distributed to Source Data Cookers as appropriate.

### Source Data Cookers

As mentioned previously, a data cooker consumes *raw* data and processes it, producing *cooked* data. At a minimum, an Extensible Data Source needs to have Source Data Cookers to transform raw event data (type *T*) into something more meaningful to be exposed for programmatic access or for Tables.

A Source Data Cooker implements abstract class BaseSourceDataCooker<T, TContext, TKey>. It needs to be public so that the for the framework to automatically find it.

Each Data Cooker is identified by a path of type DataCookerPath. This unique path is passed into BaseSourceDataCooker<T, TContext, TKey> constructor. This path is used to find and access data from a Data Cooker.

Override the Description property to provide a short text description of the Data Cooker.

Override the DataKeys property to set which events the Source Data Cooker should receive. Alternatively, override the Options property, returning SourceDataCookerOptions.ReceiveAllDataElements, to receive all events. Note that doing this for many Source Data Cookers will make source processing take longer.

Override the CookDataElement method to process events.

Optionally override the EndDataCooking method if there is work to be performed after all events have been received.

To expose cooked data, Create a get property and decorate it with the DataOutputAtrribute. This data will now be addressable using the DataOutputPath that combines the DataCookerPath of the Data Cooker with the property name.

If a Source Data Cooker needs access to data from another Source Data Cooker, override the RequiredDataCookers property, adding a DataCookerPath for each required Source Data Cooker. Then, override BeginDataCooking and store the ICookedDataRetrieval parameter. This can be used to access the data output from the requested Source Data Cookers.

### Composite Data Cookers

Composite data cookers process data after Source Data Cookers have finished processing their source directly. They may consume data from Source Data Cookers and from other Composite Data Cookers.

