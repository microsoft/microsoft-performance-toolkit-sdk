// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.

using Microsoft.Performance.SDK.Processing;
using Microsoft.Performance.SDK.Processing.ColumnBuilding;
using Microsoft.Performance.SDK.Runtime.ColumnBuilding.Builders.CallbackInvokers;
using Microsoft.Performance.SDK.Runtime.ColumnBuilding.Processors;
using Microsoft.Performance.SDK.Runtime.ColumnVariants.TreeNodes;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;

namespace Microsoft.Performance.SDK.Runtime.ColumnBuilding.Builders;

/// <summary>
///     A column variants builder for zero or more mutually exclusive modes.
/// </summary>
internal class ModalColumnWithModesBuilder
    : ModalColumnBuilder
{
    private readonly IDataColumn baseColumn;
    private readonly IColumnVariantsProcessor processor;
    private readonly List<ModalVariant> addedModes;
    private readonly int? defaultModeIndex;

    /// <summary>
    ///     Initializes a new instance of the <see cref="ModalColumnWithModesBuilder"/>
    /// </summary>
    /// <param name="processor">
    ///     The <see cref="IColumnVariantsProcessor" /> to invoke once the column variants are built.
    /// </param>
    /// <param name="addedModes">
    ///     The modes that have been so far added to the column.
    /// </param>
    /// <param name="baseColumn">
    ///     The base <see cref="IDataColumn" /> that is being built upon.
    /// </param>
    /// <param name="defaultModeIndex">
    ///     The index within <paramref name="addedModes"/> of the default mode.
    /// </param>
    public ModalColumnWithModesBuilder(
        IColumnVariantsProcessor processor,
        List<ModalVariant> addedModes,
        IDataColumn baseColumn,
        int? defaultModeIndex)
    {
        this.processor = processor;
        this.addedModes = addedModes;
        this.baseColumn = baseColumn;
        this.defaultModeIndex = defaultModeIndex;
    }

    /// <inheritdoc />
    internal override void Commit()
    {
        if (this.addedModes.Count == 0)
        {
            this.processor.ProcessColumnVariants(NullColumnVariantsTreeNode.Instance);
            return;
        }

        List<ModeColumnVariantsTreeNode> modeVariants = new();
        foreach (ModalVariant mode in this.addedModes)
        {
            var callbackInvoker = new ModeBuilderCallbackInvoker(mode.Builder, this.baseColumn);

            IColumnVariantsTreeNode subVariantsTreeNode = NullColumnVariantsTreeNode.Instance;
            if (callbackInvoker.TryGet(out var builtVariant))
            {
                subVariantsTreeNode = builtVariant;
            }
            modeVariants.Add(new ModeColumnVariantsTreeNode(mode.Descriptor, mode.Column, subVariantsTreeNode));
        }

        var variant = new ModesColumnVariantsTreeNode(modeVariants, this.defaultModeIndex ?? 0);
        this.processor.ProcessColumnVariants(variant);
    }

    /// <inheritdoc />
    public override ModalColumnBuilder WithMode<T>(
        ColumnVariantDescriptor modeDescriptor,
        IProjection<int, T> projection)
    {
        return WithMode(modeDescriptor, projection, null);
    }

    /// <inheritdoc />
    public override ModalColumnBuilder WithMode<T>(
        ColumnVariantDescriptor modeDescriptor,
        IProjection<int, T> projection,
        Func<ToggleableColumnBuilder, ColumnBuilder> builder)
    {
        Guard.NotNull(modeDescriptor, nameof(modeDescriptor));
        Guard.NotNull(projection, nameof(projection));

        ModalVariant newMode = new(
            modeDescriptor,
            new DataColumn<T>(
                new ColumnConfiguration(this.baseColumn.Configuration)
                {
                    Metadata = new ColumnMetadata(this.baseColumn.Configuration.Metadata) { Name = modeDescriptor.Properties.ColumnName ?? this.baseColumn.Configuration.Metadata.Name },
                },
                projection),
            builder);

        return WithMode(newMode);
    }

    /// <inheritdoc/>
    public override ModalColumnBuilder WithHierarchicalMode<T>(
        ColumnVariantDescriptor modeDescriptor,
        IProjection<int, T> projection,
        ICollectionInfoProvider<T> collectionProvider)
    {
        return WithHierarchicalMode(modeDescriptor, projection, collectionProvider, null);
    }

    /// <inheritdoc/>
    public override ModalColumnBuilder WithHierarchicalMode<T>(
        ColumnVariantDescriptor modeDescriptor,
        IProjection<int, T> projection,
        ICollectionInfoProvider<T> collectionProvider,
        Func<ToggleableColumnBuilder, ColumnBuilder> builder)
    {
        Guard.NotNull(modeDescriptor, nameof(modeDescriptor));
        Guard.NotNull(projection, nameof(projection));
        Guard.NotNull(collectionProvider, nameof(collectionProvider));

        ModalVariant newMode = new(
            modeDescriptor,
            new HierarchicalDataColumn<T>(
                new ColumnConfiguration(this.baseColumn.Configuration)
                {
                    Metadata = new ColumnMetadata(this.baseColumn.Configuration.Metadata) { Name = modeDescriptor.Properties.ColumnName ?? this.baseColumn.Configuration.Metadata.Name },
                },
                projection,
                collectionProvider),
            builder);

        return WithMode(newMode);
    }

    public override ModalColumnBuilder WithModalBuilder<T>(
        ColumnVariantDescriptor modeDescriptor,
        IProjection<int, T> projection,
        Func<ModalVariantBuilder, ModalVariantBuilder> buildVariant)
    {
        Guard.NotNull(modeDescriptor, nameof(modeDescriptor));
        Guard.NotNull(projection, nameof(projection));
        Guard.NotNull(buildVariant, nameof(buildVariant));

        ModalVariantBuilder variantBuilder = new ModalVariantBuilder<T>(modeDescriptor, projection);
        variantBuilder = buildVariant(variantBuilder);

        return WithMode(variantBuilder.CreateVariant(this.baseColumn));
    }

    public override ModalColumnBuilder WithHierarchicalModalBuilder<T>(
        ColumnVariantDescriptor modeDescriptor,
        IProjection<int, T> projection,
        ICollectionInfoProvider<T> collectionProvider,
        Func<ModalVariantBuilder, ModalVariantBuilder> buildVariant)
    {
        Guard.NotNull(modeDescriptor, nameof(modeDescriptor));
        Guard.NotNull(projection, nameof(projection));
        Guard.NotNull(collectionProvider, nameof(collectionProvider));
        Guard.NotNull(buildVariant, nameof(buildVariant));

        ModalVariantBuilder variantBuilder = new ModalVariantBuilder<T>(modeDescriptor, projection, collectionProvider);
        variantBuilder = buildVariant(variantBuilder);

        return WithMode(variantBuilder.CreateVariant(this.baseColumn));
    }

    /// <inheritdoc />
    public override ColumnBuilder WithDefaultMode(Guid modeIdentifierGuid)
    {
        Debug.Assert(!this.defaultModeIndex.HasValue);

        if (this.defaultModeIndex.HasValue)
        {
            throw new InvalidOperationException("Cannot have more than one default mode.");
        }

        int? index = null;
        for (int i = 0; i < this.addedModes.Count; i++)
        {
            if (this.addedModes[i].Descriptor.Guid == modeIdentifierGuid)
            {
                index = i;
                break;
            }
        }

        if (!index.HasValue)
        {
            throw new ArgumentException($"No mode with identifier {modeIdentifierGuid} has been added.");
        }

        return new ModalColumnWithModesBuilder(
            this.processor,
            this.addedModes,
            this.baseColumn,
            index);
    }

    private ModalColumnBuilder WithMode(ModalVariant newMode)
    {
        return new ModalColumnWithModesBuilder(
            this.processor,
            this.addedModes.Append(newMode).ToList(),
            this.baseColumn,
            this.defaultModeIndex);
    }
}
