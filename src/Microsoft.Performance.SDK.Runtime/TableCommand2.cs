// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.

using System;
using Microsoft.Performance.SDK.Processing;

namespace Microsoft.Performance.SDK.Runtime;

/// <summary>
///     Represents a command that can be executed against a table.
/// </summary>
/// <param name="MenuName">
///     The name of the command to be shown to the user.
/// </param>
/// <param name="CanExecute">
///     A <see cref="Predicate{T}"/> that determines if the command can be executed.
/// </param>
/// <param name="OnExecute">
///     The function to execute when the command is invoked.
/// </param>
public sealed record TableCommand2(
    string MenuName,
    Predicate<TableCommandContext> CanExecute,
    Action<TableCommandContext> OnExecute);