// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.

using System;

namespace Microsoft.Performance.SDK.Processing.ColumnBuilding;

/// <summary>
///     A builder for columns that can have nested toggled variants.
/// </summary>
public abstract class ToggleableColumnBuilder
    : ColumnBuilder
{
    private protected ToggleableColumnBuilder()
    {
    }

    /// <summary>
    ///     Adds a new toggleable variant to the column. The added toggleable variant
    ///     is nested at the "end" of the chain of toggleable variants already
    ///     added via calls to this method.
    /// </summary>
    /// <param name="toggleDescriptor">
    ///     The <see cref="ColumnVariantDescriptor"/> for the toggle. The
    ///     <see cref="ColumnVariantDescriptor.Name"/> represents the name of the toggled
    ///     on variant.
    /// </param>
    /// <param name="projection">
    ///     The projection that will be used to generate the column when this toggle is on.
    /// </param>
    /// <typeparam name="T">
    ///     The type of data that the projection will produce.
    /// </typeparam>
    /// <returns>
    ///     A new instance of <see cref="ToggleableColumnBuilder"/> that has been
    ///     configured with the added toggle.
    /// </returns>
    /// <exception cref="ArgumentNullException">
    ///     <paramref name="toggleDescriptor"/> or <paramref name="projection"/> is <c>null</c>.
    /// </exception>
    public abstract ToggleableColumnBuilder WithToggle<T>(
        ColumnVariantDescriptor toggleDescriptor,
        IProjection<int, T> projection);

    /// <summary>
    ///     Adds a new toggleable variant to the column. The added toggleable variant
    ///     is nested at the "end" of the chain of toggleable variants already
    ///     added via calls to this method.
    /// </summary>
    /// <param name="toggleDescriptor">
    ///     The <see cref="ColumnVariantDescriptor"/> for the toggle. The
    ///     <see cref="ColumnVariantDescriptor.Name"/> represents the name of the toggled
    ///     on variant.
    /// </param>
    /// <param name="projection">
    ///     The projection that will be used to generate the column when this toggle is on.
    /// </param>
    /// <param name="collectionProvider">
    ///     The collection provider for the column.
    /// </param>
    /// <typeparam name="T">
    ///     The type of data that the projection will produce.
    /// </typeparam>
    /// <returns>
    ///     A new instance of <see cref="ToggleableColumnBuilder"/> that has been
    ///     configured with the added toggle.
    /// </returns>
    /// <exception cref="ArgumentNullException">
    ///     <paramref name="toggleDescriptor"/>, <paramref name="projection"/>,
    ///     or <paramref name="collectionProvider"/> is <c>null</c>.
    /// </exception>
    public abstract ToggleableColumnBuilder WithHierarchicalToggle<T>(
        ColumnVariantDescriptor toggleDescriptor,
        IProjection<int, T> projection,
        ICollectionInfoProvider<T> collectionProvider);

    /// <summary>
    ///     Adds a set of modes to the column that are nested inside of a toggle with no
    ///     associated projection.
    /// </summary>
    /// <param name="toggleText">
    ///     The text to display for the toggle that represents the modes.
    /// </param>
    /// <param name="builder">
    ///     A callback that builds the modes.
    /// </param>
    /// <returns>
    ///     A new instance of <see cref="ColumnBuilder"/> that has been
    ///     configured with the added toggled modes.
    /// </returns>
    /// <exception cref="ArgumentNullException">
    ///     <paramref name="toggleText"/> is <c>null</c>.
    /// </exception>
    public abstract ColumnBuilder WithToggledModes(
        string toggleText,
        Action<ModalColumnBuilder> builder);
}