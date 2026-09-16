// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.

namespace Microsoft.Performance.SDK.ColumnCommands;

/// <summary>
///     Represents a command that a plugin advertises on a column to download
///     the source code associated with a given row value. Hosts discover this
///     command via <see cref="DataColumnCommands.TryGetDownloadSourceCodeCommand"/>
///     and invoke <see cref="ExecuteAsync"/> to obtain the results of the
///     source code download attempts.
/// </summary>
/// <remarks>
///     Implementations must be safe to call from a host on an arbitrary
///     thread. The <c>value</c> passed to <see cref="CanExecute"/> and
///     <see cref="ExecuteAsync"/> is the row value produced by the column,
///     which for hierarchical columns may not be the same type as the
///     column's declared projection type.
/// </remarks>
public abstract class DownloadSourceCodeCommand
{
    /// <summary>
    ///     Initializes a new instance of the
    ///     <see cref="DownloadSourceCodeCommand"/> class with a display name
    ///     that hosts may surface to users (for example, on a context-menu
    ///     item).
    /// </summary>
    /// <param name="commandName">
    ///     A human-readable name for this command.
    /// </param>
    protected DownloadSourceCodeCommand(string commandName)
    {
        CommandName = commandName;
    }

    /// <summary>
    ///     Gets the human-readable name of this command. Hosts may display
    ///     this value in UI when offering the command to the user.
    /// </summary>
    public string CommandName { get; }

    /// <summary>
    ///     Determines whether this command can be executed for the specified
    ///     row value and download path. Hosts should call this before
    ///     surfacing the command to the user, and skip or disable the
    ///     command when this method returns <c>false</c>.
    /// </summary>
    /// <param name="value">
    ///     The row value from the column for which the command may be
    ///     executed.
    /// </param>
    /// <param name="downloadPath">
    ///     The local path under which the source code would be downloaded
    ///     if the command were executed.
    /// </param>
    /// <returns>
    ///     <c>true</c> if <see cref="ExecuteAsync"/> may be called with the
    ///     given arguments; otherwise, <c>false</c>.
    /// </returns>
    public abstract bool CanExecute(object value, string downloadPath);

    /// <summary>
    ///     Asynchronously downloads the source code files associated with the
    ///     specified row value to the specified location.
    /// </summary>
    /// <param name="value">
    ///     The row value from the column for which source code should be
    ///     downloaded.
    /// </param>
    /// <param name="downloadPath">
    ///     The local path under which the source code should be
    ///     downloaded. Implementations decide the exact file layout beneath
    ///     this path and return the resulting URIs via the
    ///     <see cref="DownloadSourceCodeResult.Uri"/> properties.
    /// </param>
    /// <param name="cancellationToken">
    ///     A token that may be used to cancel the download operation.
    /// </param>
    /// <returns>
    ///     A task that produces one <see cref="DownloadSourceCodeResult"/> for
    ///     each attempted source code download. The returned array may contain
    ///     both successful and failed results. Hosts should process each result
    ///     independently, opening the <see cref="DownloadSourceCodeResult.Uri"/>
    ///     of each successful result and surfacing the
    ///     <see cref="DownloadSourceCodeResult.ErrorMessage"/> of each failed
    ///     result.
    /// </returns>
    public abstract System.Threading.Tasks.Task<DownloadSourceCodeResult[]> ExecuteAsync(
        object value,
        string downloadPath,
        System.Threading.CancellationToken cancellationToken);
}
