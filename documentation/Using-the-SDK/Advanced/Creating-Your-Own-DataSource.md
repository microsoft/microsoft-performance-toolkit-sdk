# Abstract

This document outlines a scenario where you want to implement a Data Source
beyond the FileDataSource and DirectoryDataSource provided by the SDK.

# Motivation

For users that have data stored somewhere other than Files or Directories, the
ability to have their Data Source treated the same as a first class Data Source
by the SDK is desireable. For example, let's suppose that you are writing many
Custom Data Sources that get their data from a SQL database, or from a web
service. Rather than having a file with a URI in it, it would be nice to say
that your Custom Data Source uses a SqlServerDataSource, or a 
WebServiceDataSource.

# Requirements

In order to have your new Data Source recognized by the SDK, you must do the
following:

- Create a new class inheriting from  _DataSource_ (or implementing _IDataSource_)
- Create a new Attribute inheriting from _DataSourceAttribute_ specifying the
  Type of the class you created
- Optionally implement the _Accepts_ method on your new attribute.

# Implementation

This section is going to outline implementing a new Data Source Type to be
consumed by Custom Data Sources. For the purposes of this Walkthrough, we will
be implementing a Data Source backed by a WebService. This example is being
implemented such that all three aspects are represented; there are multiple way
to implement this.

## DataSource class

We first create our Data Source class. This is how users tell us where the
Web Service is located.

````cs
public class WebServiceDataSource
    : DataSource
{
    public SqlDataSource(Uri serviceUri)
        : base(serviceUri)
    {
        this.ServiceUri = serviceUri;
    }

    public Uri ServiceUri { get; }
}
````

## DataSourceAttribute

We need some way for our Custom Data Sources to denote that they accept our
WebServiceDataSource. So we will create a new DataSourceAttribute:

````cs

// we are decorating Custom Data Source classes, so we must have the correct
// attribute targets
[AttributeUsage(AttributeTargets.Class)]
public sealed class WebServiceDataSourceAttribute
    : DataSourceAttribute
{
    // our constructor will pass the DataSource Type to the base class. We
    // can take any other parameters that we like to help denote our data. In
    // this case, we simply say that data from the given service is supported.
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

Finally, by default, all Data Sources of a given type will be routed to 
Custom Data Sources that are decorated with the corresponding attribute. In
this case, _ANY_ instance of a WebServiceDataSource will be routed to our Custom
Data Source, and the IsDataSourceSupported method would have to do interrogation
of said Data Source to determine if it is supported. Wouldn't it be nice if
there was a way to say "I only care about certain servces; don't even bother
with others?" Yes, through the _Accepts_ method. This method allows for you
to filter incoming DataSource objects before they ever reach your Custom Data
Source so that your logic is simpler. For example, the FileDataSourceAttribute
in the SDK uses this method to reject anything that does not match the given
file extension, so Custom Data Sources have _FileDataSource(".txt")_, for
example, and only ever see .txt files. You should still implement
IsDataSourceSupported in your CustomDataSource to do the final determination.

````cs
// we are decorating Custom Data Source classes, so we must have the correct
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
    // accepted by the decorated Custom Data Source.
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

# Using Your Data Source 

Now that we have our new Data Source, we will see how to use our new Data Source
with the Engine.

````cs

namespace Sample
{
    [CustomDataSource(
        /* Guid here */,
        /* Name here */,
        /* Description here */)]
    [WebServiceDataSource("https://www.contoso.com")]
    public class ContosoDataSource
        : CustomDataSourceBase
    {
        // implementation elided
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

We have seen how to create a new kind of Data Source an how to seamlessly
allow Custom Data Sources to leverage our new Data Source.
