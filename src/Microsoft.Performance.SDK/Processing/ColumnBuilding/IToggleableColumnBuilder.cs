// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.

using System;

namespace Microsoft.Performance.SDK.Processing.ColumnBuilding;

/// <summary>
///     A builder for columns that can have nested toggled variants.
/// </summary>
public interface IToggleableColumnBuilder
    : IColumnBuilder
{
    /// <summary>
    ///     Adds a new toggleable variant to the column. The added toggleable variant
    ///     is nested at the "end" of the chain of toggleable variants already
    ///     added via calls to this method.
    /// </summary>
    /// <param name="toggleIdentifier">
    ///     The <see cref="ColumnVariantIdentifier"/> for the toggle. The
    ///     <see cref="ColumnVariantIdentifier.Name"/> represents the name of the toggled
    ///     on variant.
    /// </param>
    /// <param name="projection">
    ///     The projection that will be used to generate the column when this toggle is on.
    /// </param>
    /// <typeparam name="T">
    ///     The type of data that the projection will produce.
    /// </typeparam>
    /// <returns>
    ///     A new instance of <see cref="IToggleableColumnBuilder"/> that has been
    ///     configured with the added toggle.
    /// </returns>
    IToggleableColumnBuilder WithToggle<T>(
        ColumnVariantIdentifier toggleIdentifier,
        IProjection<int, T> projection);

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
    ///     A new instance of <see cref="IColumnBuilder"/> that has been
    ///     configured with the added toggled modes.
    /// </returns>
    IColumnBuilder WithToggledModes(
        string toggleText,
        Action<IModalColumnBuilder> builder);
}