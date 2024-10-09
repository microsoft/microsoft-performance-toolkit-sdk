// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.

using System;

namespace Microsoft.Performance.SDK.Processing.ColumnBuilding
{
    public interface IColumnBuilder
    {
        IColumnBuilder WithToggle<T>(
            ColumnVariantIdentifier toggleIdentifier,
            IProjection<int, T> column);

        IColumnBuilder WithToggle<T>(
            ColumnVariantIdentifier toggleIdentifier,
            IProjection<int, T> column,
            Action<IColumnToggleBuilder> options);
    }
}