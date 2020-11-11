# Plug-in Options

This document will introduce two paths for creating Performance Toolkit SDK (SDK) plug-ins:

1) Simple Data Source
2) Extensible Data Source

## Overview

These two options offer a developer some flexibility in creating a plug-in.
Below are some points of comparison.

### Complexity

A Simple Data Source requires less code to get started.

An Extensible Data Source fits into a framework that requries additional code at the start of the project.

### Data Accessibility

Both options support data processing at the Table level - meaning table data may be accessed programmatically.

An Extensible Data Source also offers more granular data access through Data Cookers.

#### Data Cookers

A Data Cooker consumes **raw data**, performs some processing, and exposes **cooked data**. This processing happens once during the lifetime of the Data Cooker and the cooked data does not change.

There are two types of data cookers:

1) Source Data Cookers
2) Composite Data Cookers

#### Source Data Cookers

These Data Cookers consume data from a single Custom Data Source. They take part in parsing and processing of a data source.

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

An Extensible Data Source fits into a framework for processing a data source and exposing the data through a set of Data Cookers and Tables. To achieve this the developer must create a Source Parser.

### Source Parser

A Source Parser is responsible for parsing a data source into individual records or events and submitting these events for further processing by Source Data Cookers.

The Source Parser is responsible for packaging each parsed event and any additional context about the event into a prescribed format for consumption by Source Data Cookers. It also how an event may be identified as useful to a Source Data Cooker.

To create a Source Parser, define a public class that inherits from SourceParserBase<T, TContext, TKey>.

Here's a brief description of the generic parameters:

- T: The type used to package event data for this Extensible Data Source. Note that this type must implement IKeyedData<TKey>.
- TContext: The type used to package contextual data for an event for this Extensible Data Source.
- TKey: The type used to identify events for distribution to Source Data Cookers.

IKeyedDataType<TKey> inherits from IComparable<TKey> and has one additional method: TKey GetKey(). This is used to determine if an event should be distributed to a Source Data Cooker.

To parse a data source

To create a data source with extensibility support, perform the following:

1) Create a public class that implements abstract class CustomDataSourceBase.
2) Create a public class that implements abstract class 