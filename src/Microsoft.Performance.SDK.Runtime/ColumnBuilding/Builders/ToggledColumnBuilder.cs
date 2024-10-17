// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.

using System;
using System.Collections.Generic;
using System.Linq;
using Microsoft.Performance.SDK.Processing;
using Microsoft.Performance.SDK.Processing.ColumnBuilding;
using Microsoft.Performance.SDK.Runtime.ColumnBuilding.Builders.CallbackInvokers;
using Microsoft.Performance.SDK.Runtime.ColumnBuilding.Processors;
using Microsoft.Performance.SDK.Runtime.ColumnVariants.TreeNodes;

namespace Microsoft.Performance.SDK.Runtime.ColumnBuilding.Builders;

/// <summary>
///     Represents a builder for a column that has at least one toggle.
/// </summary>
internal class ToggledColumnBuilder
    : IToggleableColumnBuilder
{
    internal record AddedToggle(
        ColumnVariantIdentifier ToggleIdentifier,
        IDataColumn column);

    private readonly IReadOnlyCollection<AddedToggle> toggles;
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
        IReadOnlyCollection<AddedToggle> toggles,
        IDataColumn baseColumn,
        IColumnVariantsProcessor processor)
    {
        this.toggles = toggles;
        this.baseColumn = baseColumn;
        this.processor = processor;
    }

    /// <inheritdoc />
    public void Commit()
    {
        this.processor.ProcessColumnVariants(BuildVariant());
    }

    /// <inheritdoc />
    public IToggleableColumnBuilder WithToggle<T>(
        ColumnVariantIdentifier toggleIdentifier,
        IProjection<int, T> projection)
    {
        Guard.NotNull(toggleIdentifier, nameof(toggleIdentifier));
        Guard.NotNull(projection, nameof(projection));

        return new ToggledColumnBuilder(
            this.toggles.Append(
                new AddedToggle(toggleIdentifier, new DataColumn<T>(this.baseColumn.Configuration, projection))
            ).ToList(),
            this.baseColumn,
            this.processor);
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
                new AddedToggle(toggleIdentifier,
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
            variantsTreeNode = new ToggleableColumnVariantsTreeNode(toggle.ToggleIdentifier, toggle.column, variantsTreeNode);
        }

        return variantsTreeNode;
    }

    protected virtual IColumnVariantsTreeNode GetRootVariant()
    {
        return NullColumnVariantsTreeNode.Instance;
    }
}