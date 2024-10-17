// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.

using System;

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
    /// <exception cref="InvalidOperationException">
    ///     Thrown if multiple column variants are registered were registered the same GUID.
    /// </exception>
    void Build();
}