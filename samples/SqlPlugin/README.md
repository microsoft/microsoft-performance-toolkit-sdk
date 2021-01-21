# Introduction
This solution is the source code for the SQL plugin created during a demonstration on how to use the SDK.

Step-by-step video walkthroughs on creating this plugin can be found at:
* Part 1: creating a simple SQL custom data source and table (TODO: add link once uploaded)
* Part 2: recreating the SQL table using the data-processing pipeline (TODO: add link once uploaded)

This plugin consists of two custom data sources (each defined in its own project/assembly/namespace). Both 
custom data sources implement logic for parsing `.xml` files that contain [SQL Server Profiler](https://docs.microsoft.com/en-us/sql/tools/sql-server-profiler/sql-server-profiler?view=sql-server-ver15) 
trace data (i.e. a `.xml` file created by exporting a `.trc` file).

Each project contains one table that displays information contained within the `.xml` file parsed. 
The simple SQL custom data source's table displays all events inside the XML (except 
the `Trace Start` and `Trace End` events). The data-processing pipeline table receives 
its data by querying a data cooker that receives only `SQL:BatchStarting` and `SQL:BatchCompleted` 
events from the source parser. As such, it only displays those events. The data cooker this 
table depends on is also responsible for mapping raw `SqlEvent` data classes to `SqlEventWithRelativeTimestamp` 
data classes, which augment the raw data with a `Timestamp` representing when a given event 
occurred relative to the `Trace Start` event. For the simple custom data source, this 
conversion happens inside the `ProcessAsyncCore` method of the custom data processor.

Both custom data sources only look for/display a subset of fields which a SQL trace may contain. This was done to make 
the plugin easier to understand and to better serve as a live demonstration. A full implementation of this plugin 
would likely have a different structure.

# Requirements
1. [Visual Studio 2019](https://visualstudio.microsoft.com/downloads/)
2. [.NET Standard 2.0](https://dotnet.microsoft.com/download/visual-studio-sdks)