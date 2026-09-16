// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.

#nullable enable

using System;
using Microsoft.Performance.SDK.ColumnCommands;

namespace Microsoft.Performance.SDK.Processing.ColumnBuilding;

/// <summary>
///     Builds a strongly-typed <see cref="DataColumn{T}"/> and adds it to an
///     <see cref="ITableBuilderWithRowCount"/>. Plugins configure the column's
///     metadata, projection, commands, and optional variants on an instance
///     of this class and then pass it to
///     <see cref="ITableBuilderWithRowCount.AddColumn{T}(ColumnBuilder{T})"/>
///     to materialize the column on a table.
/// </summary>
/// <typeparam name="T">
///     The type of data produced by the column's projection.
/// </typeparam>
public class ColumnBuilder<T>
{
    private DataColumnCommands? commands = null;

    /// <summary>
    ///     Initializes a new instance of the <see cref="ColumnBuilder{T}"/>
    ///     class from the specified metadata, display hints, and projection.
    ///     A <see cref="ColumnConfiguration"/> will be constructed from
    ///     <paramref name="metadata"/> and <paramref name="displayHints"/>.
    /// </summary>
    /// <param name="metadata">
    ///     The metadata describing the column.
    /// </param>
    /// <param name="displayHints">
    ///     The UI hints describing how the column should be displayed.
    /// </param>
    /// <param name="projection">
    ///     The projection that produces the column's values.
    /// </param>
    /// <exception cref="ArgumentNullException">
    ///     <paramref name="metadata"/>, <paramref name="displayHints"/>, or
    ///     <paramref name="projection"/> is <c>null</c>.
    /// </exception>
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

    /// <summary>
    ///     Initializes a new instance of the <see cref="ColumnBuilder{T}"/>
    ///     class from an existing <see cref="ColumnConfiguration"/> and a
    ///     projection.
    /// </summary>
    /// <param name="configuration">
    ///     The column configuration to use for the built column.
    /// </param>
    /// <param name="projection">
    ///     The projection that produces the column's values.
    /// </param>
    /// <exception cref="ArgumentNullException">
    ///     <paramref name="configuration"/> or <paramref name="projection"/>
    ///     is <c>null</c>.
    /// </exception>
    public ColumnBuilder(
            ColumnConfiguration configuration,
            IProjection<int, T> projection)
    {
        Guard.NotNull(configuration, nameof(configuration));
        Guard.NotNull(projection, nameof(projection));

        this.Projection = projection;
        this.Configuration = configuration;
    }

    /// <summary>
    ///     Gets the <see cref="ColumnConfiguration"/> that describes the
    ///     column being built.
    /// </summary>
    protected ColumnConfiguration Configuration { get; }

    /// <summary>
    ///     Gets the projection that produces the column's values.
    /// </summary>
    protected IProjection<int, T> Projection { get; }

    /// <summary>
    ///     Gets or sets the delegate used to configure column variants on
    ///     the built column, or <c>null</c> if no variants have been
    ///     configured via <see cref="WithVariants"/>.
    /// </summary>
    protected Func<RootColumnBuilder, ColumnBuilder>? VariantOptions { get; private set; } = null;

    /// <summary>
    ///     Associates the specified <see cref="DataColumnCommands"/> with
    ///     the column being built. Hosts can later retrieve these commands
    ///     from the resulting <see cref="DataColumn{T}"/> via
    ///     <see cref="IDataColumnWithCommands"/>.
    /// </summary>
    /// <param name="commands">
    ///     The commands to attach to the column.
    /// </param>
    /// <returns>
    ///     This <see cref="ColumnBuilder{T}"/> instance, to allow chaining.
    /// </returns>
    public ColumnBuilder<T> WithCommands(
        DataColumnCommands commands)
    {
        this.commands = commands;
        return this;
    }

    /// <summary>
    ///     Configures column variants on the column being built by supplying
    ///     a delegate that further customizes a
    ///     <see cref="RootColumnBuilder"/>.
    /// </summary>
    /// <param name="options">
    ///     A delegate that receives a <see cref="RootColumnBuilder"/> and
    ///     returns the configured <see cref="ColumnBuilder"/> that will be
    ///     used to add variants to the column.
    /// </param>
    /// <returns>
    ///     This <see cref="ColumnBuilder{T}"/> instance, to allow chaining.
    /// </returns>
    public ColumnBuilder<T> WithVariants(
        Func<RootColumnBuilder, ColumnBuilder> options)
    {
        this.VariantOptions = options;
        return this;
    }

    /// <summary>
    ///     Builds the configured <see cref="DataColumn{T}"/> and adds it to
    ///     the specified <paramref name="tableBuilder"/>. If variants have
    ///     been configured via <see cref="WithVariants"/>, the column is
    ///     added with those variants; otherwise it is added as a plain
    ///     column.
    /// </summary>
    /// <param name="tableBuilder">
    ///     The table builder to which the built column is added.
    /// </param>
    /// <returns>
    ///     The <paramref name="tableBuilder"/>, to allow chaining additional
    ///     table-building calls.
    /// </returns>
    internal ITableBuilderWithRowCount AddColumnToTable(
        ITableBuilderWithRowCount tableBuilder)
    {
        DataColumn<T> dataColumn = BuildColumn(this.commands);

        if (this.VariantOptions is not null)
        {
            return tableBuilder.AddColumnWithVariants(dataColumn, this.VariantOptions);
        }

        return tableBuilder.AddColumn(dataColumn);
    }

    /// <summary>
    ///     Constructs the <see cref="DataColumn{T}"/> that this builder
    ///     produces. Derived classes may override this method to return a
    ///     specialized column type.
    /// </summary>
    /// <param name="commands">
    ///     The commands to associate with the built column, or <c>null</c>
    ///     if no commands have been configured.
    /// </param>
    /// <returns>
    ///     A new <see cref="DataColumn{T}"/> configured with this builder's
    ///     <see cref="Configuration"/>, <see cref="Projection"/>, and
    ///     <paramref name="commands"/>.
    /// </returns>
    protected virtual DataColumn<T> BuildColumn(
        DataColumnCommands? commands)
    {
        return new(this.Configuration, this.Projection, commands);
    }
}
