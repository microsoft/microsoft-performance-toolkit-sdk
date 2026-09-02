// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.

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

    protected override DataColumn<T> BuildColumn()
    {
        return new HierarchicalDataColumn<T>(this.Configuration, this.Projection, this.infoProvider);
    }
}