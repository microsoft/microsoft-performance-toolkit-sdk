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
///     A column variants builder with at least one hierarchical toggle.
/// </summary>
internal class ToggledColumnBuilder
    : ToggleableColumnBuilder
{
    internal record AddedToggle(
        ColumnVariantDescriptor ToggleDescriptor,
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
    public override void Commit()
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
                new AddedToggle(toggleDescriptor, new DataColumn<T>(this.baseColumn.Configuration, projection))
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
            [
                new AddedToggle(toggleDescriptor,
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
            variantsTreeNode = new ToggleableColumnVariantsTreeNode(toggle.ToggleDescriptor, toggle.column, variantsTreeNode);
        }

        return variantsTreeNode;
    }

    protected virtual IColumnVariantsTreeNode GetRootVariant()
    {
        return NullColumnVariantsTreeNode.Instance;
    }
}