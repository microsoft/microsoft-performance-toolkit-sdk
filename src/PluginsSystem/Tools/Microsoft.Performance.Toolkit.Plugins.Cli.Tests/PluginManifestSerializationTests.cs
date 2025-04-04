// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.

using Microsoft.Performance.Toolkit.Plugins.Cli.Manifest;
using Microsoft.Performance.Toolkit.Plugins.Core.Tests;
using Fixture = AutoFixture.Fixture;

namespace Microsoft.Performance.Toolkit.Plugins.Cli.Tests;

[TestClass]
public sealed class PluginManifestSerializationTest
{
    private Fixture? fixture = null;

    [TestInitialize]
    public void Setup()
    {
        this.fixture = new Fixture();
    }

    [TestMethod]
    public void PluginManifest_Serialization_RoundTrip()
    {
        SerializationTestsHelper.RunSerializationTest<PluginManifest>(this.fixture!);
    }
}
