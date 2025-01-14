// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.

using System;
using System.Collections.Generic;

namespace Microsoft.Performance.SDK.Processing;

/// <summary>
///     Context for the execution of a table command.
/// </summary>
/// <param name="Column">
///     The <see cref="Guid"/> of the column that the command was invoked on.
///     This value may be <c>null</c>.
/// </param>
/// <param name="Configuration">
///     The <see cref="TableConfiguration"/> of the table that was applied at the time the command was invoked.
///     This value may be <c>null</c>.
/// </param>
/// <param name="SelectedRows">
///     The rows that are selected by the user at the time the command is invoked.
/// </param>
public record TableCommandContext(
    Guid? Column, // May be null
    TableConfiguration Configuration, // May be null
    IReadOnlyList<int> SelectedRows);