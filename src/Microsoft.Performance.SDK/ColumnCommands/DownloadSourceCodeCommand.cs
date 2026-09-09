// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.

namespace Microsoft.Performance.SDK.ColumnCommands;

public abstract class DownloadSourceCodeCommand
{
    protected DownloadSourceCodeCommand(string commandName)
    {
        CommandName = commandName;
    }

    public string CommandName { get; }

    public abstract bool CanExecute(object value, string downloadPath);

    public abstract System.Threading.Tasks.Task<DownloadSourceCodeResult> ExecuteAsync(
        object value,
        string downloadPath,
        System.Threading.CancellationToken cancellationToken);
}
