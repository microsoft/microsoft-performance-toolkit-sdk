// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.

namespace Microsoft.Performance.SDK.Processing.DataColumns
{
    public interface IDataColumnVariants
    {
        ColumnVariantsType Type { get; }

        IDataColumnVariant[] PossibleVariants { get; }

        IDataColumnVariants SubVariants { get; }
    }
}