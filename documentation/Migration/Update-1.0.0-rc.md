# Abstract

This document outlines major code changes that might be needed when updating from 
Preview Version 0.109.\* to Release Candidate Version 1.0.0-rc This document is divided
into two sections: [Breaking Changes](#breaking-changes) and 
[Suggested Changes](#suggested-changes).

# Breaking Changes

There are a number of breaking changes in this version; please see the release notes for a list of these changes.

## Data Processors

Data Processors have been removed.

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

# Suggested Changes

None at this time.

