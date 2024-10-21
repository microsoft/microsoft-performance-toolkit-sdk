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

    internal abstract void Commit();
}