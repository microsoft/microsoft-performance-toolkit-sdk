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
    internal RootColumnBuilder()
    {
    }

    /// <summary>
    ///     Specifies that the added column should be a modal column that
    ///     has several top-level modes. The base projection/column specified
    ///     by the column being constructed is one of, and the first, available top-level modes.
    /// </summary>
    /// <param name="baseProjectionModeName">
    ///     The name of the mode that the base projection for this column
    ///     represents.
    /// </param>
    /// <returns>
    ///     A new <see cref="ModalColumnBuilder"/> to continue building
    ///     column modes.
    /// </returns>
    /// <exception cref="ArgumentNullException">
    ///     <paramref name="baseProjectionModeName"/> is <c>null</c>.
    /// </exception>
    public abstract ModalColumnBuilder WithModes(
        string baseProjectionModeName);

    /// <summary>
    ///     Specifies that the added column should be a modal column that
    ///     has several top-level modes. The base projection/column specified
    ///     by the column being constructed is one of, and the first, available top-level modes.
    /// </summary>
    /// <param name="baseProjectionModeName">
    ///     The name of the mode that the base projection for this column
    ///     represents.
    /// </param>
    /// <param name="builder">
    ///     A callback that builds sub-variants of the added mode represented by the base
    ///     column.
    /// </param>
    /// <returns>
    ///     A new <see cref="ModalColumnBuilder"/> to continue building
    ///     column modes.
    /// </returns>
    /// <exception cref="ArgumentNullException">
    ///     <paramref name="baseProjectionModeName"/> is <c>null</c>.
    /// </exception>
    public abstract ModalColumnBuilder WithModes(
        string baseProjectionModeName,
        Action<ToggleableColumnBuilder> builder);
}