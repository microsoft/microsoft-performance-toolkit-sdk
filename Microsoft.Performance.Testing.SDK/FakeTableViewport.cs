// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.

using Microsoft.Performance.SDK;
using Microsoft.Performance.SDK.Processing;

namespace Microsoft.Performance.Testing.SDK
{
    public sealed class FakeTableViewport
        : IVisibleTableRegion
    {
        public int TableRowStart { get; set; }

        public int TableRowCount { get; set; }

        public TimeRange Viewport { get; set; }

        public TAggregate AggregateRowsInViewport<T, TAggregate>(IProjection<int, T> projection, AggregationMode aggregationMode)
        {
            return default(TAggregate);
        }
    }
}
