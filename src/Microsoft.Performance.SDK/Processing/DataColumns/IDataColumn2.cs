// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.

namespace Microsoft.Performance.SDK.Processing.DataColumns
{
    public interface IDataColumn2
        : IDataColumn
    {
        IDataColumnVariants Variants { get; }
    }
}