// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.

using System;
using Microsoft.Performance.SDK.Options;
using Microsoft.Performance.Testing;

namespace Microsoft.Performance.SDK.Tests.Options;

public static class TestPluginOption
{
    public static BooleanOption BooleanOption(bool defaultValue)
    {
        return new BooleanOption()
        {
            Guid = Guid.NewGuid(),
            Category = Utilities.RandomString(5),
            Name = Utilities.RandomString(5),
            Description = Utilities.RandomString(5),
            DefaultValue = defaultValue,
        };
    }

    public static FieldOption FieldOption(string defaultValue)
    {
        return new FieldOption()
        {
            Guid = Guid.NewGuid(),
            Category = Utilities.RandomString(5),
            Name = Utilities.RandomString(5),
            Description = Utilities.RandomString(5),
            DefaultValue = defaultValue,
        };
    }

    public static FieldArrayOption FieldArrayOption(string[] defaultValue)
    {
        return new FieldArrayOption()
        {
            Guid = Guid.NewGuid(),
            Category = Utilities.RandomString(5),
            Name = Utilities.RandomString(5),
            Description = Utilities.RandomString(5),
            DefaultValue = defaultValue,
        };
    }
}