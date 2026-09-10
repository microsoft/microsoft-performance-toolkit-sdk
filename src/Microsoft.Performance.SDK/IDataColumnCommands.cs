// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.

#nullable enable

using Microsoft.Performance.SDK.ColumnCommands;

namespace Microsoft.Performance.SDK.Processing;

/// <summary>
///     Implemented by columns (such as <see cref="DataColumn{T}"/> and
///     <see cref="HierarchicalDataColumn{T}"/>) that expose a set of
///     host-invokable commands. Hosts cast a column to this interface
///     to discover the commands the plugin has associated with the
///     column.
/// </summary>
public interface IDataColumnCommands
{
    /// <summary>
    ///     Gets the <see cref="DataColumnCommands"/> associated with
    ///     the column. Never <c>null</c>; a column with no commands
    ///     exposes <see cref="DataColumnCommands.Empty"/>.
    /// </summary>
    DataColumnCommands Commands { get; }
}
