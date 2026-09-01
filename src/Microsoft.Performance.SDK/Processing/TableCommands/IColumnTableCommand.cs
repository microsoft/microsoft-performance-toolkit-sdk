// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.

using System;

namespace Microsoft.Performance.SDK.Processing.TableCommands
{
    /// <summary>
    ///     Identifies a table command that targets a single, specific column
    ///     within a table.
    /// </summary>
    /// <remarks>
    ///     Hosts may use this interface to filter commands at the column level
    ///     before considering them for execution. Because the target column is
    ///     known statically via <see cref="ColumnId"/>, a host can exclude
    ///     commands that are not relevant to a given column without invoking
    ///     <see cref="TableCommand3{TContext, TResult}.CanExecute"/>.
    /// </remarks>
    public interface IColumnTableCommand
    {
        /// <summary>
        ///     Gets the identifier of the column that this command targets.
        /// </summary>
        Guid ColumnId { get; }
    }
}
