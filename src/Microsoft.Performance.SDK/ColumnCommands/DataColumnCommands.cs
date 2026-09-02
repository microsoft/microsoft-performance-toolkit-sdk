// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.

namespace Microsoft.Performance.SDK.ColumnCommands;

public sealed class DataColumnCommands<T>
{
    internal DataColumnCommands()
    {
    }

    public DownloadSourceCodeCommand<T> DownloadSourceCodeCommand { get; internal set; }
}
