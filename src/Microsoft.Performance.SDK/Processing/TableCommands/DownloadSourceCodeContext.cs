// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.

using System;

namespace Microsoft.Performance.SDK.Processing.TableCommands
{
    /// <summary>
    ///     The context supplied to a <see cref="DownloadSourceCode"/> command
    ///     identifying the specific cell (and optional sub-row) for which
    ///     source code should be downloaded.
    /// </summary>
    public sealed class DownloadSourceCodeContext
    {
        /// <summary>
        ///     Initializes a new instance of the
        ///     <see cref="DownloadSourceCodeContext"/> class.
        /// </summary>
        /// <param name="columnId">
        ///     The identifier of the column containing the value for which
        ///     source code is being requested.
        /// </param>
        /// <param name="rowIndex">
        ///     The zero-based index of the row containing the value.
        /// </param>
        /// <param name="subRowIndex">
        ///     The optional zero-based index of the sub-row within
        ///     <paramref name="rowIndex"/>, when the row projects multiple
        ///     values. Use <c>null</c> when the row does not have sub-rows or
        ///     when no specific sub-row is being targeted.
        /// </param>
        /// <exception cref="ArgumentOutOfRangeException">
        ///     <paramref name="rowIndex"/> is negative, or
        ///     <paramref name="subRowIndex"/> has a value that is negative.
        /// </exception>
        public DownloadSourceCodeContext(Guid columnId, int rowIndex, int? subRowIndex = null)
        {
            if (rowIndex < 0)
            {
                throw new ArgumentOutOfRangeException(nameof(rowIndex), rowIndex, "Row index must be non-negative.");
            }

            if (subRowIndex.HasValue && subRowIndex.Value < 0)
            {
                throw new ArgumentOutOfRangeException(nameof(subRowIndex), subRowIndex, "Sub-row index must be non-negative when specified.");
            }

            this.ColumnId = columnId;
            this.RowIndex = rowIndex;
            this.SubRowIndex = subRowIndex;
        }

        /// <summary>
        ///     Gets the identifier of the column containing the value for
        ///     which source code is being requested.
        /// </summary>
        public Guid ColumnId { get; }

        /// <summary>
        ///     Gets the zero-based index of the row containing the value.
        /// </summary>
        public int RowIndex { get; }

        /// <summary>
        ///     Gets the optional zero-based index of the sub-row within
        ///     <see cref="RowIndex"/>, or <c>null</c> when no specific
        ///     sub-row is being targeted.
        /// </summary>
        public int? SubRowIndex { get; }
    }
}
