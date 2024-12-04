// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.

using System;

namespace Microsoft.Performance.SDK.Processing;

/// <summary>
///     Represents a time zone that is of significance to a data source.
/// </summary>
/// <param name="Label">
///     A human-readable label that describes the significance of the time zone.
/// </param>
/// <param name="TimeZone">
///     The time zone.
/// </param>
public sealed record DataSourceTimeZone(string Label, TimeZoneInfo TimeZone);