# Building a Table

Table construction happens by interacting with an instance of an `ITableBuilder`. Broadly, `Tables` consist of `TableConfigurations` and Columns. Columns can be further broken down into `ColumnConfigurations` and `Projections`. An `ITableBuilder` has methods to accept Columns and `TableConfigurations`.

* [Column](#column)
  * [ColumnConfiguration](#columnconfiguration)
  * [Projection](#projection)
  * [Combining ColumnConfiguration and Projections](#combining-columnconfiguration-and-projections)
* [TableConfiguration](#tableconfiguration)
  * [ColumnRole](#columnrole)

## Column

A column is a conceptual (`ColumnConfiguration`, `Projection`) pair that defines data inside a [Table](#table).

### ColumnConfiguration

A `ColumnConfiguration` contains metadata information about a column. For example, it contains the column's name and description, along with `UIHints` that help GUI viewers know how to render the column.

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

### Projection

A column's `Projection` is a function that maps a row index for a column to a piece of data. Projections are normally constructed at runtime by a `Build` method, since they depend on the data inside the final table. The SDK offers many helper methods for constructing `Projections`, such as:
* `Projection.Index(IReadOnlyList<T> data)`, which projects a column index `i` to `data[i]`
* `Projection.Compose(this IProjection<T1, T2>, Func<T2, TResult>)` which composes one `IProjection` with another method

For example, suppose we have a collection of `LineItem` objects that each have two properties: `LineNumber` and `Words`. We can use the above helper methods to create the following projections:

```cs
var baseProjection = Projection.Index(myLineItemCollection);

var lineNumberProjection = baseProjection.Compose(lineItem => lineItem.LineNumber);
var wordCountProjection = baseProjection.Compose(lineItem => lineItem.Words.Count());
```

## Combining ColumnConfiguration and Projections

If we have the `ColumnConfigurations` and `Projections` above, we can add them to the table we're building by calling `ITableBuilder.AddColumn`:

```cs
tableBuilder.AddColumn(this.lineNumberColumn, lineNumberProjection);
tableBuilder.AddColumn(this.wordCountColumn, wordCountProjection);
```

Note that _every_ column a table provides must be added through a call to `ITableBuilder.AddColumn`, even if they're not used in a `TableConfiguration` (see below).

## TableConfiguration

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

You also specify [Special Columns](../Glossary.md##special-columns) in a `TableConfiguration`.

Once a `TableConfiguration` is created, it is added to the table we're building by calling `ITableBuilder.AddTableConfiguration`:

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
There are many other things you can do with your `Tables`, such as adding `TableCommands` or configuring its default layout style. For more information, refer to the [Advanced Topics](./Advanced/Overview.md).