// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.

using System;
using System.Collections.Generic;

namespace Microsoft.Performance.SDK.Processing;

/// <summary>
///     Context for the execution of a table command registered via
///     <see cref="ITableBuilder.AddTableCommand3(string, Predicate{TableCommandContext2}, TableCommandCallback2)"/>.
///     <para/>
///     Unlike <see cref="TableCommandContext"/>, the selected rows are described by
///     <see cref="SelectedTableRow"/> values, which also carry any sub-row indices
///     selected within an expanded <see cref="IHierarchicalDataColumn{T}"/>.
/// </summary>
/// <param name="Column">
///     The <see cref="Guid"/> of the column that the command was invoked on.
///     This value may be <c>null</c>.
/// </param>
/// <param name="Configuration">
///     The <see cref="TableConfiguration"/> of the table that was applied at the time
///     the command was invoked. This value may be <c>null</c>.
/// </param>
/// <param name="SelectedRows">
///     The rows that are selected by the user at the time the command is invoked.
/// </param>
public record TableCommandContext2(
    Guid? Column,
    TableConfiguration Configuration,
    IReadOnlyList<SelectedTableRow> SelectedRows);
