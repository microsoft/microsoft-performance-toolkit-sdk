// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.

using Microsoft.Performance.SDK.Processing.ColumnBuilding;

namespace Microsoft.Performance.SDK.Processing.DataColumns
{
    public interface IDataColumnVariants
    {
        ColumnVariantsType Type { get; }

        IDataColumnVariant[] PossibleVariants { get; }
    }

    public interface IDataColumnVariant
    {
        ColumnVariantIdentifier Identifier { get; }
        IDataColumnVariants SubVariants { get; }
    }
}