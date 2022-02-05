# Building a Table

Table construction happens by interacting with an instance of an `ITableBuilder`. Broadly, `Tables` consist of `TableConfigurations` and columns, which are composed of `ColumnConfigurations` and `Projections`.

* [Adding Columns](#adding-columns)
  * [Setting the table's row count](#setting-the-tables-row-count)
  * [Declaring column configurations](#declaring-column-configurations)
  * [Defining column projections](#defining-column-projections)
  * [Adding columns to the table](#adding-columns-to-the-table)
* [Adding TableConfigurations](#adding-tableconfigurations)
  * [ColumnRole](#columnrole)

## Adding Columns

A column is a conceptual pair (`ColumnConfiguration`, `Projection`) that defines data inside a table. Every column inside of a table must be composed of the same number of rows. Because of this, before we can add columns to the table, we must specify the table's row count.

Adding columns to a table can therefore be broken down into the following 4 steps:
1. Setting the table's row count
2. Declaring column configurations
3. Defining column projections
4. Adding columns to the table

### Setting the table's row count

To add columns to our table, we first must get an instance of `ITableBuilderWithRowCount`. This is accomplished by calling `ITableBuilder.SetRowCount`. For example, if we have an array of data where each element will become a row in the final table, we can do

```cs
ITableBuilderWithRowCount tableBuilderWithRowCount = tableBuilder.SetRowCount(myData.Count);
```

> ❗Important: Every projection in the table must support the table's established row count.

### Declaring column configurations

Each column in a table must have an associated `ColumnConfiguration`. A `ColumnConfiguration` contains metadata information about a column. For example, it contains the column's name and description, along with `UIHints` that help GUI-based SDK drivers render the column.

Typically, `ColumnConfigurations` are stored as `static` fields on the `Table` class for the `Table` being built. For example,

```cs
[Table]                      
public sealed class WordTable
{
    ...

    private static readonly ColumnConfiguration lineNumberColumn = new ColumnConfiguration(
        new ColumnMetadata(new Guid("75b5adfe-6eee-4b95-b530-94cc68789565"), "Line Number"),
        new UIHints
        {
            IsVisible = true,
            Width = 100,
        });

    private static readonly ColumnConfiguration wordCountColumn = new ColumnConfiguration(
        new ColumnMetadata(new Guid("d1c800e5-2d19-4474-8dad-0ebc7caff3ab"), "Number of Words"),
        new UIHints
        {
            IsVisible = true,
            Width = 100,
        });
}
```

### Defining column projections


A column's `Projection` is a function that maps a row index for a column to a piece of data. Projections are normally constructed at runtime by a `Build` method, since they depend on the data inside the final table. The SDK offers many helper methods for constructing `Projections`, such as:
* `Projection.Index(IReadOnlyList<T> data)`, which projects a column index `i` to `data[i]`
* `Projection.Compose(this IProjection<T1, T2>, Func<T2, TResult>)` which composes one `IProjection` with another method

For example, suppose we have a collection of `LineItem` objects that each have two properties: `LineNumber` and `Words`. We can use the above helper methods to create the following projections:

```cs
var baseProjection = Projection.Index(myLineItemCollection);

var lineNumberProjection = baseProjection.Compose(lineItem => lineItem.LineNumber);
var wordCountProjection = baseProjection.Compose(lineItem => lineItem.Words.Count());
```

### Adding columns to the table

With the `ColumnConfigurations` and `Projections` above, we can add them to the table we're building by calling `AddColumn` on the `tableBuilderWithRowCount` created above:

```cs
tableBuilderWithRowCount.AddColumn(this.lineNumberColumn, lineNumberProjection);
tableBuilderWithRowCount.AddColumn(this.wordCountColumn, wordCountProjection);
```

Note that _every_ column a table provides must be added through a call to `ITableBuilderWithRowCount.AddColumn`, even if they're not used in a `TableConfiguration` (see below).

## Adding TableConfigurations

Some tables may have _many_ columns available. In these situations, it is not useful for a user to be shown every single column at once. A `TableConfiguration` describes groupings of columns that should be used together, along with metadata information such as `ColumnRoles`. Every `Table` must provide at least one `TableConfiguration`.

For example, here is a `TableConfiguration` that contains both of columns above:

```cs
var tableConfig = new TableConfiguration("All Data")
{
    Columns = new[]
    {
        this.lineNumberColumn,
        this.wordCountColumn,
    },
};
```

You also specify [Special Columns](../Glossary.md#special-columns) in a `TableConfiguration`.

Once a `TableConfiguration` is created, it is added to the table we're building by calling `ITableBuilder.AddTableConfiguration`:

> ⚠️ Note: `AddTableConfiguration` must be called on `ITableBuilder`, *not* on `ITableBuilderWithRowCount`.

```cs
tableBuilder.AddTableConfiguration(tableConfig);
```

It is also recommended to set a default `TableConfiguration`:
```cs
tableBuilder.SetDefaultTableConfiguration(tableConfig);
```

### ColumnRole

A `ColumnRole` is metadata information about a column that defines the column's role in data presentation. For example, a column of `Timestamp` values relative to the start of a `DataSource` may be marked as a "start time" Column:

```cs
tableConfig.AddColumnRole(ColumnRole.StartTime, relativeTimestampColumnConfiguration);
```

# More Information
There are many other things you can do with your `Tables`, such as adding `TableCommands` or configuring its default layout style. For more information, refer to the [Advanced Topics](./Advanced/README.md).