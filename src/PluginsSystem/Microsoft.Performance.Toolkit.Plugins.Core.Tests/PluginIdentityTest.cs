// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.

using Microsoft.VisualStudio.TestTools.UnitTesting;
using NuGet.Versioning;

namespace Microsoft.Performance.Toolkit.Plugins.Core.Tests;

[TestClass]
public class PluginIdentityTest
{
    [TestMethod]
    [DataRow("Test+Plugin")]
    [DataRow("Test|Plugin")]
    [DataRow("")]
    [DataRow("Test Plugin")]
    [DataRow(" ")]
    [DataRow("\t")]
    [DataRow("\n")]
    [DataRow("Test\nPlugin")]
    public void HasValidId_InvalidCharacters_ReturnFalse(string id)
    {
        var sut = new PluginIdentity(id, new SemanticVersion(1, 2, 3));
        Assert.IsFalse(sut.HasValidId(out string _));
    }

    [TestMethod]
    [DataRow("TestPlugin")]
    [DataRow("Test.Plugin")]
    [DataRow("Test-Plugin")]
    [DataRow("Test.Plugin-1")]
    [DataRow("Test.Plugin-1.2")]
    public void HasValidId_ValidId_ReturnTrue(string id)
    {
        var sut = new PluginIdentity(id, new SemanticVersion(1, 2, 3));
        Assert.IsTrue(sut.HasValidId(out string _));
    }

    [TestMethod]
    public void HasValidId_LongId_ReturnFalse()
    {
        var sut = new PluginIdentity(new string('A', PluginIdentity.MaxIdLength + 1), new SemanticVersion(1, 2, 3));
        Assert.IsFalse(sut.HasValidId(out string _));
    }
}
