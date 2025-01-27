using System;
using System.Linq;
using Microsoft.Performance.SDK.Options;
using Microsoft.Performance.SDK.Runtime.Options.Serialization.DTO;
using Microsoft.Performance.Testing;

namespace Microsoft.Performance.SDK.Tests.Options;

public static class TestPluginOptions
{
    public static BooleanOption CreateBooleanOption(bool defaultValue)
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

    public static FieldOption CreateFieldOption(string defaultValue)
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
}