// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.

namespace Microsoft.Performance.SDK.ColumnCommands;

public abstract class DownloadSourceCodeCommand<T>
{
    protected DownloadSourceCodeCommand(string commandName)
    {
        CommandName = commandName;
    }

    public string CommandName { get; }

    public abstract bool CanExecute(Context context);

    public abstract System.Threading.Tasks.Task<DownloadSourceCodeResult> ExecuteAsync(Context context, System.Threading.CancellationToken cancellationToken);

    public record Context(T Value, string DownloadPath);
}
