// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.

namespace Microsoft.Performance.SDK.Processing
{
    /// <summary>
    ///     Defines the different aggregations that can be performed over time.
    /// </summary>
    public enum AggregationOverTime
    {
        //
        // TODO: the documentation of these values needs to have additional detail
        //       added.
        //

        /// <summary>
        ///     Aggregates the current values.
        /// </summary>
        Current,

        /// <summary>
        ///     Aggregates the rate of the values.
        /// </summary>
        Rate,

        /// <summary>
        ///     Performs a cumulative aggregation.
        /// </summary>
        Cumulative,

        /// <summary>
        ///     Performs an outstanding aggregation.
        /// </summary>
        Outstanding,

        /// <summary>
        ///     Performs an aggregation of the peak.
        /// </summary>
        OutstandingPeak,
    }
}
