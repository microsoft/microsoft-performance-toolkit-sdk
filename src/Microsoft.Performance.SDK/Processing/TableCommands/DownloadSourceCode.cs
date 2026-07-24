// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.

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
    ///     the command name and the logic for
    ///     <see cref="TableCommand3{TContext, TResult}.CanExecute"/> and
    ///     <see cref="TableCommand3{TContext, TResult}.Execute"/>.
    /// </remarks>
    public abstract class DownloadSourceCode
        : TableCommand3<DownloadSourceCodeContext, OpenUriResult>
    {
        /// <summary>
        ///     Initializes a new instance of the
        ///     <see cref="DownloadSourceCode"/> class.
        /// </summary>
        /// <param name="commandName">
        ///     The human-readable name of the command.
        /// </param>
        protected DownloadSourceCode(string commandName)
            : base(commandName)
        {
        }
    }
}
