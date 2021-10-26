# Abstract

This document outlines using the `Engine` to programmatically process data.

# Overview

The `Engine` is used to programmatically process data. There are three (3)
major components to be aware of when using the `Engine`:
- The `PluginSet`
- The `DataSourceSet`
- The `Engine` itself

## Plugin Set

The `PluginSet` is a collection of all of your plugins and extensions that can
be used to process data from data sources (e.g. files). You can create a new
`PluginSet` by loading one or more extension directories:

````cs
using (var plugins = PluginSet.Load("c:\\my\\plugin\\directory"))
{
    // Use the set
}
````

In advanced scenarios, overloads are provided to allow for you to pass your
own `IAssemblyLoader` or `IPreloadValidator`. An example of this would be if
you had an `IAssemblyLoader` implementation that loaded each plugin into its
own isolated context.

If you do not specify any directories, then the `PluginSet` will load all
plugins that are found in the current working directory.

## DataSourceSet

The `DataSourceSet` is a collection of `IDataSource` instances that are to be
processed by an `Engine` instance. The `DataSourceSet` will reference a
`PluginSet` in order to validate that any `IDataSource` instances added to the
`DataSourceSet` are able to be processed.

````cs
using var plugins = PluginSet.Load();
using var dataSources = DataSourceSet.Create(plugins);

dataSources.AddDataSource(new FileDataSource("myfile.txt"));
dataSources.AddDataSource(new FileDataSource("yourfile.txt"));

// ... and so on

````

If an attempt is made to add an `IDataSource` to the `DataSourceSet` for which
no plugin can process, then the `DataSourceSet` will throw an exception. The
`DataSourceSet` class provides Try versions of each of the methods (e.g.
`TryAddDataSource`) that will not throw if the `IDataSource` does not have a
plugin to process the data source.

Finally, you can specify that the `DataSourceSet` should take ownership of the
`PluginSet`. This means that when the `DataSourceSet` is disposed, the
`DataSourceSet` will also dispose the `PluginSet`. If you wish the `PluginSet`
to be used many times, then use the `DataSourceSet.Create(PluginSet, bool)`
overload. By default, the `DataSourceSet` will take ownership of the `PluginSet`
so you *MUST* use the overload if you do not want this to occur.

NOTE: at this time, reusing the `PluginSet` is not fully implemented, and so
`Create(PluginSet, false)` will throw a `NotSupportedException`. The ability
to pass `false` will be added in a future update.

````cs
using (var plugins1 = PluginSet.Load())
{
    using (var dataSources = DataSourceSet.Create(plugins, true))
    {
    }

    // plugins1 has now also been disposed.
}
````

## Engine

In order to use the `Engine`, you must first load your plugins and data sources.
Once you have done so, you may use the `Create` method with an `EngineCreateInfo`
instance in order to create a useable `Engine.` Similar to how the `DataSourceSet`
can take ownership of the `PluginSet`, the `Engine` takes ownership of the
`DataSourceSet`. Set the `OwnsDataSources` property on the `EngineCreateInfo`
instance to suppress this behavior.
Once your `Engine` has been created, you can enable cookers and tables to
participate in processing. If an attempt is made to enable a cooker for which
there is no corresponding data source in the `DataSourceSet`, then an exception
will be thrown. You may use the Try versions of the Enable methods to avoid
the exceptions.
Once you are ready, call process and then examine the results.

````cs
using (var plugins = PluginSet.Load())
using (var dataSources = DataSourceSet.Create(plugins))
{
    dataSources.Add(new FileDataSource("myfile.txt");

    var createInfo = new EngineCreateInfo(dataSources);
    using (var engine = Engine.Create(info))
    {
        engine.EnableCooker(DataCookerPath);

        var results = engine.Process();
    }
}
````

