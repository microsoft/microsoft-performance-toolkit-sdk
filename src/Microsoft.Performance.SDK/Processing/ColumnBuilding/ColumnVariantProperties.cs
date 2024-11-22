// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.

namespace Microsoft.Performance.SDK.Processing.ColumnBuilding;

/// <summary>
///     Describes the metadata properties of a data column represented by a column variant.
/// </summary>
public record ColumnVariantProperties
{
    /// <summary>
    ///     Gets or initializes the human-readable label of the column variant.
    /// </summary>
    public required string Label { get; init; }

    /// <summary>
    ///     Gets or initializes an optional human-readable description of the column variant.
    /// </summary>
    public string Description { get; init; }

    /// <summary>
    ///     Gets or initializes the optional <see cref="ColumnMetadata.Name"/> to associate with the column
    ///     when this variant is active. If this value is not set, the base column's name will be used.
    /// </summary>
    public string ColumnName { get; init; }
}