// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.

using System;

namespace Microsoft.Performance.SDK.Processing.ColumnBuilding;

/// <summary>
///     A builder for configuring the root column that has been added to a table.
/// </summary>
public abstract class RootColumnBuilder
    : ToggleableColumnBuilder
{
    private protected RootColumnBuilder()
    {
    }

    /// <summary>
    ///     Specifies that the added column should be a modal column that
    ///     has several top-level modes. The base projection/column specified
    ///     by the column being constructed is one of, and the first, available top-level modes.
    /// </summary>
    /// <param name="baseProjectionProperties">
    ///     The metadata properties that the base projection's mode will have.
    /// </param>
    /// <returns>
    ///     A new <see cref="ModalColumnBuilder"/> to continue building
    ///     column modes.
    /// </returns>
    /// <exception cref="ArgumentNullException">
    ///     <paramref name="baseProjectionProperties"/> is <c>null</c>.
    /// </exception>
    public abstract ModalColumnBuilder WithModes(
        ColumnVariantProperties baseProjectionProperties);

    /// <summary>
    ///     Specifies that the added column should be a modal column that
    ///     has several top-level modes. The base projection/column specified
    ///     by the column being constructed is one of, and the first, available top-level modes.
    /// </summary>
    /// <param name="baseProjectionProperties">
    ///     The metadata properties that the base projection's mode will have.
    /// </param>
    /// <param name="builder">
    ///     A callback that builds sub-variants of the added mode represented by the base
    ///     column and returns the final column configuration.
    /// </param>
    /// <returns>
    ///     A new <see cref="ModalColumnBuilder"/> to continue building
    ///     column modes.
    /// </returns>
    /// <exception cref="ArgumentNullException">
    ///     <paramref name="baseProjectionProperties"/> is <c>null</c>.
    /// </exception>
    public abstract ModalColumnBuilder WithModes(
        ColumnVariantProperties baseProjectionProperties,
        Func<ToggleableColumnBuilder, ColumnBuilder> builder);
}