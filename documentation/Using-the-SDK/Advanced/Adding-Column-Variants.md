# Abstract

This document outlines how to configure a [column](../../Glossary.md#column) with different types of [column variants](../../Glossary.md#column-variant). Additionally, this document provides guidelines on when to use each type of column variant and when you should consider making column variants separate table columns.

# Motivation

There are often instances where a single piece of data can be expressed or displayed in multiple ways. Prior to SDK version `1.3`, table authors were forced to add one column for each way the data can be displayed.

For example, suppose a table author exposes a "Process" column whose data that identifies which process is associated with a given row. This data can be expressed in multiple ways:
* The process name
* The process numeric identifier (PID)
* The process name + PID

Without column variants, the table author would have to add three independent columns to a table to expose each way to represent the data. With column variants, however, the table author can add one "Process" column with three *mode variants* - one for each representation.

# When to Use Column Variants

Since column variants are defined as an alternative projection `IProjection<int, T>` with no constraint on the type `T`, there is no technical limitation on what data is exposed through a variant. As such, table authors should take care to ensure column variants make sense to be grouped together as one column. There are two restrictions on column variants that help guide whether to expose projections as variants or new columns:

1. Column variants cannot supply a new [column configuration](../../Glossary.md#columnconfiguration) for the column that the variant is for. As such, column variants should only be used when the projected data still satisfies the name and description of the base column's configuration. If a user would be confused how a column variant's data relates to its base column, you should consider exposing the variant as its own dedicated column. Note that in some cases it may make sense to edit a column configuration's name and description to accommodate new variants; for example, a column named "Local Time" could be renamed to "Time" and be supplied variants for different timezones.

2. Column variants cannot be used as a [column role](../../Glossary.md#columnrole) or highlight entries within a [column configuration](../../Glossary.md#columnconfiguration). When column roles or highlight entries are defined, they are always associated with the column's base projection.

---



There are two types of column variants: toggles and modes. While all variants types expose the same information (an alternative [projection](#projection) for some base [column](#column)), they differ in how they relate to other defined variants.

### Toggle Column Variants
A toggle column variant is a *single* variant that is mutually exclusive to a *parent* projection. Conceptually, it offers users a way to "toggle" to an alternate view of the toggle's parent projection.

For example, suppose a base column is added that exposes `DateTime` information relative to UTC. A "as local time" toggle may be added that projects each value to a `DateTime` relative to the computer's timezone.

Toggles can be added recursively on top of each other: it is possible to have a toggle from projection `A` to `B`, then another toggle from `B` to `C`, and so on.

### Mode Column Variants
Mode column variants are *a collection* of variants that are mutually exclusive to *each other*. Conceptually, they offer users a way to choose between one or more alternate views. 

There are two ways to define a set of modes
1. Modes are defined at the same level as the base column, where the base column's projection is one of the available variants in the set. For example, on a base column that exposes `DateTime` information relative to UTC, you may define additional mode variants for other timezones and have "UTC" (the base projection) be one of the modes.
2. Modes are defined 
