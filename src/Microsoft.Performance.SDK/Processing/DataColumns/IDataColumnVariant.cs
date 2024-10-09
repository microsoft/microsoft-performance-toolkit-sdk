// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.

using Microsoft.Performance.SDK.Processing.ColumnBuilding;

namespace Microsoft.Performance.SDK.Processing.DataColumns
{
    public interface IDataColumnVariant
    {
        ColumnVariantIdentifier Identifier { get; }

        bool CanUnapply { get; }

        bool IsApplied { get; }

        void TryApply();

        void TryUnapply();
    }
}