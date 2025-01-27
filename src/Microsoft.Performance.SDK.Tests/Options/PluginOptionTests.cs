// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.

using Microsoft.Performance.SDK.Options;
using Microsoft.Performance.Testing;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace Microsoft.Performance.SDK.Tests.Options;

[TestClass]
[UnitTest]
public class PluginOptionTests
{
    /// <summary>
    ///     Asserts that the <see cref="PluginOption.IsUsingDefault"/> method returns <c>false</c>
    ///     after setting the current value of an option to a new value that is different from its default.
    /// </summary>
    [TestMethod]
    public void SettingCurrentValue_ToNewValue_UpdatesIsUsingDefault()
    {
        var option = TestPluginOptions.CreateBooleanOption(true);
        option.CurrentValue = false;

        Assert.IsFalse(option.IsUsingDefault);
    }

    /// <summary>
    ///     Asserts that the <see cref="PluginOption.IsUsingDefault"/> method returns <c>false</c>
    ///     after setting the current value of an option to its default value.
    /// </summary>
    [TestMethod]
    public void SettingCurrentValue_ToDefaultValue_UpdatesIsUsingDefault()
    {
        var option = TestPluginOptions.CreateBooleanOption(true);
        option.CurrentValue = false;

        Assert.IsFalse(option.IsUsingDefault);
    }

    /// <summary>
    ///     Asserts that the <see cref="PluginOption.IsUsingDefault"/> method returns <c>true</c>
    ///     after calling <see cref="PluginOption.ApplyDefault"/> on an option.
    /// </summary>
    [TestMethod]
    public void CallingApplyDefault_UpdatesIsUsingDefault()
    {
        var option = TestPluginOptions.CreateFieldOption("Default");
        option.CurrentValue = "Non default";
        option.ApplyDefault();

        Assert.IsTrue(option.IsUsingDefault);
    }
}