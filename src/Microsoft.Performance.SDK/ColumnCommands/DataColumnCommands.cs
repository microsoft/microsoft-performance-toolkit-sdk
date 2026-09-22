// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.

#nullable enable

using System.Diagnostics.CodeAnalysis;

namespace Microsoft.Performance.SDK.ColumnCommands;

/// <summary>
///     Exposes the set of host-invokable commands that a plugin has associated
///     with a given column. Plugins construct an instance of this type and
///     attach it to a column (for example via
///     <c>ColumnBuilder&lt;T&gt;.WithCommands</c>) to advertise the operations
///     that a host may perform against the column's row values.
/// </summary>
/// <remarks>
///     This class works with both <see cref="Processing.IDataColumn{T}"/> and
///     <see cref="Processing.IHierarchicalDataColumn{T}"/>. Note that with the latter,
///     the column's row value might be different than <c>T</c> because of an
///     <see cref="Processing.ICollectionAccessProvider{T, TOut}"/> on the column. For
///     this reason, individual command APIs operate on <see cref="object"/>
///     rather than a generic value type.
/// </remarks>
public sealed class DataColumnCommands
{
    /// <summary>
    ///     Gets a shared <see cref="DataColumnCommands"/> instance that
    ///     exposes no commands. Use this when a column has no commands to
    ///     advertise, rather than allocating a new empty instance.
    /// </summary>
    public static readonly DataColumnCommands Empty = new(null);

    private readonly DownloadSourceCodeCommand? downloadSourceCodeCommand;

    /// <summary>
    ///     Initializes a new instance of the <see cref="DataColumnCommands"/>
    ///     class with the specified commands. Pass <c>null</c> for any
    ///     command that is not supported by the column.
    /// </summary>
    /// <param name="downloadSourceCodeCommand">
    ///     The <see cref="DownloadSourceCodeCommand"/> to expose on the
    ///     column, or <c>null</c> if the column does not support downloading
    ///     source code.
    /// </param>
    public DataColumnCommands(DownloadSourceCodeCommand? downloadSourceCodeCommand)
    {
        this.downloadSourceCodeCommand = downloadSourceCodeCommand;
    }

    /// <summary>
    ///     Attempts to get the <see cref="DownloadSourceCodeCommand"/>
    ///     advertised by the column.
    /// </summary>
    /// <param name="command">
    ///     When this method returns <c>true</c>, contains the
    ///     <see cref="DownloadSourceCodeCommand"/> associated with the
    ///     column; otherwise, <c>null</c>.
    /// </param>
    /// <returns>
    ///     <c>true</c> if the column exposes a
    ///     <see cref="DownloadSourceCodeCommand"/>; otherwise, <c>false</c>.
    /// </returns>
    public bool TryGetDownloadSourceCodeCommand([NotNullWhen(true)] out DownloadSourceCodeCommand? command)
    {
        command = this.downloadSourceCodeCommand;
        return command is not null;
    }
}
