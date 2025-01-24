// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.

using System;
using System.Linq;
using Microsoft.Performance.SDK.Options;
using Microsoft.Performance.SDK.Runtime.Options;
using Microsoft.Performance.SDK.Runtime.Options.Serialization.DTO;
using Microsoft.Performance.Testing;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace Microsoft.Performance.SDK.Runtime.Tests.Options;

[TestClass]
[UnitTest]
public class PluginOptionsRegistrarTests
{
    /// <summary>
    ///     Asserts that the <see cref="PluginOptionsRegistrar.IsUsingDefault"/> method returns <c>false</c>
    ///     after setting the current value of an option to a new value that is different from its default.
    /// </summary>
    [TestMethod]
    public void SettingCurrentValue_ToNewValue_UpdatesIsUsingDefault()
    {
        var sut = new PluginOptionsRegistrar();

        var option = TestOptions.CreateBooleanOption(true);
        sut.Register(option, true);

        option.CurrentValue = false;
        Assert.IsFalse(sut.IsUsingDefault(option));
    }

    /// <summary>
    ///     Asserts that the <see cref="PluginOptionsRegistrar.IsUsingDefault"/> method returns <c>false</c>
    ///     after setting the current value of an option to its default value.
    /// </summary>
    [TestMethod]
    public void SettingCurrentValue_ToDefaultValue_UpdatesIsUsingDefault()
    {
        var sut = new PluginOptionsRegistrar();

        var option = TestOptions.CreateBooleanOption(true);
        sut.Register(option, true);

        option.CurrentValue = false;
        Assert.IsFalse(sut.IsUsingDefault(option));
    }

    /// <summary>
    ///     Asserts that the <see cref="PluginOptionsRegistrar.IsUsingDefault"/> method returns <c>true</c>
    ///     after calling <see cref="PluginOption.ApplyDefault"/> on an option.
    /// </summary>
    [TestMethod]
    public void CallingApplyDefault_UpdatesIsUsingDefault()
    {
        var sut = new PluginOptionsRegistrar();

        var option = TestOptions.CreateFieldOption("Default");
        sut.Register(option, true);
        option.CurrentValue = "Non default";

        option.ApplyDefault();
        Assert.IsTrue(sut.IsUsingDefault(option));
    }

    /// <summary>
    ///     Asserts that an option's <see cref="PluginOption{T}.DefaultValue"/> is set as the current value
    ///     when updating options from a DTO whose value indicates that the option is using the default value, but
    ///     the whose serialized default value differs from the current <see cref="PluginOption{T}.DefaultValue"/>.
    /// </summary>
    [TestMethod]
    public void ApplyingDto_WithOldDefaultValue_RestoresNewDefaultValue()
    {
        var sut = new PluginOptionsRegistrar();

        string expected = "New Default Value";

        var option = TestOptions.CreateFieldOption(expected);
        option.CurrentValue = "Random Value";

        sut.Register(option, false);

        sut.UpdateFromDto(CreateDto(new FieldPluginOptionDto(option.Guid, true, "Old Default Value")));
        Assert.AreEqual(expected, option.CurrentValue);
        Assert.IsTrue(sut.IsUsingDefault(option));
    }

    private PluginOptionsDto CreateDto(params PluginOptionDto[] dtos)
    {
        return new PluginOptionsDto(
            dtos.OfType<BooleanPluginOptionDto>().ToArray(),
            dtos.OfType<FieldPluginOptionDto>().ToArray(),
            dtos.OfType<FieldArrayPluginOptionDto>().ToArray());
    }
}

public static class TestOptions
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