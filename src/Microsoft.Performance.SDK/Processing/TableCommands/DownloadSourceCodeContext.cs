// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.

using System;

namespace Microsoft.Performance.SDK.Processing.TableCommands
{
    /// <summary>
    ///     The context supplied to a <see cref="DownloadSourceCodeCommand"/> command
    ///     identifying the specific cell (and optional sub-row) for which
    ///     source code should be downloaded.
    /// </summary>
    public sealed class DownloadSourceCodeContext : ITableCommand3Context
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
        /// <param name="columnVariantId">
        ///     The optional identifier of the active column variant whose
        ///     value the source code is being requested for. This corresponds
        ///     to <see cref="Microsoft.Performance.SDK.Processing.ColumnBuilding.ColumnVariantDescriptor.Guid"/>
        ///     and is unique within the column identified by
        ///     <paramref name="columnId"/>. Use <c>null</c> when the base
        ///     column's projection is the active one (i.e., no variant is
        ///     selected).
        /// </param>
        /// <param name="downloadPath">
        ///     The path under which the source code file should be
        ///     downloaded. The implementation may place the file directly
        ///     under this path, or create additional sub-folders beneath it
        ///     as needed.
        /// </param>
        /// <exception cref="ArgumentNullException">
        ///     <paramref name="downloadPath"/> is <c>null</c>.
        /// </exception>
        /// <exception cref="ArgumentException">
        ///     <paramref name="downloadPath"/> is empty or consists only of
        ///     white-space characters.
        /// </exception>
        /// <exception cref="ArgumentOutOfRangeException">
        ///     <paramref name="rowIndex"/> is negative, or
        ///     <paramref name="subRowIndex"/> has a value that is negative.
        /// </exception>
        public DownloadSourceCodeContext(Guid columnId, string downloadPath, int rowIndex, int? subRowIndex = null, Guid? columnVariantId = null)
        {
            if (rowIndex < 0)
            {
                throw new ArgumentOutOfRangeException(nameof(rowIndex), rowIndex, "Row index must be non-negative.");
            }

            if (subRowIndex.HasValue && subRowIndex.Value < 0)
            {
                throw new ArgumentOutOfRangeException(nameof(subRowIndex), subRowIndex, "Sub-row index must be non-negative when specified.");
            }

            if (downloadPath == null)
            {
                throw new ArgumentNullException(nameof(downloadPath));
            }

            if (string.IsNullOrWhiteSpace(downloadPath))
            {
                throw new ArgumentException("Download path must not be empty or white-space.", nameof(downloadPath));
            }

            this.ColumnId = columnId;
            this.RowIndex = rowIndex;
            this.SubRowIndex = subRowIndex;
            this.ColumnVariantId = columnVariantId;
            this.DownloadPath = downloadPath;
        }

        /// <summary>
        ///     Gets the identifier of the column containing the value for
        ///     which source code is being requested.
        /// </summary>
        public Guid ColumnId { get; }

        /// <summary>
        ///     Gets the optional identifier of the active column variant
        ///     whose value the source code is being requested for, or
        ///     <c>null</c> when the base column's projection is the active
        ///     one. This corresponds to
        ///     <see cref="Microsoft.Performance.SDK.Processing.ColumnBuilding.ColumnVariantDescriptor.Guid"/>
        ///     and is unique within <see cref="ColumnId"/>.
        /// </summary>
        public Guid? ColumnVariantId { get; }

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

        /// <summary>
        ///     Gets the path under which the source code file should be
        ///     downloaded. The implementation is free to place the file
        ///     directly under this path or to create additional sub-folders
        ///     beneath it as part of the download.
        /// </summary>
        public string DownloadPath { get; }
    }
}
