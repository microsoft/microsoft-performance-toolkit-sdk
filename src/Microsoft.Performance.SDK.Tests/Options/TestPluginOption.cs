// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.

using System;
using Microsoft.Performance.SDK.Options;
using Microsoft.Performance.Testing;

namespace Microsoft.Performance.SDK.Tests.Options;

/// <summary>
///     Contains helper methods for creating <see cref="PluginOption"/> instances during testing.
/// </summary>
public static class TestPluginOption
{
    /// <summary>
    ///     Creates a new <see cref="BooleanOption"/> with random values for testing.
    /// </summary>
    /// <param name="defaultValue">
    ///     The default value for the option.
    /// </param>
    /// <param name="guid">
    ///     The GUID for the option. If <c>null</c>, a new GUID will be generated.
    /// </param>
    /// <returns>
    ///     A new <see cref="BooleanOption"/> with random values.
    /// </returns>
    public static BooleanOption BooleanOption(bool defaultValue, Guid? guid = null)
    {
        return new BooleanOption()
        {
            Guid = guid ?? Guid.NewGuid(),
            Category = Utilities.RandomString(5),
            Name = Utilities.RandomString(5),
            Description = Utilities.RandomString(5),
            DefaultValue = defaultValue,
        };
    }

    /// <summary>
    ///     Creates a new <see cref="FieldOption"/> with random values for testing.
    /// </summary>
    /// <param name="defaultValue">
    ///     The default value for the option.
    /// </param>
    /// <param name="guid">
    ///     The GUID for the option. If <c>null</c>, a new GUID will be generated.
    /// </param>
    /// <returns>
    ///     A new <see cref="FieldOption"/> with random values.
    /// </returns>
    public static FieldOption FieldOption(string defaultValue, Guid? guid = null)
    {
        return new FieldOption()
        {
            Guid = guid ?? Guid.NewGuid(),
            Category = Utilities.RandomString(5),
            Name = Utilities.RandomString(5),
            Description = Utilities.RandomString(5),
            DefaultValue = defaultValue,
        };
    }

    /// <summary>
    ///     Creates a new <see cref="FieldArrayOption"/> with random values for testing.
    /// </summary>
    /// <param name="defaultValue">
    ///     The default value for the option.
    /// </param>
    /// <param name="guid">
    ///     The GUID for the option. If <c>null</c>, a new GUID will be generated.
    /// </param>
    /// <returns>
    ///     A new <see cref="FieldArrayOption"/> with random values.
    /// </returns>
    public static FieldArrayOption FieldArrayOption(string[] defaultValue, Guid? guid = null)
    {
        return new FieldArrayOption()
        {
            Guid = guid ?? Guid.NewGuid(),
            Category = Utilities.RandomString(5),
            Name = Utilities.RandomString(5),
            Description = Utilities.RandomString(5),
            DefaultValue = defaultValue,
        };
    }
}
