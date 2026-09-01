// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.

using System.Threading;
using System.Threading.Tasks;

namespace Microsoft.Performance.SDK.Processing;

/// <summary>
///     Represents an asynchronous function to be called when the user executes a
///     command against a table registered via
///     <see cref="ITableBuilder.AddTableCommand3(string, System.Predicate{TableCommandContext2}, TableCommandCallback2)"/>.
/// </summary>
/// <param name="context">
///     The <see cref="TableCommandContext2"/> describing the invocation, including the
///     selected rows and sub-rows.
/// </param>
/// <param name="cancellationToken">
///     A <see cref="CancellationToken"/> that the host may signal to request that
///     the command abort.
/// </param>
/// <returns>
///     A <see cref="Task{TResult}"/> that resolves to a <see cref="TableCommandResult"/>
///     describing the outcome of the command. Callbacks that do not need to return a
///     value should return <see cref="VoidTableCommandResult.Instance"/>.
/// </returns>
public delegate Task<TableCommandResult> TableCommandCallback2(
    TableCommandContext2 context,
    CancellationToken cancellationToken);
