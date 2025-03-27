// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.

using System;
using Microsoft.Performance.SDK.Options;
using Microsoft.Performance.SDK.Runtime.Options;
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
        var value = true;
        var option = TestPluginOption.BooleanOption(value);
        option.SetValue(!value);

        Assert.IsFalse(option.IsUsingDefault);
    }

    /// <summary>
    ///     Asserts that the <see cref="PluginOption.IsUsingDefault"/> method returns <c>false</c>
    ///     after setting the current value of an option to its default value.
    /// </summary>
    [TestMethod]
    public void SettingCurrentValue_ToDefaultValue_UpdatesIsUsingDefault()
    {
        var value = true;
        var option = TestPluginOption.BooleanOption(value);
        option.SetValue(value);

        Assert.IsFalse(option.IsUsingDefault);
    }

    /// <summary>
    ///     Asserts that the <see cref="PluginOption.IsUsingDefault"/> method returns <c>true</c>
    ///     after calling <see cref="PluginOption.ApplyDefault"/> on an option.
    /// </summary>
    [TestMethod]
    public void CallingApplyDefault_UpdatesIsUsingDefault()
    {
        var option = TestPluginOption.FieldOption("Default");
        option.SetValue("Non default");
        option.ApplyDefault();

        Assert.IsTrue(option.IsUsingDefault);
    }

    /// <summary>
    ///     Asserts that the <see cref="PluginOption{T}.CurrentValue"/> method returns the associated <see cref="PluginOption{T}.DefaultValue"/>
    ///     after calling <see cref="PluginOption.ApplyDefault"/> on an option.
    /// </summary>
    [TestMethod]
    public void CallingApplyDefault_UpdatesCurrentValue()
    {
        string defaultValue = "Default";
        var option = TestPluginOption.FieldOption(defaultValue);
        option.SetValue("Non default");
        option.ApplyDefault();

        Assert.AreEqual(defaultValue, option.Value.CurrentValue);
    }

    [TestMethod]
    [DataRow(true, false)]
    [DataRow(true, true)]
    public void SettingCurrentValue_ToNewValue_RaisesOptionChanged(bool defaultValue, bool newValue)
    {
        var option = TestPluginOption.BooleanOption(defaultValue);
        var raised = false;
        option.Value.OptionChanged += (s, e) => raised = true;

        option.SetValue(newValue);

        Assert.IsTrue(raised);
    }

    [TestMethod]
    public void OptionsChangedHandler_ThrowingException_DoesNotThrow()
    {
        var option = TestPluginOption.BooleanOption(true);
        option.Value.OptionChanged += (s, e) => throw new Exception();

        option.SetValue(false);
    }
}
