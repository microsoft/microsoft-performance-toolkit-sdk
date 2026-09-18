// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.

#nullable enable

using Microsoft.Performance.SDK.ColumnCommands;
using Microsoft.Performance.SDK.Processing;
using Microsoft.Performance.SDK.Processing.ColumnBuilding;

namespace Microsoft.Performance.SDK.Runtime.ColumnBuilding.Builders;

/// <summary>
///     A concrete <see cref="ToggleableVariantBuilder"/> that builds a single toggleable
///     column variant, optionally hierarchical, from a projection of type <typeparamref name="T"/>.
/// </summary>
/// <typeparam name="T">
///     The type of data that the variant's projection produces.
/// </typeparam>
internal sealed class ToggledVariantBuilder<T>
    : ToggleableVariantBuilder
{
    private readonly ColumnVariantDescriptor toggleDescriptor;
    private readonly IProjection<int, T> projection;
    private readonly ICollectionInfoProvider<T>? collectionProvider = null;

    private DataColumnCommands? commands = null;

    /// <summary>
    ///     Initializes a new instance of the <see cref="ToggledVariantBuilder{T}"/> class
    ///     for a non-hierarchical toggleable variant.
    /// </summary>
    /// <param name="toggleDescriptor">
    ///     The <see cref="ColumnVariantDescriptor"/> for the toggle.
    /// </param>
    /// <param name="projection">
    ///     The projection used to generate the column when this toggle is on.
    /// </param>
    public ToggledVariantBuilder(
        ColumnVariantDescriptor toggleDescriptor,
        IProjection<int, T> projection)
    {
        this.toggleDescriptor = toggleDescriptor;
        this.projection = projection;
    }

    /// <summary>
    ///     Initializes a new instance of the <see cref="ToggledVariantBuilder{T}"/> class
    ///     for a hierarchical toggleable variant.
    /// </summary>
    /// <param name="toggleDescriptor">
    ///     The <see cref="ColumnVariantDescriptor"/> for the toggle.
    /// </param>
    /// <param name="projection">
    ///     The projection used to generate the column when this toggle is on.
    /// </param>
    /// <param name="collectionProvider">
    ///     The collection provider used to build a hierarchical column.
    /// </param>
    public ToggledVariantBuilder(
        ColumnVariantDescriptor toggleDescriptor,
        IProjection<int, T> projection,
        ICollectionInfoProvider<T> collectionProvider)
        : this(toggleDescriptor, projection)
    {
        this.collectionProvider = collectionProvider;
    }

    /// <inheritdoc />
    public override ToggleableVariantBuilder WithCommands(DataColumnCommands commands)
    {
        this.commands = commands;
        return this;
    }

    /// <inheritdoc />
    internal override ToggleableVariant CreateVariant(IDataColumn baseColumn)
    {
        if (this.collectionProvider is null)
        {
            return CreateToggleableVariant(baseColumn);
        }

        return CreateHierarchicalToggleableVariant(baseColumn);
    }

    /// <summary>
    ///     Creates a non-hierarchical <see cref="ToggleableVariant"/> backed by a
    ///     <see cref="DataColumn{T}"/>.
    /// </summary>
    /// <param name="baseColumn">
    ///     The base column whose configuration and metadata the variant derives from.
    /// </param>
    /// <returns>
    ///     The created <see cref="ToggleableVariant"/>.
    /// </returns>
    private ToggleableVariant CreateToggleableVariant(IDataColumn baseColumn)
    {
        return new ToggleableVariant(
            this.toggleDescriptor,
            new DataColumn<T>(
                new ColumnConfiguration(baseColumn.Configuration)
                {
                    Metadata = new ColumnMetadata(baseColumn.Configuration.Metadata) { Name = this.toggleDescriptor.Properties.ColumnName ?? baseColumn.Configuration.Metadata.Name },
                },
                this.projection,
                this.commands));
    }

    /// <summary>
    ///     Creates a hierarchical <see cref="ToggleableVariant"/> backed by a
    ///     <see cref="HierarchicalDataColumn{T}"/>.
    /// </summary>
    /// <param name="baseColumn">
    ///     The base column whose configuration and metadata the variant derives from.
    /// </param>
    /// <returns>
    ///     The created <see cref="ToggleableVariant"/>.
    /// </returns>
    private ToggleableVariant CreateHierarchicalToggleableVariant(IDataColumn baseColumn)
    {
        return new ToggleableVariant(
            this.toggleDescriptor,
            new HierarchicalDataColumn<T>(
                new ColumnConfiguration(baseColumn.Configuration)
                {
                    Metadata = new ColumnMetadata(baseColumn.Configuration.Metadata) { Name = this.toggleDescriptor.Properties.ColumnName ?? baseColumn.Configuration.Metadata.Name },
                },
                this.projection,
                this.collectionProvider,
                this.commands));
    }
}