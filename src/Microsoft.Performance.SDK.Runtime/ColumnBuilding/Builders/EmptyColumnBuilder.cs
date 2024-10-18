// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.

using System;
using System.Collections.Generic;
using Microsoft.Performance.SDK.Processing;
using Microsoft.Performance.SDK.Processing.ColumnBuilding;
using Microsoft.Performance.SDK.Runtime.ColumnBuilding.Builders.CallbackInvokers;
using Microsoft.Performance.SDK.Runtime.ColumnBuilding.Processors;
using Microsoft.Performance.SDK.Runtime.ColumnVariants.TreeNodes;

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
    public override void Commit()
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
                new ToggledColumnBuilder.AddedToggle(toggleDescriptor,
                    new DataColumn<T>(baseColumn.Configuration, projection)),
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
                new ToggledColumnBuilder.AddedToggle(toggleDescriptor,
                    new HierarchicalDataColumn<T>(baseColumn.Configuration, projection, collectionProvider)),
            ],
            baseColumn,
            processor);
    }

    /// <inheritdoc />
    public override ColumnBuilder WithToggledModes(
        string toggleText,
        Action<ModalColumnBuilder> builder)
    {
        Guard.NotNull(toggleText, nameof(toggleText));

        return new ToggledColumnWithToggledModesBuilder(
            new List<ToggledColumnBuilder.AddedToggle>(),
            baseColumn,
            processor,
            new ModesBuilderCallbackInvoker(builder, baseColumn),
            toggleText);
    }

    /// <inheritdoc />
    public override ModalColumnBuilder WithModes(
        string baseProjectionModeName)
    {
        return WithModes(baseProjectionModeName, null);
    }

    /// <inheritdoc />
    public override ModalColumnBuilder WithModes(
        string baseProjectionModeName,
        Action<ToggleableColumnBuilder> builder)
    {
        Guard.NotNull(baseProjectionModeName, nameof(baseProjectionModeName));

        return new ModalColumnWithModesBuilder(
            processor,
            [
                new ModalColumnWithModesBuilder.AddedMode(
                    new ColumnVariantDescriptor(baseColumn.Configuration.Metadata.Guid, baseProjectionModeName),
                    baseColumn,
                    builder),
            ],
            baseColumn,
            null);
    }
}