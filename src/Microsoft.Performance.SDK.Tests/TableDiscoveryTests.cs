// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.

using System;
using System.Linq;
using Microsoft.Performance.SDK.Processing;
using Microsoft.Performance.Testing;
using Microsoft.Performance.Testing.SDK;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace Microsoft.Performance.SDK.Tests
{
    public class TableDiscoveryTests
    {
        [TestMethod]
        [UnitTest]
        public void NoTablesInAssemblyLeavesEmptyProperties()
        {
            var assembly = new FakeAssembly
            {
                TypesToReturn = new[]
                {
                    typeof(DateTime),
                }
            };

            var sut = TableDiscovery.CreateForAssembly(assembly);
            var tables = sut.Discover(new FakeSerializer());

            Assert.AreEqual(0, tables.Count);
        }

        [TestMethod]
        [UnitTest]
        public void OnlyDataTablesInAssemblyPopulatesCorrectly()
        {
            var assembly = new FakeAssembly
            {
                TypesToReturn = new[]
                {
                    typeof(DateTime),
                    typeof(StubDataTableOne),
                    typeof(StubDataTableTwo),
                    typeof(StubDataTableThree),
                    typeof(Exception),
                }
            };

            var serializer = new FakeSerializer();

            Assert.IsTrue(
                TableDescriptorFactory.TryCreate(
                    typeof(StubDataTableOne),
                    serializer,
                    out TableDescriptor expectedDescriptor1));
            Assert.IsTrue(
                TableDescriptorFactory.TryCreate(
                    typeof(StubDataTableTwo),
                    serializer,
                    out TableDescriptor expectedDescriptor2));
            Assert.IsTrue(
                TableDescriptorFactory.TryCreate(
                    typeof(StubDataTableThree),
                    serializer,
                    out TableDescriptor expectedDescriptor3));

            var sut = TableDiscovery.CreateForAssembly(assembly);
            var tables = sut.Discover(new FakeSerializer());

            Assert.AreEqual(3, tables.Count);
            Assert.IsNotNull(tables.Single(x => x.Descriptor.Equals(expectedDescriptor1)));
            Assert.IsNotNull(tables.Single(x => x.Descriptor.Equals(expectedDescriptor2)));
            Assert.IsNotNull(tables.Single(x => x.Descriptor.Equals(expectedDescriptor3)));
        }

        [TestMethod]
        [UnitTest]
        public void OnlyMetadataTablesInAssemblyPopulatesCorrectly()
        {
            var assembly = new FakeAssembly
            {
                TypesToReturn = new[]
                {
                    typeof(DateTime),
                    typeof(StubMetadataTableOne),
                    typeof(StubMetadataTableTwo),
                    typeof(Exception),
                }
            };

            var serializer = new FakeSerializer();

            Assert.IsTrue(
                TableDescriptorFactory.TryCreate(
                    typeof(StubMetadataTableOne),
                    serializer,
                    out TableDescriptor expectedDescriptor1));
            Assert.IsTrue(
                TableDescriptorFactory.TryCreate(
                    typeof(StubMetadataTableTwo),
                    serializer,
                    out TableDescriptor expectedDescriptor2));

            var sut = TableDiscovery.CreateForAssembly(assembly);
            var tables = sut.Discover(new FakeSerializer());

            Assert.AreEqual(2, tables.Count);
            Assert.IsNotNull(tables.Single(x => x.Descriptor.Equals(expectedDescriptor1)));
            Assert.IsNotNull(tables.Single(x => x.Descriptor.Equals(expectedDescriptor2)));
        }

        [TestMethod]
        [UnitTest]
        public void BothTableTypesInInAssemblyPopulatesCorrectly()
        {
            var assembly = new FakeAssembly
            {
                TypesToReturn = new[]
                {
                    typeof(DateTime),
                    typeof(StubDataTableOne),
                    typeof(StubDataTableTwo),
                    typeof(StubDataTableThree),
                    typeof(StubMetadataTableOne),
                    typeof(StubMetadataTableTwo),
                    typeof(Exception),
                }
            };

            var serializer = new FakeSerializer();

            Assert.IsTrue(
                TableDescriptorFactory.TryCreate(
                    typeof(StubDataTableOne),
                    serializer,
                    out TableDescriptor expectedDescriptor1));
            Assert.IsTrue(
                TableDescriptorFactory.TryCreate(
                    typeof(StubDataTableTwo),
                    serializer,
                    out TableDescriptor expectedDescriptor2));
            Assert.IsTrue(
                TableDescriptorFactory.TryCreate(
                    typeof(StubDataTableThree),
                    serializer,
                    out TableDescriptor expectedDescriptor3));

            Assert.IsTrue(
                TableDescriptorFactory.TryCreate(
                    typeof(StubMetadataTableOne),
                    serializer,
                    out TableDescriptor expectedDescriptor4));
            Assert.IsTrue(
                TableDescriptorFactory.TryCreate(
                    typeof(StubMetadataTableTwo),
                    serializer,
                    out TableDescriptor expectedDescriptor5));

            var sut = TableDiscovery.CreateForAssembly(assembly);
            var tables = sut.Discover(new FakeSerializer());

            Assert.AreEqual(5, tables.Count);
            Assert.IsNotNull(tables.Single(x => x.Descriptor.Equals(expectedDescriptor1)));
            Assert.IsNotNull(tables.Single(x => x.Descriptor.Equals(expectedDescriptor2)));
            Assert.IsNotNull(tables.Single(x => x.Descriptor.Equals(expectedDescriptor3)));
            Assert.IsNotNull(tables.Single(x => x.Descriptor.Equals(expectedDescriptor4)));
            Assert.IsNotNull(tables.Single(x => x.Descriptor.Equals(expectedDescriptor5)));
        }
    }
}
