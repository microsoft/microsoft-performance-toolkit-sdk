# Abstract

This document outlines a scenario where you want to implement a `DataSource`
beyond the `FileDataSource` and `DirectoryDataSource` provided by the SDK.

# Motivation

For users that have data stored somewhere other than files or directories, it is
desirable for the SDK to treat their `DataSource` as a first class `DataSource`.
For example, suppose that you are writing many `CustomDataSource`s that get
their data from a SQL database or from a web service. Rather than having a file
with a URI in it, it would be nice to say that your `Custom DataSource` uses a 
`SqlServerDataSource` or a `WebServiceDataSource`.

# Requirements

In order to have your new `DataSource` recognized by the SDK, you must do the
following:

- Create a new class inheriting from `DataSource`.
    - You may also implment `IDataSource` directly.
- Create a new Attribute inheriting from `DataSourceAttribute`, and specify the
  type of the class you created in the call to the base constructor.
- Optionally implement the `Accepts` method on your new attribute.

Once you have implemented these three items, the SDK will recognize your new
`DataSource` and route it to `CustomDataSource`s as appropriate.

# Implementation

This section outlines implementing a new `DataSource` type to be
consumed by `CustomDataSource`s. For the purposes of this walkthrough, we will
be implementing a `DataSource` backed by data from a web service. This example will
- Create a new `WebServiceDataSource` class derived from `DataSource`.
- Create a new `WebServiceDataSourceAttribute` so that `CustomDataSource`s
  can consume `WebServiceDataSource`s
- Implement `Accepts` on the `WebServiceDataSourceAttribute`

## DataSource class

We first create our `WebServiceDataSource` class. This is how users tell us where the
Web Service is located.

````cs
public class WebServiceDataSource
    : DataSource
{
    public WebServiceDataSource(Uri serviceUri)
        : base(serviceUri)
    {
        this.ServiceUri = serviceUri;
    }

    public Uri ServiceUri { get; }
}
````

## DataSourceAttribute

We need some way for our `CustomDataSource`s to denote that they accept our
`WebServiceDataSource`. So we will create a new `WebServiceDataSourceAttribute`:

````cs

// we are decorating Custom `DataSource` classes, so we must have the correct
// attribute targets
[AttributeUsage(AttributeTargets.Class)]
public sealed class WebServiceDataSourceAttribute
    : DataSourceAttribute
{
    // our constructor will pass the DataSource Type to the base class. We
    // can take any other parameters that we like to help denote data.
    public WebServiceDataSourceAttribute(
        string serviceUri)
        : base(typeof(WebServiceDataSource))
    {
        this.ServiceUri = new Uri(serviceUri);
    }

    public Uri ServiceUri { get; }
}
````

## Accepts

Finally, by default, all `DataSource`s of a given type will be routed to 
`CustomDataSource`s that are decorated with the corresponding attribute. Currently,
_ANY_ instance of a `WebServiceDataSource` will be routed to our `CustomDataSource`
decorated with the `WebServiceDataSourceAttribute`, and the `IsDataSourceSupportedCore`
method would have to do interrogation of said `DataSource` to determine if it is supported.
It would be nice if to say "I only care about certain servces; don't even bother
with others." This is what the `Accepts` method allows. The `Accepts` method is used
to filter incoming `DataSource` objects before they ever reach your `CustomDataSource`
so that your `IsDataSourceSupportedCore` logic is simpler. For example, the `FileDataSourceAttribute`
in the SDK uses this method to reject anything that does not match the given
file extension, so `CustomDataSource`s decorated with `FileDataSource(".txt")`, for
example, only ever see `.txt` files. You should still implement
`IsDataSourceSupportedCore` in your `CustomDataSource` to do the final determination.

````cs
// we are decorating Custom `DataSource` classes, so we must have the correct
// attribute targets
[AttributeUsage(AttributeTargets.Class)]
public sealed class WebServiceDataSourceAttribute
    : DataSourceAttribute
{
    // our constructor will pass the DataSource Type to the base class. We
    // can take any other parameters that we like to help denote our data. In
    // this case, we simply say that data on the given server is supported.
    public WebServiceDataSourceAttribute(
        string serviceUri)
        : base(typeof(WebServiceDataSource))
    {
        this.ServiceUri = new Uri(serviceUri);
    }

    public Uri ServiceUri { get; }

    // implement this method to eagerly filter out data sources that are not
    // accepted by the decorated Custom `DataSource`.
    public override bool Accepts(IDataSource dataSource)
    {
        // IDataSource will always be of the Type based to the base
        // class in our constructor.
        var web = dataSource as WebServiceDataSource;

        // This check is for defensive programming in case dataSource is
        // ever null.
        if (web is null)
        {
            return false;
        }

        // We only accept WebServiceDataSources that target our declared service.
        return this.ServiceUri == web.ServiceUri;
    } 
}
````

# Using Your `DataSource` 

Now that we have our new `DataSource`, we will see how to use our new `DataSource`
with the Engine.

````cs

namespace Sample
{
    [CustomDataSource(
        /* Guid here */,
        /* Name here */,
        /* Description here */)]
    [WebServiceDataSource("http://www.contoso.com")]
    public class ContosoDataSource
        : CustomDataSourceBase
    {
        protected override bool IsDataSourceSupportedCore(
            IDataSource dataSource
        )
        {
            // ...
        }

        // ...
    }

    public class Program
    {
        public static void Main(string[] args)
        {
            //
            // Prep our data sources. These could come from the
            // command line, or be hard coded.
            //

            var data1 = new WebDataSource("https://www.microsoft.com");
            var data2 = new WebDataSource("https://www.contoso.com");

            //
            // Create the Engine.
            //
            var engine = Engine.Create();

            //
            // Add our data
            //

            // data1 will not get associated to ContosoDataSource...
            engine.AddDataSource(data1);

            // ...but data2 will.
            engine.AddDataSource(data2);

            //
            // add any cookers, etc.
            //

            //
            // Process our data
            //
            var result = engine.Process();

            //
            // Deal with the processed data.
            //
        }
    }
}

````

# Conclusion

We have seen how to create a new kind of `DataSource` an how to seamlessly
allow Custom `DataSource`s to leverage our new `DataSource`.

[Back to Advanced Topics](Overview.md)
