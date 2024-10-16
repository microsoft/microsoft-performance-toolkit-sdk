// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.

using System;

namespace Microsoft.Performance.SDK.Processing.ColumnBuilding;

/// <summary>
///     A builder for configuring modes on a column that has been
///     configured as having mutually exclusive modes.
/// </summary>
public interface IModalColumnBuilder
    : IColumnBuilder
{
    /// <summary>
    ///     Adds a mode to the column.
    /// </summary>
    /// <param name="modeIdentifier">
    ///     The <see cref="ColumnVariantIdentifier"/> for the mode. The
    ///     <see cref="ColumnVariantIdentifier.Name"/> represents the name of
    ///     this mode.
    /// </param>
    /// <param name="projection">
    ///     The projection that will be used to generate the column for this mode.
    /// </param>
    /// <typeparam name="T">
    ///     The type of data that the projection will produce.
    /// </typeparam>
    /// <returns>
    ///     A new instance of <see cref="IModalColumnBuilder"/> that has been
    ///     configured with the new mode.
    /// </returns>
    IModalColumnBuilder WithMode<T>(
        ColumnVariantIdentifier modeIdentifier,
        IProjection<int, T> projection);

    /// <summary>
    ///     Adds a mode to the column.
    /// </summary>
    /// <param name="modeIdentifier">
    ///     The <see cref="ColumnVariantIdentifier"/> for the mode.
    /// </param>
    /// <param name="projection">
    ///     The projection that will be used to generate the column for this mode.
    /// </param>
    /// <param name="builder">
    ///     A callback that builds sub-variants of the added mode.
    /// </param>
    /// <typeparam name="T">
    ///     The type of data that the projection will produce.
    /// </typeparam>
    /// <returns>
    ///     A new instance of <see cref="IModalColumnBuilder"/> that has been
    ///     configured with the new mode.
    /// </returns>
    IModalColumnBuilder WithMode<T>(
        ColumnVariantIdentifier modeIdentifier,
        IProjection<int, T> projection,
        Action<IToggleableColumnBuilder> builder);

    /// <summary>
    ///     Sets the default mode for the column.
    /// </summary>
    /// <param name="modeIdentifierGuid">
    ///     The <see cref="Guid"/> of the <see cref="ColumnVariantIdentifier"/> that
    ///     identifies the mode to set as the default.
    /// </param>
    /// <returns>
    ///     A new instance of <see cref="IColumnBuilder"/> that has been
    ///     configured with the default mode.
    /// </returns>
    /// <exception cref="ArgumentException">
    ///     The mode with the given <paramref name="modeIdentifierGuid"/> has not been
    ///     added as an available mode.
    /// </exception>
    /// <remarks>
    ///     If this method is not called prior to <see cref="IColumnBuilder.Build"/>,
    ///     the first mode added will be the default mode.
    /// </remarks>
    IColumnBuilder WithDefaultMode(Guid modeIdentifierGuid);
}