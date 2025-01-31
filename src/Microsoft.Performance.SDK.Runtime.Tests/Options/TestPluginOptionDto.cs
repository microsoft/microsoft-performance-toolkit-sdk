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
    /// <summary>
    ///     Creates a <see cref="BooleanPluginOptionDto"/> with the given values.
    /// </summary>
    /// <param name="guid">
    ///     The GUID for the option.
    /// </param>
    /// <param name="isDefault">
    ///     Whether the value is the default value.
    /// </param>
    /// <param name="value">
    ///     The value of the option.
    /// </param>
    /// <returns>
    ///     A new <see cref="BooleanPluginOptionDto"/> with the given values.
    /// </returns>
    public static BooleanPluginOptionDto BooleanOptionDto(Guid guid, bool isDefault, bool value)
    {
        return new BooleanPluginOptionDto()
        {
            Guid = guid,
            IsDefault = isDefault,
            Value = value,
        };
    }

    /// <summary>
    ///     Creates a <see cref="FieldPluginOptionDto"/> with the given values.
    /// </summary>
    /// <param name="guid">
    ///     The GUID for the option.
    /// </param>
    /// <param name="isDefault">
    ///     Whether the value is the default value.
    /// </param>
    /// <param name="value">
    ///     The value of the option.
    /// </param>
    /// <returns>
    ///     A new <see cref="FieldPluginOptionDto"/> with the given values.
    /// </returns>
    public static FieldPluginOptionDto FieldOptionDto(Guid guid, bool isDefault, string value)
    {
        return new FieldPluginOptionDto()
        {
            Guid = guid,
            IsDefault = isDefault,
            Value = value,
        };
    }

    /// <summary>
    ///     Creates a <see cref="FieldArrayPluginOptionDto"/> with the given values.
    /// </summary>
    /// <param name="guid">
    ///     The GUID for the option.
    /// </param>
    /// <param name="isDefault">
    ///     Whether the value is the default value.
    /// </param>
    /// <param name="value">
    ///     The value of the option.
    /// </param>
    /// <returns>
    ///     A new <see cref="FieldArrayPluginOptionDto"/> with the given values.
    /// </returns>
    public static FieldArrayPluginOptionDto FieldArrayOptionDto(Guid guid, bool isDefault, string[] value)
    {
        return new FieldArrayPluginOptionDto()
        {
            Guid = guid,
            IsDefault = isDefault,
            Value = value,
        };
    }

    /// <summary>
    ///     Creates a <see cref="PluginOptionsDto"/> with the given values.
    /// </summary>
    /// <param name="dtos">
    ///     The <see cref="PluginOptionDto"/> instances to include in the DTO.
    /// </param>
    /// <returns>
    ///     A new <see cref="PluginOptionsDto"/> with the given values.
    /// </returns>
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