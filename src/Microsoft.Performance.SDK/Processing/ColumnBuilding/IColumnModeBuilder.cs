// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.

using System;

namespace Microsoft.Performance.SDK.Processing.ColumnBuilding
{
    public interface IColumnModeBuilder
        : IColumnBuilder
    {
        IColumnModeBuilder WithMode<T>(
            ColumnVariantIdentifier modeIdentifier,
            IProjection<int, T> column);

        IColumnModeBuilder WithMode<T>(
            ColumnVariantIdentifier modeIdentifier,
            IProjection<int, T> column,
            Action<IColumnBuilder> options);
    }
}