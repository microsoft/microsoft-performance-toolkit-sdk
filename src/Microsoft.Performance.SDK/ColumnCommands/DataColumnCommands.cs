// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.

#nullable enable

using System;
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
    public static readonly DataColumnCommands Empty = new(new EmptyCommandsImpl());

    private readonly DataColumnCommandsImpl commands;

    private DataColumnCommands(DataColumnCommandsImpl commands)
    {
        this.commands = commands;
    }

    public bool TryGetDownloadSourceCodeCommand<T>([NotNullWhen(true)] out DownloadSourceCodeCommand<T>? command)
    {
        return this.commands.TryGetDownloadSourceCodeCommand(out command);
    }

    public static DataColumnCommands Create(object? downloadSourceCommand)
    {
        if (downloadSourceCommand is null)
        {
            return Empty;
        }

        var commandsType = typeof(DataColumnCommandsImpl<>).MakeGenericType(downloadSourceCommand.GetType());
        return (DataColumnCommands)Activator.CreateInstance(commandsType, [downloadSourceCommand]);
    }

    private abstract class DataColumnCommandsImpl
    {
        public abstract bool TryGetDownloadSourceCodeCommand<T>([NotNullWhen(true)] out DownloadSourceCodeCommand<T>? command);
    }

    private sealed class EmptyCommandsImpl
        : DataColumnCommandsImpl
    {
        public override bool TryGetDownloadSourceCodeCommand<T>([NotNullWhen(true)] out DownloadSourceCodeCommand<T>? command)
        {
            command = null;
            return false;
        }
    }

    private sealed class DataColumnCommandsImpl<TDownloadSource>
        : DataColumnCommandsImpl
    {
        DownloadSourceCodeCommand<TDownloadSource>? downloadSourceCommand = null;

        public DataColumnCommandsImpl(DownloadSourceCodeCommand<TDownloadSource>? downloadSourceCommand)
        {
            this.downloadSourceCommand = downloadSourceCommand;
        }

        public override bool TryGetDownloadSourceCodeCommand<T>([NotNullWhen(true)] out DownloadSourceCodeCommand<T>? command)
        {
            command = this.downloadSourceCommand as DownloadSourceCodeCommand<T>;
            return command is not null;
        }
    }
}
