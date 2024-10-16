// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.

using Microsoft.Performance.SDK.Processing;
using Microsoft.Performance.SDK.Processing.ColumnBuilding;
using Microsoft.Performance.SDK.Runtime.ColumnVariants.TreeNodes;

namespace Microsoft.Performance.SDK.Runtime.ColumnVariants.Registrar;

public interface IColumnVariantsRegistrar
{
    bool TryGetVariantsTreeRoot(
        IDataColumn column,
        out IColumnVariantsTreeNode variantsTreeNodes);

    bool TryGetVariant(
        IDataColumn baseColumn,
        ColumnVariantIdentifier variantIdentifier,
        out IDataColumn foundVariant);
}