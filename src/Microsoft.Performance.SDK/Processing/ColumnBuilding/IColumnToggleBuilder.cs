// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.

namespace Microsoft.Performance.SDK.Processing.ColumnBuilding
{
    public interface IColumnToggleBuilder
        : IColumnBuilder
    {
        IColumnModeBuilder AsMode(string modeName);
    }
}