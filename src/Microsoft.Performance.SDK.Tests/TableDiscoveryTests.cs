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
            var tables = sut.Discover(new FakeSerializer()).ToList();

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

            var expectedDescriptors = TableDescriptorUtils.CreateTableDescriptors(
                serializer,
                typeof(StubDataTableOne),
                typeof(StubDataTableTwo),
                typeof(StubDataTableThree));

            var sut = TableDiscovery.CreateForAssembly(assembly);
            var tables = sut.Discover(new FakeSerializer()).ToList();

            Assert.AreEqual(3, tables.Count);
            Assert.IsNotNull(tables.Single(x => x.Descriptor.Equals(expectedDescriptors[0])));
            Assert.IsNotNull(tables.Single(x => x.Descriptor.Equals(expectedDescriptors[1])));
            Assert.IsNotNull(tables.Single(x => x.Descriptor.Equals(expectedDescriptors[2])));
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
            

            var expectedDescriptors = TableDescriptorUtils.CreateTableDescriptors(
                serializer,
                typeof(StubMetadataTableOne),
                typeof(StubMetadataTableTwo));

            var sut = TableDiscovery.CreateForAssembly(assembly);
            var tables = sut.Discover(new FakeSerializer()).ToList();

            Assert.AreEqual(2, tables.Count);
            Assert.IsNotNull(tables.Single(x => x.Descriptor.Equals(expectedDescriptors[0])));
            Assert.IsNotNull(tables.Single(x => x.Descriptor.Equals(expectedDescriptors[1])));
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

            var expectedDescriptors = TableDescriptorUtils.CreateTableDescriptors(
                serializer,
                typeof(StubDataTableOne),
                typeof(StubDataTableTwo),
                typeof(StubDataTableThree),
                typeof(StubMetadataTableOne),
                typeof(StubMetadataTableTwo));

            var sut = TableDiscovery.CreateForAssembly(assembly);
            var tables = sut.Discover(new FakeSerializer()).ToList();

            Assert.AreEqual(5, tables.Count);
            Assert.IsNotNull(tables.Single(x => x.Descriptor.Equals(expectedDescriptors[0])));
            Assert.IsNotNull(tables.Single(x => x.Descriptor.Equals(expectedDescriptors[1])));
            Assert.IsNotNull(tables.Single(x => x.Descriptor.Equals(expectedDescriptors[2])));
            Assert.IsNotNull(tables.Single(x => x.Descriptor.Equals(expectedDescriptors[3])));
            Assert.IsNotNull(tables.Single(x => x.Descriptor.Equals(expectedDescriptors[4])));
        }
    }
}
