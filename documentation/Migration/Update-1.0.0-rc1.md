# Abstract

This document outlines major code changes that might be needed when updating from 
Preview Version 0.109.\* to 1.0.0 Relase Candidate 1\*.
This document is divided
into two sections: [Breaking Changes](#breaking-changes) and 
[Suggested Changes](#suggested-changes).

# Breaking Changes

There are a number of breaking changes in this version; please see the release notes for a list of these changes.

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

## Obsolete Members

All code decorated with the 'Obsolete' attribute has been removed.

### DataCookerPath

The following members have been removed from `DataCookerPath`:

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

### DataOutputPath

The following members have been removed from `DataOutputPath`:

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

### CustomDataProcessor

The following members have been removed from `CustomDataProcessor`:

- method `CustomDataProcessor.ProcessAsync(ILogger, IProgress<int>, CancellationToken)`

### RequiresCookerAttribute

This class has been removed. Please use `RequiresSourceCookerAttribute` or
`RequiresCompositCookerAttribute`.

### TableConfiguration

The following members have been removed from `TableConfiguration`:

- property `TableConfiguration.Layout`

### CustomDataSourceBase

This class has been removed. Please use `ProcessingSource`.

### CustomDataSourceAttribute

This class has been removed. Please use `ProcessingSourceAttribute`.

### CustomDataSourceInfo

This class has been removed. Please use `ProcessingSourceInfo`.

## SDK

The following are required if you are using the `ProcessingSource` base class and
are using the cosntructors that take `additionalTablesProvider` and/or the
`tableAssemblyProvider` parameters.

These parameters are being removed and replaced by the `ITableProvider` interface.
If you have custom logic for determining the tables exposed by a `ProcessingSource`,
you must implement the new interface.

The default behavior of using all tables found in the assembly has been preserved.
This change only effects those `ProcessingSource`s that use custom table providers.

Two helper methods have been added for use:
`TableDiscovery.CreateForAssembly` and `TableDiscovery.CreateForNamespace.` Users
may also provide their own implementations.

## Data Processors (NOT CustomDataProcessors)

`DataProcessor`s have been removed. Note that these are not the same as
`CustomDataProcessor`s; `CustomDataProcessor`s are still present as they were.

## Inheritance

The following classes have been sealed:

- `ProcessingSourceInfo`
- `ProcessingSourceAttribute`

## Virtuality

The following methods are no longer `virtual` and are now `abstract`:

- `CustomDataProcessor.ProcessAsyncCore(IProgress<int>, CancellationToken)`

## IApplicationEnvironment

The following properties have been renamed to better indicate their purpose:
- `GraphicalUserEnvironment` -> `IsInteractive`

## Engine

The following are required if you are using the `Engine`:

### Creation

Plugins and DataSources are now loaded independently of the `Engine`. These changes were motivated by a desire to make feedback to the user about issues clearer, reduce coupling, and enable reuse. This makes the `Engine` much more powerful and flexible.

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

### Engine Execution Results

A new parameter has been added to the constructor.

## DataCookerPath

The following have been removed:
- `DataCookerPath.Format`
- `DataCookerPath.EmptySourceParserId`
- `DataCookerPath.CookerPath`
- `DataCookerPath.Parse`
- `DataCookerPath.GetSourceParserId`
- `DataCookerPath.GetDataCookerId`
- `DataCookerPath.IsWellFormed`

To prevent build breaks, remove references to these methods and properties.

Both the constructors for `DataCookerPath` that accept string arguments and `DataCookerPath.Create` 
have been removed. 
To prevent build breaks, replace calls to this class' constructor with either
- `DataCookerPath.ForComposite`
- `DataCookerPath.ForSource`

depending on the type of data cooker the path is for.

## DataOutputPath

The following have been removed:
- `DataOutputPath.Format`
- `DataOutputPath.Path`
- `DataOutputPath.Combine`
- `DataOutputPath.GetSourceId`
- `DataOutputPath.GetDataCookerId`
- `DataOutputPath.GetDataCookerPath`
- `DataOutputPath.TryGetConstituents`
- `DataOutputPath.IsWellFormed`

To prevent build breaks, remove references to these methods and properties.

Both the constructors for `DataOutputPath` that accept string arguments and `DataOutputPath.Create` 
have been removed.
To prevent build breaks, replace calls to this class' constructor with either
- `DataOutputPath.ForComposite`
- `DataOutputPath.ForSource`

depending on the type of data cooker the data output path is for.

## RequiresCookerAttribute

This class has been made abstract. Please replace it with either
- `RequiresSourceCookerAttribute`
- `RequiresCompositeCookerAttribute`

depending on the type of required data cooker.

## Obsolete Code Removed

Code that was previously attributed as obsolete is now removed. In addition to some of the elements 
already called out in this document, this includes 

- References to CustomDataSource, previously renamed to ProcessingSource
- A constructor for `DataSourceInfo` that does not supply a wall clock value.

## CustomDataProcessorBase

This class has been renamed to `CustomDataProcessor`.

Obsolete virtual method `ProcessAsync` has been removed and method `ProcessAsyncCore` has been made abstract.
Please move any code from`ProcessAsync` to `ProcessAsyncCore`.

## IViewportSensitiveProjection

In addition to this interface being renamed to `IVisibleDomainSensitiveProjection`, the following properties and methods are renamed:
- `DependsOnViewport` -> `DependsOnVisibleDomain`
- `NotifyViewportChanged` -> `NotifyVisibleDomainChanged`

## ViewportSensitiveProjection
In addition to this class being renamed to `VisibleDomainSensitiveProjection`, the following properties and methods are renamed:
- `DependsOnViewport` -> `DependsOnVisibleDomain`
- `NotifyViewportChanged` -> `NotifyVisibleDomainChanged`
- `CloneIfViewportSensitive` -> `CloneIfVisibleDomainSensitive`

## IVisibleTableRegion

In addition to this class being renamed to `IVisibleDomainRegion`, the following properties and method are renamed:
- `Viewport` -> `Domain`
- `AggregateRowsInViewport` -> `AggregateVisibleRows`

Additionally, the properties `TableRowStart ` and `TableRowCount` are removed.

## Plugin Configurations
All references to "plug-in" and "PlugIn" have been changed to lowercase "plugin." In addition to the name
changes listed above, the follow references have been changed:

- The `Microsoft.Performance.SDK.PlugInConfiguration` namespace has changed to `Microsoft.Performance.SDK.PluginConfiguration`.
- `PlugInConfiguration.PlugInName` has beeen changed to `PluginConfiguration.PluginName`

# Suggested Changes

None at this time.

