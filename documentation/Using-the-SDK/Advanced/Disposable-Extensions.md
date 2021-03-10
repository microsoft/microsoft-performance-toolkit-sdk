# Abstract

This document outlines how to make a `Custom Data Source` or SDK
extension disposable.

# Motiviation

When your extension manages items that require cleanup, it would be useful to
implement `IDisposable` and have the SDK's runtime make sure that your
extension cleans up its resources.

# Usage

In order for the SDK runtime to invoke your `IDisposable` implementation, you
only need to make sure your extension implmenets `IDisposable`.

Users may add the `IDisposable` interface to types implementing the following
interfaces:
- `ICustomDataSource`
- `ICustomDataProcessor`
- `ISourceDataCooker`
- `ICompositeDataCookerDescriptor`
- `IDataProcessor`

Example:
````cs
public class MyDisposableCooker
    : ICompositeDataCookerDescriptor,
      IDisposable
{
    // ...
}
````

You can add `IDisposable` on types deriving any base classes that implement the
above interfaces, too.

````cs
public class MyDisposableCustomDataSource
    : CustomDataSourceBase,
      IDisposable
{
    // ...
}
````

When this interface is present, the implementation will be disposed when
the SDK runtime itself is disposed. Disposal is __opt-in__, so users need not
implement `IDisposable` on classes that do not need it.

Here is an example of the SDK runtime being used in a way that would allow your
types to be cleanly disposed:
````
using (var engine = Engine.Create())
{
    engine.EnableCooker(...);

    // ...

    var resultes = engine.Process();

    // ...

} // all cookers, data sources, etc. cleaned up here
````

Upon disposal, the SDK runtime will determine the relationship between all
loaded modules and clean them up in order based on their dependencies. For
example, if you have a `CompositeCooker` that dependes on a `SourceCooker`,
then the `CompositeCooker` will be disposed of _before_ the `Source Cooker`.
In short, all extensions are disposed of before any of their dependencies are
disposed.

Due to the relationship between a `CustomDataSource` and a
`CustomDataProcessor`, an additional method has been added to the
`ICustomDataSource` interface: DisposeProcessor. This method is invoked on the
`CustomDataSource` right before a `CustomDataProcessor` is to be cleaned up.
This gives the `CustomDataSource` an opportunity to clean up any state it might
have related to its processor. `CustomDataSourceBase` provides a default
implementation of this method that does nothing.

# Additional Notes

You are not required to implement `IDisposable` on your extensions. If 
`IDisposable` is not present, then no special cleanup logic is performed for
that implementation.

# Conclusion

We have seen how to make our extensions disposable in order to allow for the
SDK runtime to automatically dispose of objects created by our plugins.

[Back to Advanced Topics](Overview.md)
