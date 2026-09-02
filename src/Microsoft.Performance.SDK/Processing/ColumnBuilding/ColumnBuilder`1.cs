// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.

using Microsoft.Performance.SDK.ColumnCommands;
using System;

namespace Microsoft.Performance.SDK.Processing.ColumnBuilding;

public class ColumnBuilder<T>
{
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

    protected DownloadSourceCodeCommand<T> DownloadSourceCommand { get; set; } = null;

    protected Func<RootColumnBuilder, ColumnBuilder> VariantOptions { get; set; } = null;

    public ColumnBuilder<T> WithDownloadSourceCodeCommand(
        DownloadSourceCodeCommand<T> downloadSourceCommand)
    {
        this.DownloadSourceCommand = downloadSourceCommand;
        return this;
    }

    public ColumnBuilder<T> WithVariants(
        Func<RootColumnBuilder, ColumnBuilder> options)
    {
        this.VariantOptions = options;
        return this;
    }

    public ITableBuilderWithRowCount AddColumn(ITableBuilderWithRowCount tableBuilder)
    {
        DataColumn<T> dataColumn = BuildColumn();
        dataColumn.Commands.DownloadSourceCodeCommand = this.DownloadSourceCommand;

        if (this.VariantOptions is not null)
        {
            return tableBuilder.AddColumnWithVariants(dataColumn, this.VariantOptions);
        }

        return tableBuilder.AddColumn(dataColumn);
    }

    protected virtual DataColumn<T> BuildColumn()
    {
        return new(this.Configuration, this.Projection);
    }
}
