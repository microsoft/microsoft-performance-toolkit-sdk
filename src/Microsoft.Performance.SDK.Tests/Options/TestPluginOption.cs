// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.

using System;
using Microsoft.Performance.SDK.Options.Definitions;
using Microsoft.Performance.SDK.Options.Values;
using Microsoft.Performance.SDK.Runtime.Options;
using Microsoft.Performance.Testing;

namespace Microsoft.Performance.SDK.Tests.Options;

/// <summary>
///     Contains helper methods for creating <see cref="PluginOption"/> instances during testing.
/// </summary>
public static class TestPluginOption
{
    /// <summary>
    ///     Creates a new <see cref="BooleanOption"/> with a given value for testing.
    /// </summary>
    /// <param name="defaultValue">
    ///     The default value for the option.
    /// </param>
    /// <param name="guid">
    ///     The GUID for the option. If <c>null</c>, a new GUID will be generated.
    /// </param>
    /// <returns>
    ///     A new <see cref="BooleanOption"/> with the given value.
    /// </returns>
    public static BooleanOption BooleanOption(bool defaultValue, Guid? guid = null)
    {
        return new BooleanOption(
            new BooleanOptionDefinition
            {
                Guid = guid ?? Guid.NewGuid(),
                Category = Utilities.RandomString(5),
                Name = Utilities.RandomString(5),
                Description = Utilities.RandomString(5),
                DefaultValue = defaultValue,
            },
            new BooleanOptionValue() { CurrentValue = defaultValue},
            true);
    }

    /// <summary>
    ///     Creates a new <see cref="FieldOption"/> with a given value for testing.
    /// </summary>
    /// <param name="defaultValue">
    ///     The default value for the option.
    /// </param>
    /// <param name="guid">
    ///     The GUID for the option. If <c>null</c>, a new GUID will be generated.
    /// </param>
    /// <returns>
    ///     A new <see cref="FieldOption"/> with the given value.
    /// </returns>
    public static FieldOption FieldOption(string defaultValue, Guid? guid = null)
    {
        return new FieldOption(
            new FieldOptionDefinition
            {
                Guid = guid ?? Guid.NewGuid(),
                Category = Utilities.RandomString(5),
                Name = Utilities.RandomString(5),
                Description = Utilities.RandomString(5),
                DefaultValue = defaultValue,
            },
            new FieldOptionValue() { CurrentValue = defaultValue },
            true);
    }

    /// <summary>
    ///     Creates a new <see cref="FieldArrayOption"/> with a given value for testing.
    /// </summary>
    /// <param name="defaultValue">
    ///     The default value for the option.
    /// </param>
    /// <param name="guid">
    ///     The GUID for the option. If <c>null</c>, a new GUID will be generated.
    /// </param>
    /// <returns>
    ///     A new <see cref="FieldArrayOption"/> with the given value.
    /// </returns>
    public static FieldArrayOption FieldArrayOption(string[] defaultValue, Guid? guid = null)
    {
        return new FieldArrayOption(
            new FieldArrayOptionDefinition
            {
                Guid = guid ?? Guid.NewGuid(),
                Category = Utilities.RandomString(5),
                Name = Utilities.RandomString(5),
                Description = Utilities.RandomString(5),
                DefaultValue = defaultValue,
            },
            new FieldArrayOptionValue() { CurrentValue = defaultValue},
            true);
    }
}
