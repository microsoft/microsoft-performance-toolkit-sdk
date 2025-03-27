// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.

using System;
using Microsoft.Performance.SDK.Options.Definitions;
using Microsoft.Performance.SDK.Runtime.Options;
using Microsoft.Performance.Testing;

namespace Microsoft.Performance.SDK.Tests.Options;

/// <summary>
///     Contains helper methods for creating <see cref="PluginOptionDefinition"/> instances during testing.
/// </summary>
public static class TestPluginOptionDefinition
{
    /// <summary>
    ///     Creates a new <see cref="BooleanOptionDefinition"/> with a given value for testing.
    /// </summary>
    /// <param name="defaultValue">
    ///     The default value for the option.
    /// </param>
    /// <param name="guid">
    ///     The GUID for the option. If <c>null</c>, a new GUID will be generated.
    /// </param>
    /// <returns>
    ///     A new <see cref="BooleanOptionDefinition"/> with the given value.
    /// </returns>
    public static BooleanOptionDefinition BooleanOptionDefinition(bool defaultValue, Guid? guid = null)
    {
        return new BooleanOptionDefinition
        {
            Guid = guid ?? Guid.NewGuid(),
            Category = Utilities.RandomString(5),
            Name = Utilities.RandomString(5),
            Description = Utilities.RandomString(5),
            DefaultValue = defaultValue,
        };
    }

    /// <summary>
    ///     Creates a new <see cref="FieldOptionDefinition"/> with a given value for testing.
    /// </summary>
    /// <param name="defaultValue">
    ///     The default value for the option.
    /// </param>
    /// <param name="guid">
    ///     The GUID for the option. If <c>null</c>, a new GUID will be generated.
    /// </param>
    /// <returns>
    ///     A new <see cref="FieldOptionDefinition"/> with the given value.
    /// </returns>
    public static FieldOptionDefinition FieldOptionDefinition(string defaultValue, Guid? guid = null)
    {
        return new FieldOptionDefinition
        {
            Guid = guid ?? Guid.NewGuid(),
            Category = Utilities.RandomString(5),
            Name = Utilities.RandomString(5),
            Description = Utilities.RandomString(5),
            DefaultValue = defaultValue,
        };
    }

    /// <summary>
    ///     Creates a new <see cref="FieldArrayOptionDefinition"/> with a given value for testing.
    /// </summary>
    /// <param name="defaultValue">
    ///     The default value for the option.
    /// </param>
    /// <param name="guid">
    ///     The GUID for the option. If <c>null</c>, a new GUID will be generated.
    /// </param>
    /// <returns>
    ///     A new <see cref="FieldArrayOptionDefinition"/> with the given value.
    /// </returns>
    public static FieldArrayOptionDefinition FieldArrayOptionDefinition(string[] defaultValue, Guid? guid = null)
    {
        return new FieldArrayOptionDefinition
        {
            Guid = guid ?? Guid.NewGuid(),
            Category = Utilities.RandomString(5),
            Name = Utilities.RandomString(5),
            Description = Utilities.RandomString(5),
            DefaultValue = defaultValue,
        };
    }
}
