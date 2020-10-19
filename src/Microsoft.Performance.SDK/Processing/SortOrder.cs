// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.

namespace Microsoft.Performance.SDK.Processing
{
    /// <summary>
    ///     Enumerates the different ways to sort a column.
    /// </summary>
    public enum SortOrder
    {
        /// <summary>
        ///     Do not sort.
        /// </summary>
        None,

        /// <summary>
        ///     Sort values in ascending order.
        /// </summary>
        Ascending,

        /// <summary>
        ///     Sort values in descending order.
        /// </summary>
        Descending,

        /// <summary>
        ///     Sort values in ascending order based on
        ///     the absolute values.
        /// </summary>
        Ascending_Abs,

        /// <summary>
        ///     Sort values in descending order based on
        ///     the absolute values.
        /// </summary>
        Descending_Abs
    }
}
