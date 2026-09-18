// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.

#nullable enable

using Microsoft.Performance.SDK.ColumnCommands;
using Microsoft.Performance.SDK.Processing;
using Microsoft.Performance.SDK.Processing.ColumnBuilding;
using System;

namespace Microsoft.Performance.SDK.Runtime.ColumnBuilding.Builders;

internal class ModalVariantBuilder<T>
    : ModalVariantBuilder
{
    private readonly ColumnVariantDescriptor modeDescriptor;
    private readonly IProjection<int, T> projection;
    private readonly ICollectionInfoProvider<T>? collectionProvider;

    private DataColumnCommands? commands = null;
    private Func<ToggleableColumnBuilder, ColumnBuilder>? builder = null;

    public ModalVariantBuilder(
        ColumnVariantDescriptor modeDescriptor,
        IProjection<int, T> projection)
        : this(modeDescriptor, projection, null)
    {
    }

    public ModalVariantBuilder(
        ColumnVariantDescriptor modeDescriptor,
        IProjection<int, T> projection,
        ICollectionInfoProvider<T>? collectionProvider)
    {
        this.modeDescriptor = modeDescriptor;
        this.projection = projection;
        this.collectionProvider = collectionProvider;
    }

    public override ModalVariantBuilder WithCommands(DataColumnCommands commands)
    {
        this.commands = commands;
        return this;
    }

    public override ModalVariantBuilder WithBuilder(Func<ToggleableColumnBuilder, ColumnBuilder> builder)
    {
        this.builder = builder;
        return this;
    }

    internal override ModalVariant CreateVariant(IDataColumn baseColumn)
    {
        if (this.collectionProvider is null)
        {
            return CreateModalVariant(baseColumn);
        }

        return CreateHierarchicalModalVariant(baseColumn);
    }

    private ModalVariant CreateModalVariant(IDataColumn baseColumn)
    {
        ModalVariant newMode = new(
            this.modeDescriptor,
            new DataColumn<T>(
                new ColumnConfiguration(baseColumn.Configuration)
                {
                    Metadata = new ColumnMetadata(baseColumn.Configuration.Metadata) { Name = this.modeDescriptor.Properties.ColumnName ?? baseColumn.Configuration.Metadata.Name },
                },
                this.projection,
                this.commands),
            this.builder);

        return newMode;
    }

    private ModalVariant CreateHierarchicalModalVariant(IDataColumn baseColumn)
    {
        ModalVariant newMode = new(
            this.modeDescriptor,
            new HierarchicalDataColumn<T>(
                new ColumnConfiguration(baseColumn.Configuration)
                {
                    Metadata = new ColumnMetadata(baseColumn.Configuration.Metadata) { Name = this.modeDescriptor.Properties.ColumnName ?? baseColumn.Configuration.Metadata.Name },
                },
                this.projection,
                this.collectionProvider,
                this.commands),
            builder);

        return newMode;
    }
}