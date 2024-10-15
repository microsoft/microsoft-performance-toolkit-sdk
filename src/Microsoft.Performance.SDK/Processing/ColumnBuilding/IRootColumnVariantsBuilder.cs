// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.

using System;

namespace Microsoft.Performance.SDK.Processing.ColumnBuilding
{
    public interface IRootColumnVariantsBuilder
        : IToggleableColumnVariantsBuilder
    {
        /// <summary>
        ///     Specifies that the added column should be a modal column that
        ///     has several top-level modes. The base projection/column specified
        ///     by the column being constructed is one of the available top-level modes.
        /// </summary>
        /// <param name="baseProjectionModeName">
        ///     The name of the mode that the base projection for this column
        ///     represents.
        /// </param>
        /// <returns>
        ///     An <see cref="IColumnVariantsModesBuilder"/> to continue building
        ///     column variants.
        /// </returns>
        IColumnVariantsModesBuilder WithModes(
            string baseProjectionModeName);

        /// <summary>
        ///     Specifies that the added column should be a modal column that
        ///     has several top-level modes. The base projection/column specified
        ///     by the column being constructed is one of the available top-level modes.
        /// </summary>
        /// <param name="baseProjectionModeName">
        ///     The name of the mode that the base projection for this column
        ///     represents.
        /// </param>
        /// <param name="builder">
        ///     A callback that builds sub-variants of the added mode.
        /// </param>
        /// <returns>
        ///     An <see cref="IColumnVariantsModesBuilder"/> to continue building
        ///     column variants.
        /// </returns>
        IColumnVariantsModesBuilder WithModes(
            string baseProjectionModeName,
            Action<IToggleableColumnVariantsBuilder> builder);
    }

    public interface IToggleableColumnVariantsBuilder
        : IColumnVariantsBuilder
    {
        IToggleableColumnVariantsBuilder WithToggle<T>(
            ColumnVariantIdentifier toggleIdentifier,
            IProjection<int, T> column);

        IColumnVariantsBuilder WithToggledModes(
            string toggleText,
            Action<IColumnVariantsModesBuilder> builder);
    }
}