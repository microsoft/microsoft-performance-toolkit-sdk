# Abstract
This guide will show how to create your first table


# Class Setup & [Table] attribute

# Column Configurations

# Building the table

## Telling WPA about our temporal range

WPA needs to know the timespan that is encompassed by your data. You inform WPA of this interval via the `GetDataSourceInfo`.
Fundamentally, WPA works with relative times, not absolute times. The easiest way to deal with time in WPA is to normalize everything to a zero based time stamp. In this example, we will make a note of the minimum and maximum times while processing, and then subtract the minimum time from each time to get the relative timestamp.
Then, in the `GetDataSourceInfo` method, use the nanosecond value of the minimum and maximum (that you stored away in the instance) to populate the DataSourceInfo object.

How does this fit into our tables? When we are creating our data for the tables, we will take the nanosecond diff of the parsed DATETIME and our found minimum to create a timestamp:
```` cs
var ticks = parsedDateTime.Ticks  minimumDateTime.Ticks;
var nanoseconds = timespan.Ticks * 100; // 1 tick = 100 ns. See DateTime documentation
var timestamp = Timestamp.FromNanoseconds(nanoseconds);
````

## Adding columns to make the data graphable.

Now, when we build our table, we can simply add a column that projects the row into a Timestamp:

(Pseudocode)

````cs
var allData = new List<Tuple<string, Timestamp, string>>();
foreach (var kvp in this.Data)
{
    allData.Add(Tuple.Create(kvp.Key, kvp.Value.Item1, kvp.Value.Item2));
}

var wordData = Projection.Index(allData);
var fileFunc = wordData.Compose(x => x.Item1);
var timeFunc = wordData.Compose(x => x.Item2);
var wordFunc = wordData.Compose(x => x.Item3);

tableBuilder.SetRowCount(allData.Count)
    .AddColumn(fileColumn, fileFunc)
    .AddColumn(timeColumn, timeFunc)
    .AddColumn(wordColumn, wordFunc);
````

# Table Configurations

Table Configurations describe how your table should be presented to the user: the columns to show, what order to show them, which columns to aggregate, and which columns to graph. You may provide a number of columns in your table, but only want to show a subset of them by default so as not to overwhelm the user. The user can still open the table properties in WPA to turn on or off columns.

The SDK allows for you to set one or multiple configurations to be supplied for your tables out of the box.

## Programmatic Configurations
__UNDER CONSTRUCTION__

To programmatically create a configuration, you will need to use the `TableConfiguration` class. After creating an instance of this class, you will add your ColumnConfiguration instances to the instance. Each column you add to a table has a column configuration, so you can simply reuse those. The table configuration class also exposes four (4) columns that WPA explicitly recognizes (and that *all* tables have implicitly)
* Pivot Column
* Graph Column
* Left Freeze Column
* Right Freeze Column

These columns are interleaved into your column configuration collection when creating the configuration.

Example: Let's say you have five columns: Id, Name, Manager, Tenure, and Title

Let's say we want to aggregate by manager, graph on tenure, and make sure title is always displayed. Then we would set the `Columns` property on our table configuration instance with a collection containing the following:
{ Id, Manager, PivotColumn, Left Freeze Column, Name, Title, Graph Column, Tenure, Right Freeze Column }

# Advanced Graphing
## Time Ranges
__UNDER CONSTRUCTION__
## StartTime, EndTime
__UNDER CONSTRUCTION__
## Chart Types
__UNDER CONSTRUCTION__

NOTES:

-Talk about time range in the callback in projectors

-Talk to dynamic column header projectors

- Mention <CopyLocalLockFileAssemblies>true</CopyLocalLockFileAssemblies>