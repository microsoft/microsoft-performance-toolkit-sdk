// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.

using System;

namespace Microsoft.Performance.SDK.Processing
{
    /// <summary>
    ///     Contains static (Shared in Visual Basic) methods for interacting
    ///     with <see cref="SortOrder"/> values.
    /// </summary>
    public static class SortOrderExtensions
    {
        /// <summary>
        ///     Given a sort order, returns the sort order that is the
        ///     logical reverse.
        /// </summary>
        /// <param name="sortOrder">
        ///     The sort order to examine.
        /// </param>
        /// <returns>
        ///     The logical reverse of the given sort order.
        /// </returns>
        public static SortOrder Reverse(this SortOrder sortOrder)
        {
            switch (sortOrder)
            {
                case SortOrder.None:
                    return SortOrder.None;

                case SortOrder.Ascending:
                    return SortOrder.Descending;

                case SortOrder.Descending:
                    return SortOrder.Ascending;

                case SortOrder.Ascending_Abs:
                    return SortOrder.Descending_Abs;

                case SortOrder.Descending_Abs:
                    return SortOrder.Ascending_Abs;

                default:
                    throw new ArgumentException("Unknown SortOrder", nameof(sortOrder));
            }
        }

        /// <summary>
        ///     Toggles whether a given sort order is sensitive to absolute
        ///     value.
        /// </summary>
        /// <param name="sortOrder">
        ///     The sort order to interrogate.
        /// </param>
        /// <returns>
        ///     The new sort order.
        /// </returns>
        public static SortOrder SwitchAbs(this SortOrder sortOrder)
        {
            switch (sortOrder)
            {
                case SortOrder.None:
                    return SortOrder.None;

                case SortOrder.Ascending:
                    return SortOrder.Ascending_Abs;

                case SortOrder.Descending:
                    return SortOrder.Descending_Abs;

                case SortOrder.Ascending_Abs:
                    return SortOrder.Ascending;

                case SortOrder.Descending_Abs:
                    return SortOrder.Descending;

                default:
                    throw new ArgumentException("Unknown SortOrder", nameof(sortOrder));
            }
        }
    }
}
