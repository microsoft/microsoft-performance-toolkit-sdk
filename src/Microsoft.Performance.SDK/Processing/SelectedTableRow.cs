// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.

using System;
using System.Collections.Generic;

namespace Microsoft.Performance.SDK.Processing;

/// <summary>
///     Represents a row selected by the user at the time a table command is invoked.
///     <para/>
///     A selected row is identified by its top-level <see cref="RowIndex"/> in the table.
///     When the row belongs to (or expands into) an <see cref="IHierarchicalDataColumn{T}"/>,
///     the specific expansion(s) selected within that row are described by
///     <see cref="SubRowIndices"/>. An empty <see cref="SubRowIndices"/> means the
///     top-level row itself is selected, with no sub-row expansion.
/// </summary>
/// <param name="RowIndex">
///     The zero-based index of the selected row in the table.
/// </param>
/// <param name="SubRowIndices">
///     The zero-based indices of the selected sub-rows within the expanded
///     hierarchical column of <see cref="RowIndex"/>. May be empty if the top-level
///     row itself is selected. Never <c>null</c>; a <c>null</c> value passed to the
///     constructor is normalized to an empty list.
/// </param>
public record SelectedTableRow(int RowIndex, IReadOnlyList<int> SubRowIndices)
{
    /// <summary>
    ///     The zero-based indices of the selected sub-rows within the expanded
    ///     hierarchical column of <see cref="RowIndex"/>. Never <c>null</c>.
    /// </summary>
    public IReadOnlyList<int> SubRowIndices { get; init; } = SubRowIndices ?? Array.Empty<int>();

    /// <summary>
    ///     Initializes a new instance of the <see cref="SelectedTableRow"/> record
    ///     representing a selection of the top-level row only, with no sub-row expansion.
    /// </summary>
    /// <param name="rowIndex">
    ///     The zero-based index of the selected row in the table.
    /// </param>
    public SelectedTableRow(int rowIndex)
        : this(rowIndex, Array.Empty<int>())
    {
    }
}
