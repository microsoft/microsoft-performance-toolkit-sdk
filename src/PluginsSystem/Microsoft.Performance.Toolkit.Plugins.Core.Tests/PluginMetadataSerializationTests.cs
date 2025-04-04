// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.

using Microsoft.Performance.Toolkit.Plugins.Core.Metadata;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Fixture = AutoFixture.Fixture;

namespace Microsoft.Performance.Toolkit.Plugins.Core.Tests;

[TestClass]
public sealed class PluginMetadataSerializationTests
{
    private Fixture? fixture = null;

    [TestInitialize]
    public void Setup()
    {
        this.fixture = new Fixture();
    }

    [TestMethod]
    public void PluginMetadata_Serialization_RoundTrip()
    {
        SerializationTestsHelper.RunSerializationTest<PluginMetadata>(this.fixture!);
    }

    [TestMethod]
    public void PluginContentsMetadata_Serialization_RoundTrip()
    {
        SerializationTestsHelper.RunSerializationTest<PluginContentsMetadata>(this.fixture!);
    }
}
