# Abstract
This document outlines how to use the `IDataSourceGrouper` interface to specify which combinations of `IDataSource` instances are capable
of being processed together as a single unit.

> :info: This feature is currently optional, but will be required starting in SDK version `2.0.*`.
> If you do not implement this feature in earlier versions, you may see warning messages logged
> as "<processing_source_name> does not support processing user-specified processing groups - falling back to default processing."
> This is expected behavior.

# Motivation
In versions `1.*.*` of the SDK, `ProcessingSource` instances received a collection of `IDataSource` instances that should be processed.
Every `ProcessingSource` implemented this method:

```cs
protected abstract ICustomDataProcessor CreateProcessorCore(
    IEnumerable<IDataSource> dataSources,
    IProcessorEnvironment processorEnvironment,
    ProcessorOptions options);
```

This interface presents a problem: what if the specified data sources cannot be processed together by a single
instance of the `ICustomDataProcessor`? For example, if a plugin's `ICustomDataProcessor` can only process
single files at a time, what should happen if more than one `IDataSource` is provided to this method?

The root issue is `ProcessingSource` instances are unable to advertise which specific combination of `IDataSource` instances
can be processed together. As such, SDK drivers (such as Windows Performance Analyzer) may assume *any* combination
of to-be-processed data sources can be processed together and the user is free to decide which combination(s) to use.

To address this, SDK version `1.1.13` introduced the concept of "data source grouping" and added APIs that allow
`ProcessingSource` instances to
1. Decide which combinations of `IDataSource` instances are compatible with each other
2. Decide which *processing mode* a specific combination of `IDataSource` groups can be processed in
3. Receive these advertised combinations during processing in place of the legacy `IEnumerable<IDataSource>` parameter

Beginning in SDK version `2.0.*`, the legacy `IEnumerable<IDataSource>` processing path will be removed, and all `ProcessingSource` instances must implement the interface described by this document.

# Usage
First, ensure you are targeting SDK version `1.1.13` or later. Then, there are two code changes that must be performed:
1. Implement an API that constructs `IDataSourceGroup` instances that can be processed
2. Implement a new method to process a `IDataSourceGroup`

## Specifying supported `IDataSourceGroup` instances
On the `ProcessingSource` implementation you wish to adopt this feature on, implement the new [IDataSourceGrouper interface](https://github.com/microsoft/microsoft-performance-toolkit-sdk/blob/main/src/Microsoft.Performance.SDK/Processing/DataSourceGrouping/IDataSourceGrouper.cs).

The method to implement is

```cs
IReadOnlyCollection<IDataSourceGroup> CreateValidGroups(
    IEnumerable<IDataSource> dataSources,
    ProcessorOptions options);
```

I.e., your processing source must map the `IDataSource` instances it advertised support for to `IDataSourceGroup` instances, optionally
using the specific `ProcessorOptions` to help determine which groups to return. 

An `IDataSourceGroup` specifies two properties:

```cs
public interface IDataSourceGroup
{
    /// <summary>
    ///     Gets the <see cref="IDataSource"/>s in this group.
    /// </summary>
    IReadOnlyCollection<IDataSource> DataSources { get; }
    
    /// <summary>
    ///     Gets the <see cref="IProcessingMode"/> for this group.
    /// </summary>
    IProcessingMode ProcessingMode { get; }
}
```

The `DataSources` property is the collection of `IDataSource` instances that can be processed together by a single call. Each item in this collection
**must** be an `IDataSource` instance passed to `CreateValidGroups`.

The `ProcessingMode` property can be used by your plugin to signal to your `ProcessingSource` and/or `ICustomDataProcessor` how to process the data sources in the group.
For your conveinence, there is a [DefaultProcessingMode class](https://github.com/microsoft/microsoft-performance-toolkit-sdk/blob/main/src/Microsoft.Performance.SDK/Processing/DefaultProcessingMode.cs)
defined that you may use. However, you may create custom `IProcessingMode` implementations defined by your plugin for this property.

To create `IDataSourceGroup` instances during `CreateValidGroups`, you may use the [DataSourceGroup class](https://github.com/microsoft/microsoft-performance-toolkit-sdk/blob/main/src/Microsoft.Performance.SDK/Processing/DataSourceGrouping/DataSourceGroup.cs) defined in the SDK.

## Processing specified `IDataSourceGroup` instances
SDK version `1.1.13` introduced a new `ProcessingSource.CreateProcessorCore` method that takes in an `IDataSourceGroup`:

```cs
protected virtual ICustomDataProcessor CreateProcessorCore(
    IDataSourceGroup dataSourceGroup,
    IProcessorEnvironment processorEnvironment,
    ProcessorOptions options)
```

For SDK version `1.*.*`, this method is optional and marked as virtual. Starting in SDK versions `2.0.*`, this method will be required to implement.

To consume your advertised `IDataSourceGroup` instances, you must override this method in your processing source.

## Testing

After doing both of these steps, SDK drivers (such as Windows Performance Analyzer) will ensure only `IDataSourceGroup` instances returned by your `ProcessingSource`'s implementation of `CreateValidGroups` are provided to your `ProcessingSource` via `CreateProcessorCore`. You may test your plugin with these changes in WPA version `11.5.29.41056` or newer.