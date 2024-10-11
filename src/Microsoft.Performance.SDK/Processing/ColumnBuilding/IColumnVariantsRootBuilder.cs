// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.

using System;

namespace Microsoft.Performance.SDK.Processing.ColumnBuilding
{
    public interface IColumnVariantsRootBuilder
        : IColumnVariantsBuilder
    {
        IColumnVariantsRootBuilder WithToggle<T>(
            ColumnVariantIdentifier toggleIdentifier,
            IProjection<int, T> column);

        IColumnVariantsBuilder WithModes(
            Action<IColumnVariantsModesBuilder> builder);
    }
}