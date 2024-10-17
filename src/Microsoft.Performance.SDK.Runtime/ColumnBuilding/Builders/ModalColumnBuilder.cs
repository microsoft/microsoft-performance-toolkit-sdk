// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.

using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using Microsoft.Performance.SDK.Processing;
using Microsoft.Performance.SDK.Processing.ColumnBuilding;
using Microsoft.Performance.SDK.Runtime.ColumnBuilding.Builders.CallbackInvokers;
using Microsoft.Performance.SDK.Runtime.ColumnBuilding.Processors;
using Microsoft.Performance.SDK.Runtime.ColumnVariants.TreeNodes;

namespace Microsoft.Performance.SDK.Runtime.ColumnBuilding.Builders;

/// <summary>
///     Represents a builder for a column that has at least one mode.
/// </summary>
internal class ModalColumnBuilder
    : IModalColumnBuilder
{
    private readonly IDataColumn baseColumn;
    private readonly IColumnVariantsProcessor processor;
    private readonly List<AddedMode> addedModes;
    private readonly int? defaultModeIndex;

    internal record AddedMode(
        ColumnVariantIdentifier Identifier,
        IDataColumn column,
        Action<IToggleableColumnBuilder> builder);

    /// <summary>
    ///     Initializes a new instance of the <see cref="ModalColumnBuilder"/>
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
    public ModalColumnBuilder(
        IColumnVariantsProcessor processor,
        List<AddedMode> addedModes,
        IDataColumn baseColumn,
        int? defaultModeIndex)
    {
        this.processor = processor;
        this.addedModes = addedModes;
        this.baseColumn = baseColumn;
        this.defaultModeIndex = defaultModeIndex;
    }

    /// <inheritdoc />
    public void Commit()
    {
        if (this.addedModes.Count == 0)
        {
            this.processor.ProcessColumnVariants(NullColumnVariantsTreeNode.Instance);
            return;
        }

        List<ModeColumnVariantsTreeNode> modeVariants = new();
        foreach (AddedMode mode in this.addedModes)
        {
            var callbackInvoker = new ModeBuilderCallbackInvoker(mode.builder, this.baseColumn);

            IColumnVariantsTreeNode subVariantsTreeNode = NullColumnVariantsTreeNode.Instance;
            if (callbackInvoker.TryGet(out var builtVariant))
            {
                subVariantsTreeNode = builtVariant;
            }
            modeVariants.Add(new ModeColumnVariantsTreeNode(mode.Identifier, mode.column, subVariantsTreeNode));
        }

        var variant = new ModesColumnVariantsTreeNode(modeVariants, this.defaultModeIndex ?? 0);
        this.processor.ProcessColumnVariants(variant);
    }

    /// <inheritdoc />
    public IModalColumnBuilder WithMode<T>(
        ColumnVariantIdentifier modeIdentifier,
        IProjection<int, T> projection)
    {
        return WithMode(modeIdentifier, projection, null);
    }

    /// <inheritdoc />
    public IModalColumnBuilder WithMode<T>(
        ColumnVariantIdentifier modeIdentifier,
        IProjection<int, T> projection,
        Action<IToggleableColumnBuilder> builder)
    {
        Guard.NotNull(modeIdentifier, nameof(modeIdentifier));
        Guard.NotNull(projection, nameof(projection));

        AddedMode newMode = new(
            modeIdentifier,
            new DataColumn<T>(this.baseColumn.Configuration, projection),
            builder);

        return WithMode(newMode);
    }

    public IModalColumnBuilder WithHierarchicalMode<T>(
        ColumnVariantIdentifier modeIdentifier,
        IProjection<int, T> projection,
        ICollectionInfoProvider<T> collectionProvider)
    {
        return WithHierarchicalMode(modeIdentifier, projection, collectionProvider, null);
    }

    public IModalColumnBuilder WithHierarchicalMode<T>(
        ColumnVariantIdentifier modeIdentifier,
        IProjection<int, T> projection,
        ICollectionInfoProvider<T> collectionProvider,
        Action<IToggleableColumnBuilder> builder)
    {
        Guard.NotNull(modeIdentifier, nameof(modeIdentifier));
        Guard.NotNull(projection, nameof(projection));
        Guard.NotNull(collectionProvider, nameof(collectionProvider));

        AddedMode newMode = new(
            modeIdentifier,
            new HierarchicalDataColumn<T>(this.baseColumn.Configuration, projection, collectionProvider),
            builder);

        return WithMode(newMode);
    }

    /// <inheritdoc />
    public IColumnBuilder WithDefaultMode(Guid modeIdentifierGuid)
    {
        Debug.Assert(!this.defaultModeIndex.HasValue);

        if (this.defaultModeIndex.HasValue)
        {
            throw new InvalidOperationException("Cannot have more than one default mode.");
        }

        int? index = null;
        for (int i = 0; i < this.addedModes.Count; i++)
        {
            if (this.addedModes[i].Identifier.Guid == modeIdentifierGuid)
            {
                index = i;
                break;
            }
        }

        if (!index.HasValue)
        {
            throw new ArgumentException($"No mode with identifier {modeIdentifierGuid} has been added.");
        }

        return new ModalColumnBuilder(
            this.processor,
            this.addedModes,
            this.baseColumn,
            index);
    }

    private IModalColumnBuilder WithMode(AddedMode newMode)
    {
        return new ModalColumnBuilder(
            this.processor,
            this.addedModes.Append(newMode).ToList(),
            this.baseColumn,
            this.defaultModeIndex);
    }
}