// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.

#nullable enable

using Microsoft.Performance.SDK.ColumnCommands;
using System;

namespace Microsoft.Performance.SDK.Processing.ColumnBuilding;

public class ColumnBuilder<T>
{
    private DataColumnCommands? commands = null;

    public ColumnBuilder(
            ColumnMetadata metadata,
            UIHints displayHints,
            IProjection<int, T> projection)
    {
        Guard.NotNull(metadata, nameof(metadata));
        Guard.NotNull(displayHints, nameof(displayHints));
        Guard.NotNull(projection, nameof(projection));

        this.Projection = projection;
        this.Configuration = new(metadata, displayHints);
    }

    public ColumnBuilder(
            ColumnConfiguration configuration,
            IProjection<int, T> projection)
    {
        Guard.NotNull(configuration, nameof(configuration));
        Guard.NotNull(projection, nameof(projection));

        this.Projection = projection;
        this.Configuration = configuration;
    }

    protected ColumnConfiguration Configuration { get; }

    protected IProjection<int, T> Projection { get; }

    protected Func<RootColumnBuilder, ColumnBuilder>? VariantOptions { get; set; } = null;

    public ColumnBuilder<T> WithCommands(
        DataColumnCommands commands)
    {
        this.commands = commands;
        return this;
    }

    public ColumnBuilder<T> WithVariants(
        Func<RootColumnBuilder, ColumnBuilder> options)
    {
        this.VariantOptions = options;
        return this;
    }

    public ITableBuilderWithRowCount AddToTable(
        ITableBuilderWithRowCount tableBuilder)
    {
        DataColumn<T> dataColumn = BuildColumn(this.commands);

        if (this.VariantOptions is not null)
        {
            return tableBuilder.AddColumnWithVariants(dataColumn, this.VariantOptions);
        }

        return tableBuilder.AddColumn(dataColumn);
    }

    protected virtual DataColumn<T> BuildColumn(
        DataColumnCommands? commands)
    {
        return new(this.Configuration, this.Projection, commands);
    }
}
