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
    /// <exception cref="ArgumentNullException">
    ///     <paramref name="modeIdentifier"/> or <paramref name="projection"/> is <c>null</c>.
    /// </exception>
    IModalColumnBuilder WithMode<T>(
        ColumnVariantIdentifier modeIdentifier,
        IProjection<int, T> projection);

    /// <summary>
    ///     Adds a hierarchical mode to the column.
    /// </summary>
    /// <param name="modeIdentifier">
    ///     The <see cref="ColumnVariantIdentifier"/> for the mode. The
    ///     <see cref="ColumnVariantIdentifier.Name"/> represents the name of
    ///     this mode.
    /// </param>
    /// <param name="projection">
    ///     The projection that will be used to generate the column for this mode.
    /// </param>
    /// <param name="collectionProvider">
    ///     The collection provider for the column.
    /// </param>
    /// <typeparam name="T">
    ///     The type of data that the projection will produce.
    /// </typeparam>
    /// <returns>
    ///     A new instance of <see cref="IModalColumnBuilder"/> that has been
    ///     configured with the new mode.
    /// </returns>
    /// <exception cref="ArgumentNullException">
    ///     <paramref name="modeIdentifier"/>, <paramref name="projection"/>, or
    ///     <paramref name="collectionProvider"/> is <c>null</c>.
    /// </exception>
    IModalColumnBuilder WithHierarchicalMode<T>(
        ColumnVariantIdentifier modeIdentifier,
        IProjection<int, T> projection,
        ICollectionInfoProvider<T> collectionProvider);

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
    /// <exception cref="ArgumentNullException">
    ///     <paramref name="modeIdentifier"/> or <paramref name="projection"/> is <c>null</c>.
    /// </exception>
    IModalColumnBuilder WithMode<T>(
        ColumnVariantIdentifier modeIdentifier,
        IProjection<int, T> projection,
        Action<IToggleableColumnBuilder> builder);

    /// <summary>
    ///     Adds a hierarchical mode to the column.
    /// </summary>
    /// <param name="modeIdentifier">
    ///     The <see cref="ColumnVariantIdentifier"/> for the mode. The
    ///     <see cref="ColumnVariantIdentifier.Name"/> represents the name of
    ///     this mode.
    /// </param>
    /// <param name="projection">
    ///     The projection that will be used to generate the column for this mode.
    /// </param>
    /// <param name="collectionProvider">
    ///     The collection provider for the column.
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
    /// <exception cref="ArgumentNullException">
    ///     <paramref name="modeIdentifier"/>, <paramref name="projection"/>,
    ///     or <paramref name="collectionProvider"/> is <c>null</c>.
    /// </exception>
    IModalColumnBuilder WithHierarchicalMode<T>(
        ColumnVariantIdentifier modeIdentifier,
        IProjection<int, T> projection,
        ICollectionInfoProvider<T> collectionProvider,
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