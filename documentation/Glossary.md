# Glossary

- [Glossary](#glossary)
  - [AboutInfo](#aboutinfo)
  - [Column](#column)
  - [ColumnConfiguration](#columnconfiguration)
  - [ColumnRole](#columnrole)
  - [CompositeDataCooker](#compositedatacooker)
  - [CustomDataProcessor](#customdataprocessor)
  - [DataCooker](#datacooker)
  - [DataSource](#datasource)
  - [DynamicTable](#dynamictable)
  - [Pivot Column](#pivot-column)
  - [Pivot Table](#pivot-table)
  - [Plugin](#plugin)
  - [Processing Pipeline](#processing-pipeline)
  - [ProcessingSource](#processingsource)
  - [Projection](#projection)
  - [SDK Driver](#sdk-driver)
  - [Simple Table](#simple-table)
  - [SourceDataCooker](#sourcedatacooker)
  - [SourceParser](#sourceparser)
  - [Special Columns](#special-columns)
  - [Table](#table)
  - [TableBuilder](#tablebuilder)
  - [Table Building Cycle](#table-building-cycle)
  - [TableCommand](#tablecommand)
  - [TableConfiguration](#tableconfiguration)
  - [VisibleDomainSensitiveProjection](#visibledomainsensitiveprojection)
  - [Windows Performance Analyzer](#windows-performance-analyzer)

---

## AboutInfo

Exposes information about a plugin, such as its authors, copyright, and license. See [Adding About Information](./Using-the-SDK/Creating-your-plugin.md#adding-about-information).

## Column

A ([ColumnConfiguration](#columnconfiguration), [Projection](#projection)) pair that defines data inside a [Table](#table).

See also: [Special Columns](#special-columns)

## ColumnConfiguration

Information about a column of a [Table](#table). Contains the column's name, metadata, and [UIHints](#uihints).

See also: [Projection](#projection).

## ColumnRole

Meta-information about a [Column](#column) that defines the Column's role in data presentation. 
For example, a Column of `Timestamp` values relative to the start of a [DataSource](#datasource) may be marked as a "start time" Column.

## CompositeDataCooker

A [DataCooker](#datacooker) that receives input solely from other DataCookers.

## CustomDataProcessor

Component responsible for processing a [DataSource](#datasource). For example, a CustomDataProcessor may be responsible for parsing lines in a `.txt` file into `LineItem` objects that later get consumed by a [DataCooker](#datacooker) or [Table](#table).

In most situations, a CustomDataProcessor will further delegate the responsibility of processing a DataSource to a [SourceParser](#sourceparser).

A CustomDataProcessor is responsible for building any [Simple Tables](#simple-table) discovered by the [ProcessingSource](#processingsource) that created it.

## DataCooker

Component of a [Processing Pipeline](#processing-pipeline) that consumes data from one or more sources and optionally outputs new data for other components to consume. Conceptually, a DataCooker "cooks" data from one type to another.

See also: [SourceDataCooker](#sourcedatacooker) and [CompositeDataCooker](#compositedatacooker).

## DataSource

An input to be processed by a [CustomDataProcessor](#customdataprocessor). In most situations, this is a `FileDataSource` that contains the full path to a file specified by a user. 

## DynamicTable

A [Table](#table) that is built outside of the standard [table building cycle](#table-building-cycle). DynamicTables are commonly built in response to a [TableCommand](#tablecommand) being invoked.

## Pivot Column

NOTE: (Not to be confused with the [Special Column](#special-columns) `PivotColumn`)

A special type of [Column](#column) that should be pivoted about when its [Table](#table) is interpreted as a [Pivot Table](#pivot-table). Any Column that appears in a [TableConfiguration](#tableconfiguration) before the `PivotColumn` is a Pivot Column.

## Pivot Table

A representation of a [Table](#table) where certain [Columns](#column) are pivoted/grouped about. For example, consider this table with 5 rows:

| State        | Zip Code | Population |
|--------------|----------|------------|
| Washington   | 98052    | 65,558     |
| New York     | 10023    | 60,998     |
| Washington   | 98101    | 13,877     |
| Washington   | 98109    | 20,715     |
| New York     | 10025    | 94,600     |

If the "State" column above is a [Pivot Column](#pivot-column), the 5 rows would be grouped into the following Pivot Table:

| State        | Zip Code | Population |
|--------------|----------|------------|
| > Washington |          |            |
|              | 98052    | 65,558     |
|              | 98101    | 13,877     |
|              | 98109    | 20,715     |
| > New York   |          |            |
|              | 10023    | 60,998     |
|              | 10025    | 94,600     |

NOTE: the SDK has no understanding of Pivot Tables. Tables created by a plugin are purely "flat" tables - i.e. tables similar the first one above. It is up to programs like [Windows Performance Analyzer](#windows-performance-analyzer) to use pivot information in a [Table Configuration](#tableconfiguration) to present a plugin's Table as a Pivot Table.

See also: [Wikipedia Pivot Tables](https://en.wikipedia.org/wiki/Pivot_table).

## Plugin

A collection of one-or-more [ProcessingSources](#processingsource) that collectively create zero-or-more [Tables](#table) and consist of zero-or-more [DataCookers](#datacooker).

## Processing Pipeline

A collection of [DataCookers](#datacooker) and [SourceParsers](#sourceparser) that define a structured flow of data.

## ProcessingSource

An entrypoint for a [Plugin](#plugin). A ProcessingSource
1. Declares a human-readable name and description
2. Defines the [DataSource(s)](#datasource) it supports
3. Creates the [CustomDataProcessor](#customdataprocessor) that will process instances of supported DataSources
4. Advertises the [Table(s)](#table) that may be built in response to DataSources being processed

## Projection

A function that maps a row index for a [Table](#table) to a piece of data. Conceptually, a Projection is combined with a [ColumnConfiguration](#columnconfiguration) to complete define a Table's [Column](#column): the ColumnConfiguration defines the name and metadata about a Column, and its associated Projection defines the Column's data.

## SDK Driver

A program that utilizes the SDK and SDK plugins to process [Data Sources](#datasource) and present information to users.

See also: [Windows Performance Analyzer](#windows-performance-analyzer).

## Simple Table

A [Table](#table) variant that cannot participate in a [Processing Pipeline](#processing-pipeline) and must be built by a [CustomDataProcessor](#customdataprocessor). Simple Tables should only be used if your plugin does not use [DataCookers](#datacooker). However, since in most data-processing situations it is recommended to use DataCookers to architect your data-processing, it is recommended to use only standard [Tables](#table).

## SourceDataCooker

A [DataCooker](#datacooker) that receives input from an associated [SourceParser](#sourceparser). A SourceDataCooker may also receive input from other SourceDataCookers enabled on the same SourceParser.

## SourceParser

An object that a [CustomDataProcessor](#customdataprocessor) typically tasks with parsing a [DataSource](#datasource) into individual records. The records that a SourceParser emits typically begin a [Processing Pipeline](#processing-pipeline), wherein [DataCookers](#datacooker) further process data to be consumed by [Tables](#table).  

## Special Columns

Columns that all [Tables](#table) may use in their [TableConfiguration](#tableconfiguration) and denote special behavior. These columns are
* `PivotColumn`: a column that marks the end of columns which should be pivoted about. All columns in a Table that appear before this Special Column are interpreted as [Pivot Columns](#pivot-column)
* `GraphColumn`: a column that marks the start of columns that can/should be graphed
* `LeftFreezeColumn`: a column that marks the end of columns that should be frozen in GUIs
* `RightFreezeColumn`: a column that marks the start of columns that should be frozen in GUIs

## Table

A collection of one-or-more columns that contain data. Each column of a Table must consist of the same number of rows.

A Table may receive data from one-or-more [DataCookers](#datacooker). A Table is also responsible for "building itself" by having a static `Build` method interacts with a [TableBuilder](#tablebuilder).

## TableBuilder

The object used to concretely construct [Tables](#table) and [Simple Tables](#simple-table). When a [CustomDataProcessor](#customdataprocessor) is asked to build a Simple Table, or when a normal Table "builds itself," it will be given an instance of an `ITableBuilder` to add its [Columns](#column) and [TableConfigurations](#tableconfiguration) to.

## Table Building Cycle

The standard process used to construct [Tables](#table). This process involves
1. A user specifying which [DataSource(s)](#datasource) to process
2. [CustomDataProcessor(s)](#customdataprocessor) being given the selected DataSources to process
3. [Simple Tables](#simple-table) being built by these CustomDataProcessor(s)
4. [DataCooker(s)](#datacooker) further processing data produced by any [SourceParsers](#sourceparser) used by the CustomDataProcessor(s) referenced above
5. [Tables](#table) "building themselves" from the data produced by these DataCookers. 

## TableCommand

An action that can be called on a [Table](#table). Concretely, a TableCommand is a `string => Func` key-value pair where the key is the TableCommand's name and the value is a function to be called when the TableCommand is invoked.

A common use-case of TableCommands is creating [DynamicTables](#dynamictable)

## TableConfiguration

A pre-defined collection of properties for a [Table](#table). This includes, but is not limited to,
* The order of [Columns](#column) in the Table
* An initial filter to apply to the Table
* [ColumnRoles](#columnrole) for Columns in the table

## VisibleDomainSensitiveProjection

A [Projection](#projection) that depends upon the visible subrange of values spanned by the Projection's [Table](#table)'s domain. For example, if a Table's domain is time-based (e.g. `Timestamp`-based), a VisibleDomainSensitiveProjection could be a Projection that reports the percent of time spent inside a method call _relative to the timerange currently visible to the user_.

## Windows Performance Analyzer

An [SDK Driver](#sdk-driver) that can display [Tables](#table) plugins create as [Pivot Tables](#pivot-table) and graph data in various formats.