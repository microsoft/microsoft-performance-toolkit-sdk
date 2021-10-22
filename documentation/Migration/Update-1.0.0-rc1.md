# Abstract

This document outlines major code changes that might be needed when updating from 
Preview Version 0.109.\* to 1.0.0 Relase Candidate 1\*.
This document is divided
into two sections: [Breaking Changes](#breaking-changes) and 
[Suggested Changes](#suggested-changes).

# Breaking Changes

There are a number of breaking changes in this version; please see the release notes for a list of these changes.

## Renamed Classes
The following references must be changed:
- `BaseSourceDataCooker` -> `SourceDataCooker`
- `SourceParserBase` -> `SourceParser`
- `BaseDataColumn` -> `DataColumn`
- `CustomDataProcessorBase` -> `CustomDataProcessor`
- `CustomDataProcessorBaseWithSourceParser` -> `CustomDataProcessorWithSourceParser`

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

# Suggested Changes

None at this time.