// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.

using System;
using System.Linq;
using Microsoft.Performance.SDK.Runtime.Options.Serialization.DTO;

namespace Microsoft.Performance.SDK.Runtime.Tests.Options;

internal static class TestPluginOptionDtos
{
    public static BooleanPluginOptionDto CreateBooleanOptionDto(Guid guid, bool isDefault, bool value)
    {
        return new BooleanPluginOptionDto()
        {
            Guid = guid,
            IsDefault = isDefault,
            Value = value,
        };
    }

    public static FieldPluginOptionDto CreateFieldOptionDto(Guid guid, bool isDefault, string value)
    {
        return new FieldPluginOptionDto()
        {
            Guid = guid,
            IsDefault = isDefault,
            Value = value,
        };
    }

    public static PluginOptionsDto CreateDto(params PluginOptionDto[] dtos)
    {
        return new PluginOptionsDto
        {
            BooleanOptions = dtos.OfType<BooleanPluginOptionDto>().ToArray(),
            FieldOptions = dtos.OfType<FieldPluginOptionDto>().ToArray(),
            FieldArrayOptions = dtos.OfType<FieldArrayPluginOptionDto>().ToArray(),
        };
    }
}