// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.

using System;

namespace Microsoft.Performance.SDK.Processing.ColumnBuilding;

/// <summary>
///     Base class for classes that can build/extend upon columns that are
///     part of a table.
/// </summary>
public abstract class ColumnBuilder
{
    private protected ColumnBuilder()
    {
    }

    /// <summary>
    ///     Commits this builder's configuration to the column being built. Note that this method MUST be called to have
    ///     any effect on the column being built. Calling this method multiple times will override previous calls.
    /// </summary>
    /// <exception cref="InvalidOperationException">
    ///     Thrown if multiple column variants are registered were registered the same GUID.
    /// </exception>
    public abstract void Commit();
}