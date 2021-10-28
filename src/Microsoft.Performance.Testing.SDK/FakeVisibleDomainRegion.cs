// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.

using Microsoft.Performance.SDK;
using Microsoft.Performance.SDK.Processing;

namespace Microsoft.Performance.Testing.SDK
{
    public sealed class FakeVisibleDomainRegion
        : IVisibleDomainRegion
    {
        public TimeRange Domain { get; set; }

        public TAggregate AggregateVisibleRows<T, TAggregate>(IProjection<int, T> projection, AggregationMode aggregationMode)
        {
            return default(TAggregate);
        }
    }
}
