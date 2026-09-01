// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.

using System;
using Microsoft.Performance.SDK.Processing;

namespace Microsoft.Performance.SDK.Runtime;

/// <summary>
///     Represents an asynchronous, result-returning command that can be executed
///     against a table. See
///     <see cref="ITableBuilder.AddTableCommand3(string, Predicate{TableCommandContext2}, TableCommandCallback2)"/>.
/// </summary>
/// <param name="CommandName">
///     The name of the command to be shown to the user.
/// </param>
/// <param name="CanExecute">
///     A <see cref="Predicate{T}"/> that determines if the command can be executed.
/// </param>
/// <param name="OnExecute">
///     The asynchronous function to execute when the command is invoked.
/// </param>
public sealed record TableCommand3(
    string CommandName,
    Predicate<TableCommandContext2> CanExecute,
    TableCommandCallback2 OnExecute);
