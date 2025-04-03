// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.

using AutoFixture;
using Microsoft.Performance.Toolkit.Plugins.Cli.Manifest;
using Microsoft.Performance.Toolkit.Plugins.Core.Serialization;
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
        ISerializer<PluginManifest> manifestSerializer = SerializationUtils.GetJsonSerializer<PluginManifest>(Program.PluginManifestSerializerDefaultOptions);

        PluginManifest pluginManifest = this.fixture.Create<PluginManifest>();

        using var stream = new MemoryStream();
        manifestSerializer.Serialize(stream, pluginManifest);

        stream.Position = 0;

        PluginManifest deserializedPluginManifest = manifestSerializer.Deserialize(stream);

        Assert.IsNotNull(deserializedPluginManifest);
        Assert.IsTrue(pluginManifest.Equals(deserializedPluginManifest));
    }
}
