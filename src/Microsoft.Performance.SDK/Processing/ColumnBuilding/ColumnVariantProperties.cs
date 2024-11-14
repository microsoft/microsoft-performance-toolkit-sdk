// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.

namespace Microsoft.Performance.SDK.Processing.ColumnBuilding;

/// <summary>
///     Describes the metadata properties of a data column represented by a column variant.
/// </summary>
public record ColumnVariantProperties
{
    /// <summary>
    ///     Gets or initializes the human-readable name of the column variant.
    /// </summary>
    public required string Name { get; init; }

    /// <summary>
    ///     Gets or initializes an optional human-readable description of the column variant.
    /// </summary>
    public string Description { get; init; }
}