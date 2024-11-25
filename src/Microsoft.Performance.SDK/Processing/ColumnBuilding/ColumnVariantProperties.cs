// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.

namespace Microsoft.Performance.SDK.Processing.ColumnBuilding;

/// <summary>
///     Describes the metadata properties of a data column represented by a column variant.
/// </summary>
public record ColumnVariantProperties
{
    private readonly string label;

    /// <summary>
    ///     Gets or initializes the human-readable label of the column variant.
    /// </summary>
    /// <exception cref="System.ArgumentException">
    ///     The supplied value is whitespace.
    /// </exception>
    /// <exception cref="System.ArgumentNullException">
    ///     The supplied value is <c>null</c>.
    /// </exception>
    public required string Label
    {
        get
        {
            return this.label;
        }
        init
        {
            Guard.NotNullOrWhiteSpace(value, nameof(Label));
            this.label = value;
        }
    }

    /// <summary>
    ///     Gets or initializes an optional human-readable description of the column variant.
    /// </summary>
    public string Description { get; init; }

    /// <summary>
    ///     Gets or initializes an optional value to use as the variant's <see cref="ColumnMetadata.Name"/>
    ///     when this variant is active. If this value is not set, the base column's name will be used.
    /// </summary>
    public string ColumnName { get; init; }
}