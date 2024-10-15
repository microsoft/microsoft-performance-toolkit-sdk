// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.

using System;

namespace Microsoft.Performance.SDK.Processing.ColumnBuilding;

/// <summary>
///     Represents a identifier for a column variant.
/// </summary>
/// <param name="Guid">
///     The unique identifier for the column variant.
/// </param>
/// <param name="Name">
///     The human-readable name of the column variant.
/// </param>
public record ColumnVariantIdentifier(Guid Guid, string Name);