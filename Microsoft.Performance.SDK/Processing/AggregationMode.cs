// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.

namespace Microsoft.Performance.SDK.Processing
{
    /// <summary>
    ///     Defines the method of aggregation for values in a column.
    /// </summary>
    public enum AggregationMode
    {
        /// <summary>
        ///     No aggregation is to be performed.
        /// </summary>
        None,

        /// <summary>
        ///     Average of the values.
        /// </summary>
        Average,

        /// <summary>
        ///     Sum the values.
        /// </summary>
        Sum,

        /// <summary>
        ///     The count of all values.
        /// </summary>
        Count,

        /// <summary>
        ///     Take the minimum value.
        /// </summary>
        Min,

        /// <summary>
        ///     Take the maximum value.
        /// </summary>
        Max,

        /// <summary>
        ///     The count of unique values.
        /// </summary>
        UniqueCount,

        /// <summary>
        ///     The peak values.
        ///     todo: __CDS__
        ///     What is this really?
        /// </summary>
        Peak,

        /// <summary>
        ///     The weighted average of the values.
        /// </summary>
        WeightedAverage
    }
}
