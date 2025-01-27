// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.

using System;
using System.Linq;
using Microsoft.Performance.SDK.Options;
using Microsoft.Performance.SDK.Runtime.Options;
using Microsoft.Performance.SDK.Runtime.Options.Serialization.DTO;
using Microsoft.Performance.SDK.Tests.Options;
using Microsoft.Performance.Testing;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace Microsoft.Performance.SDK.Runtime.Tests.Options;

/// <summary>
///     Contains tests for <see cref="PluginOptionsRegistry"/>.
/// </summary>
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

        var option = TestPluginOption.FieldOption(expected);
        option.CurrentValue = "Random Value";

        sut.RegisterFrom(new TestPluginOptionsRegistryProvider(option));

        sut.UpdateFromDto(TestPluginOptionDto.PluginOptionsDto(TestPluginOptionDto.FieldOptionDto(option.Guid, true, "Old Default Value")));
        Assert.AreEqual(expected, option.CurrentValue);
        Assert.IsTrue(option.IsUsingDefault);
    }

    /// <summary>
    ///     Asserts that applying a <see cref="PluginOptionDto"/> that contains an entry for an option that has not
    ///     been registered has no effect on the registry.
    /// </summary>
    [TestMethod]
    public void ApplyingDto_WithNonExistentOption_HasNoEffect()
    {
        var sut = new PluginOptionsRegistry();
        var option = TestPluginOption.BooleanOption(false);
        sut.RegisterFrom(new TestPluginOptionsRegistryProvider(option));

        sut.UpdateFromDto(TestPluginOptionDto.PluginOptionsDto(TestPluginOptionDto.FieldOptionDto(Guid.NewGuid(), true, "Random Value")));

        Assert.AreEqual(1, sut.Options.Count);
        Assert.AreEqual(option, sut.Options.First());
        Assert.IsTrue(option.IsUsingDefault);
    }

    /// <summary>
    ///     Asserts that an option with a default value has its <see cref="PluginOption{T}.CurrentValue"/> updated
    ///     to the serialized value of the option's corresponding <see cref="PluginOptionDto"/>.
    /// </summary>
    [TestMethod]
    public void ApplyingDto_WithSerializedNonDefaultValue_WhileUsingDefault_SetsCurrentValue_ToSerializedValue()
    {
        var sut = new PluginOptionsRegistry();
        var option = TestPluginOption.FieldOption("Default");
        Assert.IsTrue(option.IsUsingDefault);

        const string expected = "Serialized value";

        sut.RegisterFrom(new TestPluginOptionsRegistryProvider(option));
        sut.UpdateFromDto(TestPluginOptionDto.PluginOptionsDto(TestPluginOptionDto.FieldOptionDto(option.Guid, false, expected)));

        Assert.AreEqual(expected, option.CurrentValue);
        Assert.IsFalse(option.IsUsingDefault);
    }

    /// <summary>
    ///     Asserts that an option with a non-default value has its <see cref="PluginOption{T}.CurrentValue"/> updated
    ///     to the serialized value of the option's corresponding <see cref="PluginOptionDto"/>.
    /// </summary>
    [TestMethod]
    public void ApplyingDto_WithSerializedNonDefaultValue_WhileUsingNonDefaultValue_RestoresValue()
    {
        var sut = new PluginOptionsRegistry();
        var option = TestPluginOption.FieldOption("Default");
        option.CurrentValue = "Non default";
        Assert.IsFalse(option.IsUsingDefault);

        const string expected = "Serialized value";

        sut.RegisterFrom(new TestPluginOptionsRegistryProvider(option));
        sut.UpdateFromDto(TestPluginOptionDto.PluginOptionsDto(TestPluginOptionDto.FieldOptionDto(option.Guid, false, expected)));

        Assert.AreEqual(expected, option.CurrentValue);
        Assert.IsFalse(option.IsUsingDefault);
    }
}