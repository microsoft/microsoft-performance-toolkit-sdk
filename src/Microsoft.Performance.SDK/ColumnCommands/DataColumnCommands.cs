// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.

#nullable enable

using System.Diagnostics.CodeAnalysis;

namespace Microsoft.Performance.SDK.ColumnCommands;

/// <summary>
///     This class exposes commands on a given column.
/// </summary>
/// <remarks>
///     This class works with both <see cref="IDataColumn{T}"/> and <see cref="IHierarchicalDataColumn{T}"/>.
///     Note that with the latter, the column's row value might be different than {T} because of an
///     <see cref="ICollectionAccessProvider{T, TOut}"/> on the column.
/// </remarks>
public sealed class DataColumnCommands
{
    public static readonly DataColumnCommands Empty = new(null);

    private readonly DownloadSourceCodeCommand? downloadSourceCodeCommand;

    public DataColumnCommands(DownloadSourceCodeCommand? downloadSourceCodeCommand)
    {
        this.downloadSourceCodeCommand = downloadSourceCodeCommand;
    }

    public bool TryGetDownloadSourceCodeCommand([NotNullWhen(true)] out DownloadSourceCodeCommand? command)
    {
        command = this.downloadSourceCodeCommand;
        return command is not null;
    }
}
