// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.

using System;
using System.Linq;

namespace Microsoft.Performance.SDK.Runtime.Progress
{
    /// <summary>
    ///     An <see cref="ProgressAggregator{TAggregate,T}"/> that aggregates integer-based percentages (0-100) to a new
    ///     aggregated integer-based percentage. The aggregated percentage reported is defined as the average percentage
    ///     across all children (i.e. SUM(children)/COUNT(children)).
    /// </summary>
    public sealed class PercentageProgressAggregator
        : ProgressAggregator<int, int>
    {
        public PercentageProgressAggregator(
            IProgress<int> aggregateTo)
            : base(aggregateTo,  values => (int)((double)values.Sum() / values.Count()))
        {
        }
    }
}