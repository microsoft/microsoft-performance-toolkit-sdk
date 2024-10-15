// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.

namespace Microsoft.Performance.SDK.Processing.ColumnBuilding;

/// <summary>
///     Base interface for classes that can build/extend upon columns that are
///     part of a table.
/// </summary>
public interface IColumnBuilder
{
    /// <summary>
    ///     Builds the column. Note that this method MUST be called to have
    ///     any effect on the column being built. Calling this method multiple
    ///     times will result in only the final call having any effect.
    /// </summary>
    void Build();
}