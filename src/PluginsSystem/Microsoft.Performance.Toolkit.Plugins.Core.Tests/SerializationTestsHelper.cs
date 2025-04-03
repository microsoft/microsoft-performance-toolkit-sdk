// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.

using AutoFixture;
using Microsoft.Performance.Toolkit.Plugins.Core.Serialization;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace Microsoft.Performance.Toolkit.Plugins.Core.Tests;

public class SerializationTestsHelper
{
    public static void RunSerializationTest<T>(Fixture fixture) where T : class
    {
        ISerializer<T> serializer = SerializationUtils.GetJsonSerializerWithDefaultOptions<T>();

        T original = fixture.Create<T>();

        using var stream = new MemoryStream();
        serializer.Serialize(stream, original);

        stream.Position = 0;

        T deserialized = serializer.Deserialize(stream);

        Assert.IsNotNull(deserialized);
        Assert.IsTrue(original.Equals(deserialized));
    }
}
