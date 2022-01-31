# Glossary

* [AboutInfo](#aboutinfo)
* [Column](#column)
* [ColumnConfiguration](#columnconfiguration)
* [ColumnRole](#columnrole)
* [CompositeDataCooker](#compositedatacooker)
* [CustomDataProcessor](#customdataprocessor)
* [DataCooker](#datacooker)
* [DataSource](#datasource)
* [DynamicTable](#dynamictable)
* [Plugin](#plugin)
* [Processing Pipeline](#processing-pipeline)
* [ProcessingSource](#processingsource)
* [Projection](#projection)
* [Simple Table](#simple-table)
* [SourceDataCooker](#sourcedatacooker)
* [SourceParser](#sourceparser)
* [Special Columns](#special-columns)
* [Table](#table)
* [TableBuilder](#tablebuilder)
* [Table Building Cycle](#table-building-cycle)
* [TableCommand](#tablecommand)
* [TableConfiguration](#tableconfiguration)
* [VisibleDomainSensitiveProjection](#visibledomainsensitiveprojection)

---

## AboutInfo

Exposes information about a plugin, such as its authors, copyright, and license. See [Adding About Information](./Using-the-SDK/Advanced/Adding-About-Information.md).

## Column

A ([ColumnConfiguration](#columnconfiguration), [Projection](#projection)) pair that defines data inside a [Table](#table).

See also: [Special Columns](#special-columns)

## ColumnConfiguration

Information about a column of a [Table](#table). Contains the column's name, metadata, and [UIHints](#uihints).

See also: [Projection](#projection).

## ColumnRole

Meta-information about a [Column](#column) that defines the Column's role in data presentation. For example, a Column that contains a `Timestamp` whose value indicates when an event occured relative to the start of a [DataSource](#datasource) may be marked as a "start time" Column.

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

## Simple Table

A [Table](#table) variant that cannot participate in a [Processing Pipeline](#processing-pipeline) and must be built by a [CustomDataProcessor](#customdataprocessor). Simple Tables should only be used if your plugin does not use [DataCookers](#datacooker). However, since in most data-processing situations it is recommended to use DataCookers to architect your data-processing, it is recommended to use only standard [Tables](#table).

## SourceDataCooker

A [DataCooker](#datacooker) that receives input from a specific [SourceParser](#sourceparser). A SourceDataCooker may also receive input from other DataCookers of the same SourceParser.

## SourceParser

An object that a [CustomDataProcessor](#customdataprocessor) typically tasks with parsing a [DataSource](#datasource) into individual records. The records that a SourceParser emits typically begin a [Processing Pipeline](#processing-pipeline), wherein [DataCookers](#datacooker) further process data to be consumed by [Tables](#table).  

## Special Columns

Columns that all [Tables](#table) may use in their [TableConfiguration](#tableconfiguration) and denote special behavior. These columns are
* Pivot Column: a column that marks the end of columns which should be pivoted about
* Graph Column: a column that marks the start of columns that can/should be graphed
* Left Freeze Column: a column that marks the end of columns that should be frozen in GUIs
* Right Freeze Column: a column that marks the start of columns that should be frozen in GUIs

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