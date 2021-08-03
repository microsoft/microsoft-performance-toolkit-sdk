# Components

This file outlines and provides a brief description of the various components that make up the SDK. 
Any types defined in the SDK that you can use to develop an SDK plugin will be highlighted in `code formatting`. 
Any references to other high-level components (but not neccessarily types) will be highlighted in **bold**.

It is recommended to read the (Architecture)[./Architecture] documentation before reading this file to 
understand how different components work together at a higher level.

----

## Processing Sources
Processing sources are the entry points the SDK runtime uses to interact with your plugin when a user wants to 
open a file/data source for analysis. They provide the SDK with
1) Information on what types of data sources your plugin supports
2) A way to obtain a **Custom Data Processor** that implements logic for processing the data sources your plugin supports

<details>

<summary>Click to Expand</summary>

Under Construction
 
</details>

----

## Custom Data Processors

<details>

<summary>Click to Expand</summary>

Under Construction
 
</details>



----

## Tables

<details>

<summary>Click to Expand</summary>

Under Construction
 
</details>

----

## Source Parsers

<details>

<summary>Click to Expand</summary>

Under Construction
 
</details>

----

## Data Cookers

<details>

<summary>Click to Expand</summary>

Under Construction
 
</details>

----

## Data Outputs

<details>

<summary>Click to Expand</summary>

Under Construction
 
</details>

----

## Data Cooker Paths

<details>

<summary>Click to Expand</summary>

Under Construction
 
</details>

----

## Extended Tables

Extended tables are similar to regular **Table**s in function, but differ in the way they get data. Regular tables
are "hooked up" to processors by the SDK and are expected to be built by the **Custom Data Processor**'s `BuildTableCore` method 
when their `TableDescriptor`s are passed in as a parameter.

Extended tables make use of the SDK's *Data Processing Pipeline* to programmatically gain access to data exposed by 
**Data Cooker**s. This means
1) **Extended Table**s do not get passed into any **Custom Data Processor**'s `BuildTableCore` method
2) **Extended Table**s rely on the SDK to know when its required data is available from all of its required **Data Cooker**s
3) **Extended Table**s are responsible for "building itself" (populating an `ITableBuilder`)

To accomplish this third task, **Extended Table**s must implement a method with the following signature:

```C#
public static void BuildTable(ITableBuilder tableBuilder, IDataExtensionRetrieval tableData)
```

The SDK will invoke this method when all of the **Extended Table**'s required data is available from all of its required **Data Cooker**s. Data 
can be gathered from required **Data Cooker**s' **Data Output**s through the `IDataExtensionRetrieval` passed in to this method.

To designate a table as an **Extended Table**, pass in the `requiredDataCookers` parameter to the table's `TableDescriptor` static property's constructor. 
This parameter must enumerate all of the `DataCookerPath`s for the **Data Cooker**s the table depends upon.

Because **Data Cookers** allow programmatic access to its **Data Output**s across plugin boundaries, an **Extended Table** can require data from 
a data cooker from another plugin - even one which it does not have the source code for. If a developer knows the `string`s which make up a 
**Data Cooker**'s path, it can create a new `DataCookerPath` that instructs the SDK to pipe its data to tables defined in his/her plugin.