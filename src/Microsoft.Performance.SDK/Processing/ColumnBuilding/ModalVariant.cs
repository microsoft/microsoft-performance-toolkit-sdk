// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.

#nullable enable

using System;

namespace Microsoft.Performance.SDK.Processing.ColumnBuilding;

internal record ModalVariant(
    ColumnVariantDescriptor Descriptor,
    IDataColumn Column,
    Func<ToggleableColumnBuilder, ColumnBuilder>? Builder);
