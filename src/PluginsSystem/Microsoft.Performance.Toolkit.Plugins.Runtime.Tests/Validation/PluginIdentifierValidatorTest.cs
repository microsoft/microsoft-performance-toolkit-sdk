// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.

using Microsoft.Performance.SDK.Runtime;
using Microsoft.Performance.Toolkit.Plugins.Core;
using Microsoft.Performance.Toolkit.Plugins.Core.Metadata;
using Microsoft.Performance.Toolkit.Plugins.Runtime.Validation;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using NuGet.Versioning;

namespace Microsoft.Performance.Toolkit.Plugins.Runtime.Tests.Validation;

[TestClass]
public class PluginIdentifierValidatorTest
{
    [TestMethod]
    public void ValidPluginIdPassesGate()
    {
        var sut = new PluginIdentifierValidator(Logger.Null);

        var metadata = CreatePluginMetadata("Test.Plugin");
        var errors = sut.GetValidationErrors(metadata);

        Assert.IsTrue(metadata.Identity.HasValidId(out string _));
        Assert.AreEqual(0, errors.Length);
    }

    [TestMethod]
    public void InvalidPluginIdDoesNotPassGate()
    {
        var sut = new PluginIdentifierValidator(Logger.Null);

        var metadata = CreatePluginMetadata("Test+Plugin");
        var errors = sut.GetValidationErrors(metadata);

        Assert.IsFalse(metadata.Identity.HasValidId(out string _));
        Assert.IsTrue(errors.Length > 0);
    }

    private PluginMetadata CreatePluginMetadata(string pluginId)
    {
        var stubVersion = new SemanticVersion(1, 2, 3);

        var pluginIdentity = new PluginIdentity(pluginId, stubVersion);

        return new PluginMetadata(
            pluginIdentity,
            100,
            "Test Plugin",
            "Test Description",
            stubVersion,
            new Uri("https://www.contoso.com"),
            [new PluginOwnerInfo("John Doe", "Address", [], [])]);
    }
}
