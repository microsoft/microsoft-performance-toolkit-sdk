// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.

using Microsoft.Performance.SDK.Processing;
using Microsoft.Performance.SDK.Processing.ColumnBuilding;
using Microsoft.Performance.SDK.Runtime.ColumnBuilding.Builders.CallbackInvokers;
using Microsoft.Performance.SDK.Runtime.ColumnBuilding.Processors;
using Microsoft.Performance.SDK.Runtime.ColumnVariants.TreeNodes;
using System;
using System.Collections.Generic;
using System.Linq;

namespace Microsoft.Performance.SDK.Runtime.ColumnBuilding.Builders;

/// <summary>
///     A column variants builder with at least one hierarchical toggle.
/// </summary>
internal class ToggledColumnBuilder
    : ToggleableColumnBuilder
{

    private readonly IReadOnlyCollection<ToggleableVariant> toggles;
    private readonly IDataColumn baseColumn;
    private readonly IColumnVariantsProcessor processor;

    /// <summary>
    ///     Initializes a new instance of the <see cref="ToggledColumnBuilder"/>
    /// </summary>
    /// <param name="toggles">
    ///     The toggles that have been so far added to the column.
    /// </param>
    /// <param name="baseColumn">
    ///     The base <see cref="IDataColumn" /> that is being built upon.
    /// </param>
    /// <param name="processor">
    ///     The <see cref="IColumnVariantsProcessor" /> to invoke once the column variants are built.
    /// </param>
    public ToggledColumnBuilder(
        IReadOnlyCollection<ToggleableVariant> toggles,
        IDataColumn baseColumn,
        IColumnVariantsProcessor processor)
    {
        this.toggles = toggles;
        this.baseColumn = baseColumn;
        this.processor = processor;
    }

    /// <inheritdoc />
    internal override void Commit()
    {
        this.processor.ProcessColumnVariants(BuildVariant());
    }

    /// <inheritdoc />
    public override ToggleableColumnBuilder WithToggle<T>(
        ColumnVariantDescriptor toggleDescriptor,
        IProjection<int, T> projection)
    {
        Guard.NotNull(toggleDescriptor, nameof(toggleDescriptor));
        Guard.NotNull(projection, nameof(projection));

        return new ToggledColumnBuilder(
            this.toggles.Append(
                new ToggleableVariant(
                    toggleDescriptor,
                    new DataColumn<T>(
                        new ColumnConfiguration(this.baseColumn.Configuration)
                        {
                            Metadata = new ColumnMetadata(this.baseColumn.Configuration.Metadata) { Name = toggleDescriptor.Properties.ColumnName ?? this.baseColumn.Configuration.Metadata.Name },
                        },
                        projection))
            ).ToList(),
            this.baseColumn,
            this.processor);
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
            this.toggles.Append(
                new ToggleableVariant(
                    toggleDescriptor,
                    new HierarchicalDataColumn<T>(
                        new ColumnConfiguration(this.baseColumn.Configuration)
                        {
                            Metadata = new ColumnMetadata(this.baseColumn.Configuration.Metadata) { Name = toggleDescriptor.Properties.ColumnName ?? this.baseColumn.Configuration.Metadata.Name },
                        },
                        projection,
                        collectionProvider))
                ).ToList(),
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

        return CreateFromBuilder(toggleDescriptor, projection, collectionProvider, buildVariant);
    }

    /// <inheritdoc />
    public override ColumnBuilder WithToggledModes(
        string toggleText,
        Func<ModalColumnBuilder, ColumnBuilder> builder)
    {
        Guard.NotNull(toggleText, nameof(toggleText));

        return new ToggledColumnWithToggledModesBuilder(
            this.toggles,
            this.baseColumn,
            this.processor,
            new ModesBuilderCallbackInvoker(builder, this.baseColumn),
            toggleText);
    }

    private IColumnVariantsTreeNode BuildVariant()
    {
        IColumnVariantsTreeNode variantsTreeNode = GetRootVariant();

        foreach (var toggle in this.toggles.Reverse())
        {
            variantsTreeNode = new ToggleableColumnVariantsTreeNode(toggle.ToggleDescriptor, toggle.Column, variantsTreeNode);
        }

        return variantsTreeNode;
    }

    protected virtual IColumnVariantsTreeNode GetRootVariant()
    {
        return NullColumnVariantsTreeNode.Instance;
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
