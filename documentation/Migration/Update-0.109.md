# Abstract

This document outlines major code changes that might be needed when updating from 
Preview Version 0.108.\* to Preview Version 0.109.\* This document is divided
into two sections: [Breaking Changes](#breaking-changes) and 
[Suggested Changes](#suggested-changes).

# Breaking Changes

There are a number of breaking changes in this version; please see the release notes for a list of these changes.

## CustomDataSource (now ProcessingSource)

In order to update your `CustomDataSource`, (now named `ProcessingSource` - see below) you will need to
- change your `IsFileSupportedCore(string)` method to `IsDataSourceSupportedCore(IDataSource)`
- Update the logic to act on the data source.
 for example:
 ````cs

protected override bool IsFileSupportedCore(string path)
{
    return Path.GetExtension(path) == ".txt";
}
 ````
 becomes
 ````cs
 protected override bool IsDataSourceSupportedCore(IDataSource dataSource)
 {
     return (dataSource is FileDataSource fds) &&
        (Path.GetExtension(fds.FullPath) == ".txt")
 }
 ````
 If you are implementing `ICustomDataSource` (now named `IProcessingSource` - see below) directly then you will change you `IsFileSupported(string)` method to `IsDataSourceSupported(IDataSource)` and update the logic similar to above.

## IDataSource

`IDataSource.GetUri()` has been changed to a read-only property: `IDataSource.Uri`.

- Anywhere you are calling `IDataSource.GetUri()` can be simply updated to `IDataSource.Uri`

## ErrorInfo

The `InnerError` property has been removed from `ErrorInfo`. The `InnerError`
type has also been removed.

- Any classes inheriting from `InnerError` should now inherit from `ErrorInfo`
directly.

- Any errors that were using `InnerError` may either be replaced with your
sub class, or make use of the `Details` property.

Example:
````cs
public class MyInnerError
    : InnerError
{
    public MyInnerError(int number)
        : base("my_code")
    {
        this.Number = number;
    }
}
````
becomes
````cs
public class MyError
    : ErrorInfo
{
    public MyError(int number)
        : base("my_code", "my_message")
    {
        this.Number = number;
    }
}
````
and using it:
````cs
var error = new ErrorInfo("error", "message")
{
    Inner = new MyInnerError(23),
};
````
becomes
````cs
var error = new MyError(23);
````
or
````cs
var error = new ErrorInfo("error", "message")
{
    Details = new[] { new MyError(23), },
};
````

# Suggested Changes

The following are changes that are not required, but may be useful to you.

## CustomDataSourceBase
The method `SetApplicationEnvironmentCore` is now virtual, so you no longer have
to override it if you do not want to. An `ApplicationEnvironment` property is
now available on the base class that you may reference in your `ProcessingSource.`

## Name changes

The name "custom data source" will be phased out by SDK v1.0.0 release candidate 1. Any classes and 
interfaces using this terminology must be renamed at that time. Existing classes have been marked 
obsolete and will be removed by v1.0.0 release candidate 1.

To prevent build breaks in the future, we recommend updating the following references:
- `CustomDataSourceBase` -> `ProcessingSource`
- `CustomDataSourceAttribute` -> `ProcessingSourceAttribute`
- `CustomDataSourceInfo` -> `ProcessingSourceInfo`
- `ICustomDataSource` -> `IProcessingSource`

For v0.109, the SDK will still be compatible with any plugins using the now obsolete classes and interfaces. 

## DataCookerPath

The following have been marked as obsolete and will be removed by SDK v1.0.0 release candidate 1:
- `DataCookerPath.Format`
- `DataCookerPath.EmptySourceParserId`
- `DataCookerPath.CookerPath`
- `DataCookerPath.Parse`
- `DataCookerPath.GetSourceParserId`
- `DataCookerPath.GetDataCookerId`
- `DataCookerPath.IsWellFormed`
To prevent future build breaks, remove references to these methods and properties.

Both the constructors for `DataCookerPath` that accept string arguments and `DataCookerPath.Create` 
have been marked obsolete and will be removed by SDK v1.0.0 release candidate 1. 
To prevent future build breaks, replace calls to this class' constructor with either
- `DataCookerPath.ForComposite`
- `DataCookerPath.ForSource`
depending on the type of data cooker the path is for.

## DataOutputPath

The following have been marked as obsolete and will be removed by SDK v1.0.0 release candidate 1:
- `DataOutputPath.Format`
- `DataOutputPath.Path`
- `DataOutputPath.Combine`
- `DataOutputPath.GetSourceId`
- `DataOutputPath.GetDataCookerId`
- `DataOutputPath.GetDataCookerPath`
- `DataOutputPath.TryGetConstituents`
- `DataOutputPath.IsWellFormed`
To prevent future build breaks, remove references to these methods and properties.

Both the constructors for `DataOutputPath` that accept string arguments and `DataOutputPath.Create` 
have been marked obsolete and will be removed by SDK v1.0.0 release candidate 1. 
To prevent future build breaks, replace calls to this class' constructor with either
- `DataOutputPath.ForComposite`
- `DataOutputPath.ForSource`
depending on the type of data cooker the data output path is for.

## TableConfiguration

The property `Layout` has been marked obsolete and will be removed prior to SDK v1.0.0 
release candidate 1. If you are setting this property on a `TableConfiguration`, you 
can move to setting the property on the table's `TableDescriptor`.
