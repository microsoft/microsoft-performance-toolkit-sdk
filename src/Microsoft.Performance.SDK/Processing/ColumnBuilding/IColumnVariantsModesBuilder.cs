// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.

using System;

namespace Microsoft.Performance.SDK.Processing.ColumnBuilding
{
    public interface IColumnVariantsModesBuilder
        : IColumnVariantsBuilder
    {
        IColumnVariantsModesBuilder WithMode<T>(
            ColumnVariantIdentifier modeIdentifier,
            IProjection<int, T> column);

        IColumnVariantsModesBuilder WithMode<T>(
            ColumnVariantIdentifier modeIdentifier,
            IProjection<int, T> column,
            Action<IToggleableColumnVariantsBuilder> builder);

        // If not called, the first mode added will be the default mode
        IColumnVariantsBuilder WithDefaultMode(Guid modeIdentifierGuid);
    }
}