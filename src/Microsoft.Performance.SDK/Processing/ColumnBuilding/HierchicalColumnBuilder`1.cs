// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.

#nullable enable

using Microsoft.Performance.SDK.ColumnCommands;

namespace Microsoft.Performance.SDK.Processing.ColumnBuilding;

/// <summary>
///     Builds a <see cref="HierarchicalDataColumn{T}"/> and adds it to an
///     <see cref="ITableBuilderWithRowCount"/>. Extends
///     <see cref="ColumnBuilder{T}"/> with the
///     <see cref="ICollectionInfoProvider{T}"/> needed to describe the
///     hierarchical structure of the column's values.
/// </summary>
/// <typeparam name="T">
///     The type of data produced by the column's projection.
/// </typeparam>
public sealed class HierchicalColumnBuilder<T>
    : ColumnBuilder<T>
{
    private readonly ICollectionInfoProvider<T> infoProvider;

    /// <summary>
    ///     Initializes a new instance of the
    ///     <see cref="HierchicalColumnBuilder{T}"/> class from the specified
    ///     metadata, display hints, projection, and collection info
    ///     provider.
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
    /// <param name="infoProvider">
    ///     The collection info provider that describes the hierarchical
    ///     structure of the column's values.
    /// </param>
    public HierchicalColumnBuilder(
            ColumnMetadata metadata,
            UIHints displayHints,
            IProjection<int, T> projection,
            ICollectionInfoProvider<T> infoProvider)
        : base(metadata, displayHints, projection)
    {
        this.infoProvider = infoProvider;
    }

    /// <summary>
    ///     Initializes a new instance of the
    ///     <see cref="HierchicalColumnBuilder{T}"/> class from an existing
    ///     <see cref="ColumnConfiguration"/>, projection, and collection
    ///     info provider.
    /// </summary>
    /// <param name="columnConfiguration">
    ///     The column configuration to use for the built column.
    /// </param>
    /// <param name="projection">
    ///     The projection that produces the column's values.
    /// </param>
    /// <param name="infoProvider">
    ///     The collection info provider that describes the hierarchical
    ///     structure of the column's values.
    /// </param>
    public HierchicalColumnBuilder(
            ColumnConfiguration columnConfiguration,
            IProjection<int, T> projection,
            ICollectionInfoProvider<T> infoProvider)
        : base(columnConfiguration, projection)
    {
        this.infoProvider = infoProvider;
    }

    /// <summary>
    ///     Constructs a <see cref="HierarchicalDataColumn{T}"/> using this
    ///     builder's configuration, projection, collection info provider,
    ///     and the supplied <paramref name="commands"/>.
    /// </summary>
    /// <param name="commands">
    ///     The commands to associate with the built column, or <c>null</c>
    ///     if no commands have been configured.
    /// </param>
    /// <returns>
    ///     A new <see cref="HierarchicalDataColumn{T}"/>.
    /// </returns>
    protected override DataColumn<T> BuildColumn(DataColumnCommands? commands)
    {
        return new HierarchicalDataColumn<T>(this.Configuration, this.Projection, this.infoProvider, commands);
    }
}