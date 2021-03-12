# Abstract

This document outlines major code changes that might be needed when updating from Preview Version 0.108.* to Preview Version 0.109.*

# Code Updates

There are a number of breaking changes in this version; please see the [CHANGELOG](../../CHANGELOG.md) for a list of these changes.

## Custom Data Source

In order to update your `CustomDataSource`, you will need to
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
 If you are implementing `ICustomDataSource` directly then you will change you `IsFileSupported(string)` method to `IsDataSourceSupported(IDataSource)` and update the logic similar to above.

 ## Engine

The following are required if you are using the `Engine`:

- Try-catch blocks that were expecting `UnsupportedDataSourceException`s to signal an invalid `CustomDataSource` in any of the `Add*` methods should be updated to catch `UnsupportedCustomDataSourceException`:
````cs
try
{
    engine.AddFile("test", typeof(BadCustomDataSource))
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
    engine.AddFile("test", typeof(BadCustomDataSource))
}
catch (UnsupportedCustomDataSourceException)
{
    // ...
}
````

- Try-catch blocks that were expecting `UnsupportedFileException`s to signal an invalid file in any of the `Add*` methods should be updated to catch `UnsupportedDataSourceExcepton`:
````cs
try
{
    engine.AddFile("test.nope")
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
    engine.AddFile("test.nope")
}
catch (UnsupportedDataSourceException)
{
    // ...
}
````

# IDataSource

## GetUri

Anywhere you where calling `IDataSource.GetUri()` can be simply updated to `IDataSource.Uri`