// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.

using Microsoft.Performance.SDK.Processing;
using Microsoft.Performance.SDK.Processing.ColumnBuilding;
using Microsoft.Performance.SDK.Runtime.ColumnBuilding.Builders.CallbackInvokers;
using Microsoft.Performance.SDK.Runtime.ColumnBuilding.Processors;
using Microsoft.Performance.SDK.Runtime.ColumnVariants.TreeNodes;
using System;
using System.Collections.Generic;

namespace Microsoft.Performance.SDK.Runtime.ColumnBuilding.Builders;

/// <summary>
///     A column variants builder that has not been modified in any way.
/// </summary>
public sealed class EmptyColumnBuilder
    : RootColumnBuilder
{
    private readonly IColumnVariantsProcessor processor;
    private readonly IDataColumn baseColumn;

    /// <summary>
    ///     Initializes a new instance of the <see cref="EmptyColumnBuilder"/>
    /// </summary>
    /// <param name="processor">
    ///     The <see cref="IColumnVariantsProcessor" /> to invoke once the column variants are built.
    /// </param>
    /// <param name="baseColumn">
    ///     The base <see cref="IDataColumn" /> that is being built upon.
    /// </param>
    public EmptyColumnBuilder(
        IColumnVariantsProcessor processor,
        IDataColumn baseColumn)
    {
        this.baseColumn = baseColumn;
        this.processor = processor;
    }

    /// <inheritdoc />
    internal override void Commit()
    {
        processor.ProcessColumnVariants(NullColumnVariantsTreeNode.Instance);
    }

    /// <inheritdoc />
    public override ToggleableColumnBuilder WithToggle<T>(
        ColumnVariantDescriptor toggleDescriptor,
        IProjection<int, T> projection)
    {
        Guard.NotNull(toggleDescriptor, nameof(toggleDescriptor));
        Guard.NotNull(projection, nameof(projection));

        return new ToggledColumnBuilder(
            [
                new ToggleableVariant(toggleDescriptor,
                    new DataColumn<T>(
                        new ColumnConfiguration(this.baseColumn.Configuration)
                        {
                            Metadata = new ColumnMetadata(this.baseColumn.Configuration.Metadata) { Name = toggleDescriptor.Properties.ColumnName ?? this.baseColumn.Configuration.Metadata.Name},
                        },
                        projection)),
            ],
            baseColumn,
            processor);
    }

    /// <inheritdoc />
    public override ToggleableColumnBuilder WithHierarchicalToggle<T>(
        ColumnVariantDescriptor toggleDescriptor,
        IProjection<int, T> projection,
        ICollectionInfoProvider<T> collectionProvider)
    {
        Guard.NotNull(toggleDescriptor, nameof(toggleDescriptor));
        Guard.NotNull(projection, nameof(projection));
        Guard.NotNull(collectionProvider, nameof(collectionProvider));

        return new ToggledColumnBuilder(
            [
                new ToggleableVariant(toggleDescriptor,
                    new HierarchicalDataColumn<T>(
                        new ColumnConfiguration(this.baseColumn.Configuration)
                        {
                            Metadata = new ColumnMetadata(this.baseColumn.Configuration.Metadata) { Name = toggleDescriptor.Properties.ColumnName ?? this.baseColumn.Configuration.Metadata.Name},
                        },
                        projection,
                        collectionProvider)),
            ],
            baseColumn,
            processor);
    }

    /// <inheritdoc />
    public override ToggleableColumnBuilder WithToggleableBuilder<T>(
        ColumnVariantDescriptor toggleDescriptor,
        IProjection<int, T> projection,
        Func<ToggleableVariantBuilder, ToggleableVariantBuilder> buildVariant)
    {
        Guard.NotNull(toggleDescriptor, nameof(toggleDescriptor));
        Guard.NotNull(projection, nameof(projection));
        Guard.NotNull(buildVariant, nameof(buildVariant));

        return CreateFromBuilder(toggleDescriptor, projection, null, buildVariant);
    }

    /// <inheritdoc />
    public override ToggleableColumnBuilder WithHierarchicalToggleableBuilder<T>(
        ColumnVariantDescriptor toggleDescriptor,
        IProjection<int, T> projection,
        ICollectionInfoProvider<T> collectionProvider,
        Func<ToggleableVariantBuilder, ToggleableVariantBuilder> buildVariant)
    {
        Guard.NotNull(toggleDescriptor, nameof(toggleDescriptor));
        Guard.NotNull(projection, nameof(projection));
        Guard.NotNull(collectionProvider, nameof(collectionProvider));
        Guard.NotNull(buildVariant, nameof(buildVariant));

        return CreateFromBuilder(toggleDescriptor, projection, collectionProvider, buildVariant);
    }

    /// <inheritdoc />
    public override ColumnBuilder WithToggledModes(
        string toggleText,
        Func<ModalColumnBuilder, ColumnBuilder> builder)
    {
        Guard.NotNull(toggleText, nameof(toggleText));

        return new ToggledColumnWithToggledModesBuilder(
            new List<ToggleableVariant>(),
            baseColumn,
            processor,
            new ModesBuilderCallbackInvoker(builder, baseColumn),
            toggleText);
    }

    /// <inheritdoc />
    public override ModalColumnBuilder WithModes(
        ColumnVariantProperties baseProjectionProperties)
    {
        return WithModes(baseProjectionProperties, null);
    }

    /// <inheritdoc />
    public override ModalColumnBuilder WithModes(
        ColumnVariantProperties baseProjectionProperties,
        Func<ToggleableColumnBuilder, ColumnBuilder> builder)
    {
        Guard.NotNull(baseProjectionProperties, nameof(baseProjectionProperties));

        return new ModalColumnWithModesBuilder(
            processor,
            [
                new ModalVariant(
                    new ColumnVariantDescriptor(baseColumn.Configuration.Metadata.Guid, baseProjectionProperties),
                    baseColumn,
                    builder),
            ],
            baseColumn,
            null);
    }

    private ToggleableColumnBuilder CreateFromBuilder<T>(
        ColumnVariantDescriptor toggleDescriptor,
        IProjection<int, T> projection,
        ICollectionInfoProvider<T> collectionProvider,
        Func<ToggleableVariantBuilder, ToggleableVariantBuilder> buildVariant)
    {
        ToggleableVariantBuilder variantBuilder = new ToggledVariantBuilder<T>(toggleDescriptor, projection, collectionProvider);
        variantBuilder = buildVariant(variantBuilder);

        return new ToggledColumnBuilder(
            [
                variantBuilder.CreateVariant(this.baseColumn),
            ],
            baseColumn,
            processor);
    }
}