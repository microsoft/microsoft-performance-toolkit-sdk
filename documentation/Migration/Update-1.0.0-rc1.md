# Abstract and Table of Contents

This document outlines major code changes that might be needed when updating from 
Preview Version 0.109.\* to 1.0.0 Relase Candidate 1.

Table of Contents
- [Abstract and Table of Contents](#abstract-and-table-of-contents)
- [Conceptual Changes](#conceptual-changes)
  - [Table Discovery](#table-discovery)
  - [Internal Tables](#internal-tables)
  - [Metadata Tables](#metadata-tables)
- [Breaking Changes](#breaking-changes)
  - [Renamed Classes, Interfaces and Static Methods](#renamed-classes-interfaces-and-static-methods)
  - [Obsolete Members](#obsolete-members)
    - [DataSourceInfo](#datasourceinfo)
    - [DataCookerPath](#datacookerpath)
    - [DataOutputPath](#dataoutputpath)
    - [CustomDataProcessor](#customdataprocessor)
    - [RequiresCookerAttribute](#requirescookerattribute)
    - [TableConfiguration](#tableconfiguration)
    - [CustomDataSourceBase](#customdatasourcebase)
    - [CustomDataSourceAttribute](#customdatasourceattribute)
    - [CustomDataSourceInfo](#customdatasourceinfo)
  - [ProcessingSource](#processingsource)
  - [ColumnRole](#columnrole)
  - [TableConfiguration](#tableconfiguration-1)
  - [Data Processors (NOT CustomDataProcessors)](#data-processors-not-customdataprocessors)
  - [ProcessingSourceInfo](#processingsourceinfo)
  - [ProcessingSourceAttribute](#processingsourceattribute)
  - [CustomDataProcessor](#customdataprocessor-1)
  - [CustomDataProcessorWithSourceParser](#customdataprocessorwithsourceparser)
  - [TableDescriptor](#tabledescriptor)
  - [Engine](#engine)
    - [Creation](#creation)
  - [Interfaces implemented in the SDK or SDK.Runtime](#interfaces-implemented-in-the-sdk-or-sdkruntime)
    - [ICustomDataProcessor](#icustomdataprocessor)
    - [IDataProcessorExtensibilitySupport](#idataprocessorextensibilitysupport)
    - [IDataExtensionRetrievalFactory](#idataextensionretrievalfactory)
    - [IMetadataTableBuilderFactory](#imetadatatablebuilderfactory)
    - [IProcessorEnvironment](#iprocessorenvironment)
    - [IApplicationEnvironment](#iapplicationenvironment)
  - [TableAttribute and InternalTableAttribute](#tableattribute-and-internaltableattribute)
    - [Engine Execution Results](#engine-execution-results)
  - [IViewportSensitiveProjection](#iviewportsensitiveprojection)
  - [ViewportSensitiveProjection](#viewportsensitiveprojection)
  - [IVisibleTableRegion](#ivisibletableregion)
  - [Plugin Configurations](#plugin-configurations)
- [Suggested Changes](#suggested-changes)
  - [Metadata Tables](#metadata-tables-1)

# Conceptual Changes

## Table Discovery

Prior to this release candidate, any tables defined in a processing source's assembly were associated with
that processing source. Now, a `ProcessingSource` provides an instance of an `IProcessingSourceTableProvider`. All tables returned by
this instance's `Discover` method are associated with that `ProcessingSource`.

Note that "cooker tables" (tables which depend on data cookers and provide their own static build methods) that
are discovered by a `IProcessingSourceTableProvider` are also associated with the provider's `ProcessingSource`.
This may result in tables which "build themselves" through the extensions runtime being passed into a `CustomDataProcessor.BuildTable` method. 
**The included implementations of `IProcessingSourceTableProvider` will never "discover" a cooker table.** This ensures tables
that depend on data cookers are only requested to be built once data is available.

Custom implementations of `IProcessingSourceTableProvider` should aim to preserve this behavior. For most plugins,
the included implementations should suffice. This release candidate introduces the static methods `TableDiscovery.CreateForAssembly` and `TableDiscovery.CreateForNamespace` to create included implementations of `IProcessingSourceTableProvider`.

## Internal Tables

The concept of "internal tables" has been removed. Internal tables were initially used to differentiate tables that must be
built by a `CustomDataProcessor`. Instead, tables that must be built by a `CustomDataProcessor` are now discovered by a
`IProcessingSourceTableProvider` (see above).

## Metadata Tables

Prior to this release candidate, metadata tables were conceptually separate from "normal" tables. Starting with this release
candidate, metadata tables are treated exactly the same as all other tables.

# Breaking Changes

## Renamed Classes, Interfaces and Static Methods

The following references must be changed:
- `BaseSourceDataCooker` -> `SourceDataCooker`
- `SourceParserBase` -> `SourceParser`
- `BaseDataColumn` -> `DataColumn`
- `CustomDataProcessorBase` -> `CustomDataProcessor`
- `CustomDataProcessorBaseWithSourceParser` -> `CustomDataProcessorWithSourceParser`
- `CustomDataSourceAttribute` -> `ProcessingSourceAttribute`
- `CustomDataSourceBase` -> `ProcessingSource`
- `CustomDataSourceInfo` -> `ProcessingSourceInfo`
- `PlugInConfiguration` -> `PluginConfiguration`
- `PlugInConfigurationExtensions` -> `PluginConfigurationExtensions`
- `PlugInConfigurationSerializer` -> `PluginConfigurationSerializer`
- `PlugInConfigurationValidation` -> `PluginConfigurationValidation`
- `IViewportSensitiveProjection` -> `IVisibleDomainSensitiveProjection`
- `ViewportSensitiveProjection` -> `VisibleDomainSensitiveProjection`
- `IVisibleTableRegion` -> `IVisibleDomainRegion`
- `ViewportRelativePercent` -> `VisibleDomainRelativePercent`
- `Projection.ClipTimeToViewport` -> `Projection.ClipTimeToVisibleDomain`
- `Projection.AggregateInViewport` -> `Projection.AggregateInVisibleDomain`
- `ISerializer` -> `ITableConfigurationsSerializer`

## Obsolete Members

All code decorated with the 'Obsolete' attribute has been removed.

### DataSourceInfo

- The following obsolete constructors have been removed:

    - constructor that does not supply a wall clock value.

### DataCookerPath

- The following obsolete members have been removed:
    - field `DataCookerPath.Format`
    - constant `DataCookerPath.EmptySourceParserId`
    - constructor `public DataCookerPath(string)`
    - constructor `public DataCookerPath(string, string)`
    - property `DataCookerPath.CookerPath`
    - method `DataCookerPath.Parse(string)`
    - method `DataCookerPath.Create(string, string)`
    - method `DataCookerPath.GetSourceParserId(string)`
    - method `DataCookerPath.GetdataCookerId(string)`
    - method `DataCookerPath.IsWellFormed(string)`
    - method `DataCookerPath.SplitPath(string)`

    To prevent build breaks, remove references to these methods and properties.

- Both the obsolete constructors for `DataCookerPath` that accept string arguments and the obsolete `DataCookerPath.Create` 
  method have been removed. 
  To prevent build breaks, replace calls to this class' constructor with either
    - `DataCookerPath.ForComposite`
    - `DataCookerPath.ForSource`

    depending on the type of data cooker the path is for.

### DataOutputPath

- The following obsolete members have been removed from `DataOutputPath`:

    - field `DataOutputPath.Format`
    - method `DataOutputPath.Create(string)`
    - method `DataOutputPath.Create(string, string, string)`
    - property `DataOutputPath.Path`
    - method `DataOutputPath.Combine(string, string, string)`
    - method `DataOutputPath.GetSourceId(string)`
    - method `DataOutputPath.GetDataCookerId(string)`
    - method `DataOutputPath.GetDataOutputId(string)`
    - method `DataOutputPath.GetDataCookerPath(string)`
    - method `DataOutputPath.TryGetConstituents(string, out string, out string, out string)`
    - method `DataOutputPath.IsWellFormed(string)`
    - method `DataOutputPath.SplitPath(string)`

    To prevent build breaks, remove references to these methods and properties.

- Both the obsolete constructors for `DataOutputPath` that accept string arguments and the obsolete `DataOutputPath.Create` 
  methods have been removed.
    To prevent build breaks, replace calls to this class' constructor with either
    - `DataOutputPath.ForComposite`
    - `DataOutputPath.ForSource`

    depending on the type of data cooker the data output path is for.

### CustomDataProcessor

The following obsolete members have been removed:

- method `CustomDataProcessor.ProcessAsync(ILogger, IProgress<int>, CancellationToken)`

### RequiresCookerAttribute

This obsolete class has been removed. Please use `RequiresSourceCookerAttribute` or
`RequiresCompositCookerAttribute`.

### TableConfiguration

- The following obsolete members have been removed from `TableConfiguration`:

    - property `TableConfiguration.Layout`

### CustomDataSourceBase

This obsolete class has been removed. Please use `ProcessingSource`.

### CustomDataSourceAttribute

This obsolete class has been removed. Please use `ProcessingSourceAttribute`.

### CustomDataSourceInfo

This obsolete class has been removed. Please use `ProcessingSourceInfo`.

## ProcessingSource

- `ProcessingSource.AllTables` has changed from being an `IReadOnlyDictionary<TableDescriptor, Action<ITableBuilder, IDataExtensionRetrieval>>` to an `IEnumerable<TableDescriptor>`

- The following members have been removed:
    - method `ProcessingSource.GetTableBuildAction`

- The constructors that take an `additionalTablesProvider` and/or a `tableAssemblyProvider` have been removed. Please see the 
  Table Discovery section for more details.

## ColumnRole

This enum has been removed. Please use the new `ColumnRole` static class for defined column roles.

## TableConfiguration

- The `ColumnRoles` property now is an `IDictionary<string, Guid>` instead of an `IDictionary<ColumnRole, Guid>`.

- The `AddColumnRole` and `RemoveColumnRole` methods now take in case sensitive strings. Plese see `ColumnRoles` static class for string options.

## Data Processors (NOT CustomDataProcessors)

`DataProcessor`s have been removed. Note that these are not the same as
`CustomDataProcessor`s; `CustomDataProcessor`s are still present as they were.

## ProcessingSourceInfo

This class has been sealed.

## ProcessingSourceAttribute

This class has been sealed.

## CustomDataProcessor

- All changes to `ICustomDataProcessor` apply to this abstract class as well.

- The following methods are no longer `virtual` and are now `abstract`:

    - `CustomDataProcessor.ProcessAsyncCore(IProgress<int>, CancellationToken)`

- The following members have been removed:
    - method `CustomDataProcessor.EnableMetadataTables`
    - method `CustomDataProcessor.BuildMetadataTables`
    - property `CustomDataProcessor.TableDescriptorToBuildAction`

- `CustomDataProcessor.BuildTableCore` no longer receives an `Action<ITableBuilder, IDataExtensionRetrieval>` parameter.  

- The constructor for `CustomDataProcessor` has the following parameters removed:

    - `IReadOnlyDictionary<TableDescriptor, Action<ITableBuilder, IDataExtensionRetrieval>> allTablesMapping`
    - `IEnumerable<TableDescriptor> metadataTables`

    Any classes deriving `CustomDataProcessor` must remove these parameters from their base constructor calls.

- Data-Derived tables may no longer supply a build action that receives a `IDataExtensionRetrieval` instance. The `CustomDataProcessor.AddTableGeneratedFromDataProcessing` method has been updated to take a `Action<ITableBuilder>` instead of a `Action<ITableBuilder, IDataExtensionRetrieval>` for its `buildAction` parameter. Additionally, this method may throw an `InvalidOperationException` if `ProcessAsync` has already been called, or an `ArgumentException` if a table with the supplied `ArgumentException` has already been enabled or added as a data-derived table.

## CustomDataProcessorWithSourceParser

- All changes to `CustomDataProcessor` and `ICustomDataProcessor` apply to this abstract class as well.

- The following members have been removed:
    - method `CustomDataProcessorWithSourceParser.GetDataExtensionRetrieval`

- The constructor for `CustomDataProcessorWithSourceParser` has the following parameters removed:

    - `IReadOnlyDictionary<TableDescriptor, Action<ITableBuilder, IDataExtensionRetrieval>> allTablesMapping`
    - `IEnumerable<TableDescriptor> metadataTables`

    Any classes deriving `CustomDataProcessorWithSourceParser` must remove these parameters from their base constructor calls.

- The default implementation of `CustomDataProcessorWithSourceParser.BuildTableCore` now does nothing. If a deriving classes expects calls to this method (i.e. the `ProcessingSource` that creates the processor discovers tables that do not have their own static build methods), it must override this method and implement logic to build the requested table. The required logic is similar to that which would be implemented in a custom implemention of `CustomDataProcessor.BuildTableCore`.

## TableDescriptor

- This class' constructor no longer receives an `IEnumerable<DataProcessorId>` of required data processors.

- The following members have been removed:
    - property `TableDescriptor.IsInternalTable`
    - property `TableDescriptor.RequiredDataProcessors`

## Engine

The following are required if you are using the `Engine`:

### Creation

Plugins and `DataSource`s are now loaded independently of the `Engine`. These changes were motivated by a desire to make feedback to the user about issues clearer, reduce coupling, and enable reuse. This makes the `Engine` much more powerful and flexible.

- Users will now need to create a `DataSourceSet` to pass to the `Engine` via the `EngineCreateInfo` class rather than calling the `Engine.Add*` methods.
    - See the [`Engine`](Using-the-SDK/Using-the-engine.md) documentation for more details on using `DataSourceSet`, `PluginSet`, and the `Engine` together.

As a simple example:
````cs
using (var engine = Engine.Create())
{
    engine.AddDataSource(new FileDataSource("test.txt"));
    engine.EnableCooker(cookerPath);

    // ... use the engine as desired.
}

````
becomes
````cs

using (var dataSources = DataSourceSet.Create())
{
    dataSources.AddDataSource(new FileDataSource("test.txt"));

    var info = new EngineCreateInfo(dataSources);
    using (var engine = Engine.Create(info))
    {
        engine.EnableCooker(cookerPath);

        // ... use the engine as desired.
    }
}

````

- Try-catch blocks that were expecting `UnsupportedDataSourceException`s to signal an invalid `CustomDataSource`, (now named `ProcessingSource` - see below) in any of the `Add*` methods should be updated to catch `UnsupportedProcessingSourceException`:
````cs
try
{
    dataSources.AddFile("test", typeof(BadProcessingSource))
}
catch (UnsupportedDataSourceException)
{
    // ...
}
````
becomes
````cs
try
{
    dataSources.AddFile("test", typeof(BadProcessingSource))
}
catch (UnsupportedProcessingSourceException)
{
    // ...
}
````

Note that `UnsupportedProcessingSourceException` was named `UnsupportedCustomDataSourceException` in a previous preview of v0.109.0.

- Try-catch blocks that were expecting `UnsupportedFileException`s to signal an invalid file in any of the `Add*` methods should be updated to catch `UnsupportedDataSourceExcepton`:
````cs
try
{
    dataSources.AddDataSource(new FileDataSource("test.nope"));
}
catch (UnsupportedFileException)
{
    // ...
}
````
becomes
````cs
try
{
    dataSources.AddDataSource(new FileDataSource("test.nope"));
}
catch (UnsupportedDataSourceException)
{
    // ...
}
````

## Interfaces implemented in the SDK or SDK.Runtime

There are some changes made to interfaces that generally don't need to be implemented by plugins. Those changes are documented in this section.

### ICustomDataProcessor

- The following members have been removed:

    * method `ICustomDataProcessor.BuildMetadataTables`

- The following members have been added:
    - method `TryEnableTable`. This is similar to `EnableTable`, but catches most exceptions and returns a `bool`.

- `EnableTable` and `ProcessAsync` may now throw an `InvalidOperationException` if `ProcessAsync` has already been called.

### IDataProcessorExtensibilitySupport

This interface has been removed.

### IDataExtensionRetrievalFactory

This was moved from the `SDK` namespace to `SDK.Runtime` namespace.

### IMetadataTableBuilderFactory

This interface has been removed.

### IProcessorEnvironment

- The following members have been removed:
    - method `IProcessorEnvironment.CreateDataProcessorExtensibilitySupport`

### IApplicationEnvironment

- The following properties have been renamed to better indicate their purpose:
    - `GraphicalUserEnvironment` -> `IsInteractive`

## TableAttribute and InternalTableAttribute

- The `InternalTableAttribute` was removed.

- Internal table constructor arguments and properties have been removed from `TableAttribute`.

- The runtime will now determine if a table needs to be marked as internal.

- Cooker tables - i.e. tables that require data cookers - must provide a build table method.

### Engine Execution Results

A new parameter has been added to the constructor.

## IViewportSensitiveProjection

- This interface has been renamed to `IVisibleDomainSensitiveProjection`

- The following members have been renamed:
    - property `DependsOnViewport` -> `DependsOnVisibleDomain`
    - method `NotifyViewportChanged` -> `NotifyVisibleDomainChanged`

## ViewportSensitiveProjection
- This class has been renamed to `VisibleDomainSensitiveProjection`

- The following members have been renamed:
    - property `DependsOnViewport` -> `DependsOnVisibleDomain`
    - method `NotifyViewportChanged` -> `NotifyVisibleDomainChanged`
    - method `CloneIfViewportSensitive` -> `CloneIfVisibleDomainSensitive`

## IVisibleTableRegion

- This interface has been renamed to `IVisibleDomainRegion`

- The following members have been renamed:
    - property `Viewport` -> `Domain`
    - method `AggregateRowsInViewport` -> `AggregateVisibleRows`

- The following members have been removed:
    - property `TableRowStart`
    - property `TableRowCount`

## Plugin Configurations

- All references to "plug-in" and "PlugIn" have been changed to lowercase "plugin." 

- The `Microsoft.Performance.SDK.PlugInConfiguration` namespace has changed to `Microsoft.Performance.SDK.PluginConfiguration`.

- `PlugInConfiguration.PlugInName` has beeen changed to `PluginConfiguration.PluginName`.

# Suggested Changes

## Metadata Tables
When using the Engine, metadata tables are no longer enabled by default. If you wish to build
metadata tables, you must update your Engine interaction to manually enable them.
