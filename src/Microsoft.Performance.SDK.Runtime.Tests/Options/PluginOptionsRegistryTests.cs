// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.

using System;
using System.Linq;
using Microsoft.Performance.SDK.Options;
using Microsoft.Performance.SDK.Runtime.Options;
using Microsoft.Performance.SDK.Tests.Options;
using Microsoft.Performance.Testing;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace Microsoft.Performance.SDK.Runtime.Tests.Options;

[TestClass]
[UnitTest]
public class PluginOptionsRegistryTests
{
    /// <summary>
    ///     Asserts that an option's <see cref="PluginOption{T}.DefaultValue"/> is set as the current value
    ///     when updating options from a DTO whose value indicates that the option is using the default value, but
    ///     the whose serialized default value differs from the current <see cref="PluginOption{T}.DefaultValue"/>.
    /// </summary>
    [TestMethod]
    public void ApplyingDto_WithOldDefaultValue_RestoresNewDefaultValue()
    {
        var sut = new PluginOptionsRegistry();

        const string expected = "New Default Value";

        var option = TestPluginOptions.CreateFieldOption(expected);
        option.CurrentValue = "Random Value";

        sut.Register(option);

        sut.UpdateFromDto(TestPluginOptionDtos.CreateDto(TestPluginOptionDtos.CreateFieldOptionDto(option.Guid, true, "Old Default Value")));
        Assert.AreEqual(expected, option.CurrentValue);
        Assert.IsTrue(option.IsUsingDefault);
    }

    [TestMethod]
    public void ApplyingDto_WithNonExistentOption_HasNoEffect()
    {
        var sut = new PluginOptionsRegistry();
        var option = TestPluginOptions.CreateBooleanOption(false);
        sut.Register(option);

        sut.UpdateFromDto(TestPluginOptionDtos.CreateDto(TestPluginOptionDtos.CreateFieldOptionDto(Guid.NewGuid(), true, "Random Value")));

        Assert.AreEqual(1, sut.Options.Count);
        Assert.AreEqual(option, sut.Options.First());
        Assert.IsTrue(option.IsUsingDefault);
    }

    [TestMethod]
    public void ApplyingDto_WithSerializedNonDefaultValue_WhileUsingDefault_RestoresValue()
    {
        var sut = new PluginOptionsRegistry();
        var option = TestPluginOptions.CreateFieldOption("Default");
        Assert.IsTrue(option.IsUsingDefault);

        const string expected = "Serialized value";

        sut.Register(option);
        sut.UpdateFromDto(TestPluginOptionDtos.CreateDto(TestPluginOptionDtos.CreateFieldOptionDto(option.Guid, false, expected)));

        Assert.AreEqual(expected, option.CurrentValue);
        Assert.IsFalse(option.IsUsingDefault);
    }

    [TestMethod]
    public void ApplyingDto_WithSerializedNonDefaultValue_WhileUsingNonDefaultValue_RestoresValue()
    {
        var sut = new PluginOptionsRegistry();
        var option = TestPluginOptions.CreateFieldOption("Default");
        option.CurrentValue = "Non default";
        Assert.IsFalse(option.IsUsingDefault);

        const string expected = "Serialized value";

        sut.Register(option);
        sut.UpdateFromDto(TestPluginOptionDtos.CreateDto(TestPluginOptionDtos.CreateFieldOptionDto(option.Guid, false, expected)));

        Assert.AreEqual(expected, option.CurrentValue);
        Assert.IsFalse(option.IsUsingDefault);
    }
}