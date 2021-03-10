# Abstract

This document outlines making your Custom Data Sources and Extensions
disposable.

# Motiviation

Your extension manages items that require cleanup, and you would like to be
able to implement `IDisposable` and have the SDK's runtime (Runtime) make sure
that your extension is disposed when executioin is complete.

# Usage

In order for the Runtime to invoke your `IDisposable` implementation, all you
need to do is make sure your extension implmenets `IDisposable`.

Users may add the `IDisposable` interface to types implementing the following
interfaces, whether they are implemented directly or via one of the SDK 
provided base classes:
- `ICustomDataSource`
- `ICustomDataProcessor`
- `ISourceDataCooker`
- `ICompositeDataCookerDescriptor`
- `IDataProcessor`

Example:
````
public class MyDisposableCooker
    : ICompositeDataCookerDescriptor,
      IDisposable
{
    // ...
}

public class MyDisposableCustomDataSource
    : CustomDataSourceBase,
      IDisposable
{
    // ...
}
````

When this interface is present, then the implementation will be disposed when
the Runtime is disposing. This makes disposal 'opt-in' for users without forcing
`IDisposable` onto implementations that may not need it.

Example:
````
using (var engine = Engine.Create())
{
    engine.EnableCooker(...);

    // ...

    var resultes = engine.Process();

    // ...

} // all cookers, data sources, etc. cleaned up here
````

Upon disposal, the Runtime will walk the graph of all loaded extensions and
clean them up in order based on their dependencies. For example, if you have
a `Composite Cooker` that dependes on a `Source Cooker`, then the `Composite
Cooker` will be disposed _before_ the `Source Cooker`. Extensions are disposed
before any of their dependencies are disposed.

Due to the relationship between CustomDataSources and CustomDataProcessors, an 
additional method has been added to the `ICustomDataSource` interface:
DisposeProcessor. This method is invoked on the Custom Data Source right before
a Custom Data Processor is to be cleaned up. This gives the Custom Data Source
an opportunity to clean up any state it might have related to the processor.
`CustomDataSourceBase` provides a default implementation of this method that
does nothing.

# Additional Notes

You are not required to implement `IDisposable` on your extensions. If 
`IDisposable` is not present, then no special cleanup logic is performed for
that implementation.

# Conclusion

We have seen how to make our extensions disposable in order to allow for the
Runtime to automatically dispose our extensions.

[Back to Advanced Topics](Overview.md)
