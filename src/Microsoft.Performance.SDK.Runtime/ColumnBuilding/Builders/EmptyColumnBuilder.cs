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
///     Represents a column builder that has not been modified in any way.
/// </summary>
public sealed class EmptyColumnBuilder
    : IRootColumnBuilder
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
    public void Commit()
    {
        processor.ProcessColumnVariants(NullColumnVariantsTreeNode.Instance);
    }

    /// <inheritdoc />
    public IToggleableColumnBuilder WithToggle<T>(
        ColumnVariantIdentifier toggleIdentifier,
        IProjection<int, T> projection)
    {
        Guard.NotNull(toggleIdentifier, nameof(toggleIdentifier));
        Guard.NotNull(projection, nameof(projection));

        return new ToggledColumnBuilder(
            [
                new ToggledColumnBuilder.AddedToggle(toggleIdentifier,
                    new DataColumn<T>(baseColumn.Configuration, projection)),
            ],
            baseColumn,
            processor);
    }

    /// <inheritdoc />
    public IToggleableColumnBuilder WithHierarchicalToggle<T>(
        ColumnVariantIdentifier toggleIdentifier,
        IProjection<int, T> projection,
        ICollectionInfoProvider<T> collectionProvider)
    {
        Guard.NotNull(toggleIdentifier, nameof(toggleIdentifier));
        Guard.NotNull(projection, nameof(projection));
        Guard.NotNull(collectionProvider, nameof(collectionProvider));

        return new ToggledColumnBuilder(
            [
                new ToggledColumnBuilder.AddedToggle(toggleIdentifier,
                    new HierarchicalDataColumn<T>(baseColumn.Configuration, projection, collectionProvider)),
            ],
            baseColumn,
            processor);
    }

    /// <inheritdoc />
    public IColumnBuilder WithToggledModes(
        string toggleText,
        Action<IModalColumnBuilder> builder)
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
    public IModalColumnBuilder WithModes(
        string baseProjectionModeName)
    {
        return WithModes(baseProjectionModeName, null);
    }

    /// <inheritdoc />
    public IModalColumnBuilder WithModes(
        string baseProjectionModeName,
        Action<IToggleableColumnBuilder> builder)
    {
        Guard.NotNull(baseProjectionModeName, nameof(baseProjectionModeName));

        return new ModalColumnBuilder(
            processor,
            [
                new ModalColumnBuilder.AddedMode(
                    new ColumnVariantIdentifier(baseColumn.Configuration.Metadata.Guid, baseProjectionModeName),
                    baseColumn,
                    builder),
            ],
            baseColumn,
            null);
    }
}