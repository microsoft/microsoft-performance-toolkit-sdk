// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.

using System;

namespace Microsoft.Performance.SDK.Processing.TableCommands
{
    /// <summary>
    ///     A table command that, given a
    ///     <see cref="DownloadSourceCodeContext"/> identifying a specific
    ///     cell (column, row, and optional sub-row), produces an
    ///     <see cref="OpenUriResult"/> whose <see cref="OpenUriResult.Uri"/>
    ///     points to source code that can be downloaded for the value at that
    ///     location.
    /// </summary>
    /// <remarks>
    ///     This class fixes the context and result shape for
    ///     "download source code" commands. Concrete implementations supply
    ///     the command name, the <see cref="ColumnId"/> of the column the
    ///     command targets, and the logic for
    ///     <see cref="TableCommand3{TContext, TResult}.CanExecute"/> and
    ///     <see cref="TableCommand3{TContext, TResult}.ExecuteAsync"/>.
    ///     Because this command implements <see cref="IColumnTableCommand"/>,
    ///     hosts may filter it out at the column level without invoking
    ///     <see cref="TableCommand3{TContext, TResult}.CanExecute"/>.
    /// </remarks>
    public abstract class DownloadSourceCodeCommand
        : TableCommand3<DownloadSourceCodeContext, OpenUriResult>,
          IColumnTableCommand
    {
        /// <summary>
        ///     Initializes a new instance of the
        ///     <see cref="DownloadSourceCodeCommand"/> class.
        /// </summary>
        /// <param name="commandName">
        ///     The human-readable name of the command.
        /// </param>
        /// <param name="columnId">
        ///     The identifier of the column that this command targets.
        /// </param>
        protected DownloadSourceCodeCommand(string commandName, Guid columnId)
            : base(commandName)
        {
            this.ColumnId = columnId;
        }

        /// <inheritdoc/>
        public Guid ColumnId { get; }
    }
}
