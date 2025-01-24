using System;
using System.Text.Json;
using Microsoft.Performance.SDK.Options;
using Microsoft.Performance.SDK.Runtime.Options;
using Microsoft.Performance.SDK.Runtime.Options.Serialization;
using Microsoft.Performance.Testing;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace Microsoft.Performance.SDK.Runtime.Tests.Options;

[TestClass]
[UnitTest]
public class PluginOptionsJsonConverterTests
{
    [TestMethod]
    public void WriteTest()
    {
        PluginOption[] options = {
            new BooleanOption()
            {
                Guid = Guid.NewGuid(),
                Category = "Category",
                Name = "Name",
                Description = "Description",
                DefaultValue = true,
                CurrentValue = false,
            },

            new FieldArrayOption()
            {
                Guid = Guid.NewGuid(),
                Category = "Category",
                Name = "Name",
                Description = "Description",
                DefaultValue = new[] { "Foo", "bar" }
            },
        };

        var registry = new PluginOptionsRegistrar();
        registry.RegisterAsDefault(options);

        string jsonString = JsonSerializer.Serialize(new PluginOptionToOptionDtoConverter().Convert(registry));
    }
}