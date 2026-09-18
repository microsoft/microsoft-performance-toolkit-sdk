// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.

#nullable enable

namespace Microsoft.Performance.SDK.Processing.ColumnBuilding;

internal record ToggleableVariant(
    ColumnVariantDescriptor ToggleDescriptor,
    IDataColumn Column);
