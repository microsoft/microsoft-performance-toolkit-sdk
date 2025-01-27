// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.

using System;
using System.Linq;
using Microsoft.Performance.SDK.Runtime.Options.Serialization.DTO;

namespace Microsoft.Performance.SDK.Runtime.Tests.Options;

/// <summary>
///     Contains helper methods for creating <see cref="PluginOptionDto"/> instances during testing.
/// </summary>
internal static class TestPluginOptionDto
{
    public static BooleanPluginOptionDto BooleanOptionDto(Guid guid, bool isDefault, bool value)
    {
        return new BooleanPluginOptionDto()
        {
            Guid = guid,
            IsDefault = isDefault,
            Value = value,
        };
    }

    public static FieldPluginOptionDto FieldOptionDto(Guid guid, bool isDefault, string value)
    {
        return new FieldPluginOptionDto()
        {
            Guid = guid,
            IsDefault = isDefault,
            Value = value,
        };
    }

    public static FieldArrayPluginOptionDto FieldArrayOptionDto(Guid guid, bool isDefault, string[] value)
    {
        return new FieldArrayPluginOptionDto()
        {
            Guid = guid,
            IsDefault = isDefault,
            Value = value,
        };
    }

    public static PluginOptionsDto PluginOptionsDto(params PluginOptionDto[] dtos)
    {
        return new PluginOptionsDto
        {
            BooleanOptions = dtos.OfType<BooleanPluginOptionDto>().ToArray(),
            FieldOptions = dtos.OfType<FieldPluginOptionDto>().ToArray(),
            FieldArrayOptions = dtos.OfType<FieldArrayPluginOptionDto>().ToArray(),
        };
    }
}