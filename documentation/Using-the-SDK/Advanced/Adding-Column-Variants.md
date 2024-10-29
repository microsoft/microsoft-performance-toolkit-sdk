# Abstract

This document outlines how to configure a [column](../../Glossary.md#column) with [column variants](../../Glossary.md#column-variant). Additionally, this document provides guidelines on when to use each type of column variant and when you should consider making create separate columns instead of column variants.

# Motivation

There are often instances where a single piece of data can be expressed in multiple ways. Prior to SDK version `1.3`, table authors were forced to add one column for each way the data can be displayed.

For example, suppose a table has a "Process" column whose data identifies which process is associated with each row. This data can be expressed in multiple ways:
* The process name
* The process identifier (PID)
* The process name + PID

Without column variants, the table author would have to add three independent columns to a table to expose these three representations of the same data. With column variants, however, the table author can add one "Process" column with three variants - one for each representation.

# When to Use Column Variants

Column variants are defined as an alternative projection `IProjection<int, T>` with no constraint on the type `T`, which places no technical limitation on what data is exposed through a variant. As such, table authors should take care to ensure column variants make sense to be grouped together under one column. There are two restrictions on column variants that help guide whether to expose projections as variants or new columns:

1. Column variants cannot supply a new [column configuration](../../Glossary.md#columnconfiguration) for the column that the variant is for. As such, column variants should only be used when the projected data makes sense given the base column's configuration. If a user would be confused how a column variant's data relates to its base column, you should consider exposing the variant as a separate dedicated column. 

    Note that in some cases it may make sense to edit a column configuration's name and description to accommodate new variants; for example, a column named "Process Name" could be renamed to "Process" and be expose variants for process name, PID, and process name + PID.

2. Column variants cannot be used as a [column role](../../Glossary.md#columnrole) or highlight entries within a [column configuration](../../Glossary.md#columnconfiguration). When column roles or highlight entries are defined, they are always associated with the column's base projection.

# How to Add Column Variants

Column variants are added to a column by adding the base column to an `ITableBuilder` with the `AddColumnWithVariants` methods introduced in SDK version `1.3`. These methods take a callback `Func<RootColumnBuilder, ColumnBuilder>` whose return value defines all the variants to associate with the added column. The supplied `Func` will be invoked with a `RootColumnBuilder` supplied by the SDK runtime and must return the final `ColumnBuilder` configured with variants.

Note that unlike the `ITableBuilder`, column builders are purely functional components: every column builder method returns a **new instance** of a column builder. For example, consider the code snippet below:

```cs
tableBuilder
    .AddColumnWithVariants(baseConfig, projectionA, builder =>
    {
        // These modes are ignored because the return value from
        // WithMode(modeB, projectionB) is being discarded 
        builder
            .WithModes("Mode A")
            .WithMode(modeB, projectionB);

        // Will ignore added modes above because the returned value is building
        // off of the input builder
        return builder
                .WithToggle(toggleIdentifier, projectionToggle);
    });
```

The final column with only contain the base column with one toggle. **There will not be any modes associated with this column.** Even though `builder.WithModes` was called, the `ColumnBuilder` returned by the method was the supplied (empty) builder with one `WithToggle` added.

Any time a new projection is associated with a variant, a `ColumnVariantDescriptor` must be used to uniquely identify the added variant amongst all variants in the column. The `ColumnVariantDescriptor` is created either
* Explicitly by the table author and passed into a `ColumnBuilder` method, such as the case when calling `WithToggle`
* Implicitly by the SDK runtime by associating the added variant with the base column, such as the case when calling `WithModes`

## Column Variant Types

There are two primary types of column variants: toggles and modes. While all variant types expose the same information (an alternative [projection](../../Glossary.md#projection) for the base [column](../../Glossary.md#column)), they differ in how they relate to other defined variants.

### Toggle Column Variants
A toggle column variant is a *single* variant that is mutually exclusive to all of its *parent* projections. Conceptually, it offers users a way to "toggle" to an alternate view of the toggle's parent projection.

For example, suppose a base column is added that exposes `DateTime` information relative to UTC. An "as local time" toggle may be added that projects each value to a `DateTime` relative to the computer's timezone:

```cs
ColumnConfiguration columnConfiguration = 
    new ColumnConfiguration(new ColumnMetadata(new Guid("..."), "Time"));

IProjection<int, DateTime> asUtc = GetUtcProjection(); // Omitted for brevity

tableBuilder
    .AddColumnWithVariants(columnConfiguration, asUtc, builder =>
    {
        ColumnVariantDescriptor local = new(new Guid("..."), "As Local Time");

        return builder
                .WithToggle(
                    local,
                    asUtc.Compose(utc => utc.ToLocalTime()));
    });
```

Toggles can be added recursively on top of each other: it is possible to have a toggle from projection `A` to `B`, then another toggle from `B` to `C`, and so on.

### Mode Column Variants
Mode column variants are *a collection* of variants that are mutually exclusive to *each other*. Conceptually, they offer users a way to choose between one or more alternate views. Unlike toggle variants that can be in on/off states, a collection of modes should have one mode selected at all times.

There are two ways to define a set of modes:

1. Using `WithModes`, define modes at the base column level. In this configuration, the base column's projection is the first of the available modes in the collection. The base column variant's `ColumnVariantDescriptor` identifier will automatically be set to the `ColumnConfiguration`'s identifier.

    ```cs
    ColumnConfiguration columnConfiguration = 
        new ColumnConfiguration(new ColumnMetadata(new Guid("..."), "Time"));

    IProjection<int, DateTime> asUtc = GetUtcProjection(); // Omitted for brevity

    tableBuilder
        .AddColumnWithVariants(columnConfiguration, asUtc, builder =>
        {
            return builder
                    .WithModes("UTC")  // The name of the mode to associate with the base column projection
                    .WithMode(
                        new ColumnVariantDescriptor(new Guid("..."), "Local"),
                        asUtc.Compose(utc => utc.ToLocalTime()))
                    .WithMode(
                        new ColumnVariantDescriptor(new Guid("..."), "Binary"),
                        asUtc.Compose(utc => utc.ToBinary()));
        });
    ```

    `WithModes` can *only* be invoked from a `RootColumnBuilder`. It is not possible to call, for example, `WithToggle` followed by `WithModes`. This restriction is in place because, if you could do this, the column variant associated with the modes' direct parent would be overshadowed by the set of modes. If you wish to expose, for UX purposes, a toggle that exposes a set of modes, use `WithToggledModes` described below.

2. Using `WithToggledModes`, define modes whose entire collection represents a set of modes that are mutually exclusive to all of its parents. For example, a `Timestamp` column could offer a "as DateTime" toggle that itself allows users to select between UTC vs local time.

    ```cs
    ColumnConfiguration columnConfiguration = 
        new ColumnConfiguration(new ColumnMetadata(new Guid("..."), "Timestamp"));

    IProjection<int, Timestamp> timestampProjection = GetTimestampProjection(); // Omitted for brevity

    tableBuilder
        .AddColumnWithVariants(columnConfiguration, timestampProjection, builder =>
        {
            return builder
                    .WithToggledModes(
                        "as DateTime",
                        modesBuilder => 
                        {
                            ColumnVariantDescriptor utc = new(new Guid("..."), "UTC");
                            ColumnVariantDescriptor local = new(new Guid("..."), "Local");

                            IProjection<int, DateTime> utcProjection = ToUtcDateTime(timestampProjection);

                            return modesBuilder
                                .WithMode(utc, utcProjection)
                                .WithMode(local, utcProjection.Compose(utc => utc.ToLocalTime()));
                        });
        });
    ```

    `WithToggledModes` may be called after `WithToggle`, meaning you can end a chain of hierarchical toggles with a set of toggled modes. However, you are unable to continue adding regular toggles on top of toggled modes.

## Recursive Variants

Both `WithModes` and `WithMode` described above have overloaded methods that take a `Func<ToggleableColumnBuilder, ColumnBuilder>`. These methods allow you to add variants that are direct children of the mode that that is being added, letting you define "recursive" column variants.

For example, consider this code:

```cs
ColumnConfiguration columnConfiguration = 
    new ColumnConfiguration(new ColumnMetadata(new Guid("..."), "Time"));

IProjection<int, DateTime> asUtc = GetUtcProjection(); // Omitted for brevity
IProjection<int, DateTime> asLocal = asUtc.Compose(utc => utc.ToLocalTime();

tableBuilder
    .AddColumnWithVariants(columnConfiguration, asUtc, builder =>
    {
        return builder
                .WithModes("UTC")  // The name of the mode to associate with the base column projection
                .WithMode(
                    new ColumnVariantDescriptor(new Guid("..."), "Local"),
                    asLocal,
                    modeBuilder => 
                    {
                        ColumnVariantDescriptor withDST = new(new Guid("..."), "With DST");

                        return modeBuilder
                                .WithToggle(
                                    withDST,
                                    asLocal.Compose(local => FixDST(local)));
                    });
    });
```

This code will create a "Time" column that, at the root level, has two available modes:
- "UTC" mode, which uses the base column's projection
- "Local" mode, which converts each UTC datetime to local time

In addition to these modes, this code adds a toggle "With DST" to the "Local" mode. Conceptually, this means that when a user is choosing to display the time as a local DateTime, they have the option to toggle DST on and off.

If desired, it is also possible to define new sub-modes of a given mode using `WithToggledModes` in the callback.

The ability to recursively defined column variants within a mode makes it possible to define arbitrarily complex trees of column variants. For a better user experience, it is recommended to limit the number of levels of column variants; if your column has a complex tree of variants, you should consider creating new columns instead.

# Defining Default Variants
Starting in SDK version `1.3`, you can specify on a `ColumnConfiguration` the `Guid` of `ColumnVariantDescriptor` that should be used as the default presentation of the column. You may also add this property to any prebuilt table configuration JSON files, as long as your JSON file uses version `1.3` of the JSON schema.


# Using Column Variants via the Engine

To use column variants in the SDK Engine, please refer to the "Using Column Variants" section of the [Using the Engine](../Using-the-engine.md#using-column-variants) documentation.

# Using Column Variants via the SDK Runtime

The SDK runtime does not directly expose a collection of column variants that can be selected. Instead, each concrete `TableBuilder` instances exposes an `IColumnVariantsRegistrar` that can be used to find column variants that were added during table building.

The `IColumnVariantsRegistrar` has a single method for getting the root `IColumnVariantsTreeNode` for a given base `IDataColumn` (i.e. the columns inside `TableBuilder.Columns`).

These tree nodes follow a visitor pattern that allow SDK runtime users to discover both the `IDataColumn` instances that represent column variants as well as how the different variants relate to each other. Once you obtain the root tree node, you must create an `IColumnVariantsTreeNodesVisitor` implementation that the root node can `Accept`. 