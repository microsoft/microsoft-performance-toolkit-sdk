// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.

using System;
using System.Collections.Generic;

namespace Microsoft.Performance.SDK.Processing
{
    /// <summary>
    ///     Represents a function to be called when the user executes a command
    ///     against a table. For example, in the UI, users can invoke table commands
    ///     by right clicking the table and selecting the command from the context
    ///     menu. See <see cref="ITableBuilder.AddTableCommand(string,TableCommandCallback )"/>.
    /// </summary>
    /// <param name="selectedRows">
    ///     The rows that are selected by the user at the time the command is invoked.
    /// </param>
    public delegate void TableCommandCallback(IReadOnlyList<int> selectedRows);

    public record TableCommandContext(
        TableConfiguration Configuration, // May be null
        Guid? Column, // May be null
        IReadOnlyList<int> SelectedRows);
}
