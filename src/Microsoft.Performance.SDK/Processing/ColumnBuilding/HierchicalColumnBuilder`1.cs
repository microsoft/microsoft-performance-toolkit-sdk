// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.

#nullable enable

using Microsoft.Performance.SDK.ColumnCommands;

namespace Microsoft.Performance.SDK.Processing.ColumnBuilding;

public sealed class HierchicalColumnBuilder<T>
    : ColumnBuilder<T>
{
    private readonly ICollectionInfoProvider<T> infoProvider;

    public HierchicalColumnBuilder(
            ColumnMetadata metadata,
            UIHints displayHints,
            IProjection<int, T> projection,
            ICollectionInfoProvider<T> infoProvider)
        : base(metadata, displayHints, projection)
    {
        this.infoProvider = infoProvider;
    }

    protected override DataColumn<T> BuildColumn(DataColumnCommands<T>? commands)
    {
        return new HierarchicalDataColumn<T>(this.Configuration, this.Projection, this.infoProvider, commands);
    }
}