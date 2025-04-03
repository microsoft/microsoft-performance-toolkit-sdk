using AutoFixture;
using Microsoft.Performance.Toolkit.Plugins.Core.Metadata;
using Microsoft.Performance.Toolkit.Plugins.Core.Serialization;
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
        ISerializer<PluginMetadata> metadataSerializer = SerializationUtils.GetJsonSerializerWithDefaultOptions<PluginMetadata>();

        PluginMetadata metadata = this.fixture!.Create<PluginMetadata>();

        using var stream = new MemoryStream();
        metadataSerializer.Serialize(stream, metadata);

        stream.Position = 0;

        PluginMetadata deserializedMetadata = metadataSerializer.Deserialize(stream);

        Assert.IsNotNull(deserializedMetadata);
        Assert.IsTrue(metadata.Equals(deserializedMetadata));
    }

    [TestMethod]
    public void PluginContentsMetadata_Serialization_RoundTrip()
    {
        ISerializer<PluginContentsMetadata> contentsMetadataSerializer = SerializationUtils.GetJsonSerializerWithDefaultOptions<PluginContentsMetadata>();
        
        PluginContentsMetadata contentsMetadata = this.fixture!.Create<PluginContentsMetadata>();
       
        using var stream = new MemoryStream();
        contentsMetadataSerializer.Serialize(stream, contentsMetadata);
        
        stream.Position = 0;

        PluginContentsMetadata deserializedContentsMetadata = contentsMetadataSerializer.Deserialize(stream);
        
        Assert.IsNotNull(deserializedContentsMetadata);
        Assert.IsTrue(contentsMetadata.Equals(deserializedContentsMetadata));
    }
}
