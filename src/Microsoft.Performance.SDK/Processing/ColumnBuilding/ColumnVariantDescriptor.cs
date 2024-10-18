// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.

using System;

namespace Microsoft.Performance.SDK.Processing.ColumnBuilding;

/// <summary>
///     Describes a column variant, where a column variant is defined as an alternative
///     <see cref="IProjection{TSource,TResult}"/> of an <see cref="IDataColumn"/> that has
///     been added to a table.
/// </summary>
/// <param name="Guid">
///     The unique identifier for the column variant. This value must be unique within a single column's
///     set of variants, but may be reused across columns.
/// </param>
/// <param name="Name">
///     The human-readable name of the column variant.
/// </param>
/// <param name="Description">
///     An optional human-readable description of the column variant. This value may be <c>null</c>.
/// </param>
public record ColumnVariantDescriptor(
    Guid Guid,
    string Name,
    string Description = null);