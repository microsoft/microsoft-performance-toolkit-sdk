// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.

namespace Microsoft.Performance.SDK.Processing.DataColumns
{
    public interface IDynamicDataColumn
    {
        IDataColumn CurrentColumn { get; }

        IDataColumnVariants Variants { get; }
    }
}