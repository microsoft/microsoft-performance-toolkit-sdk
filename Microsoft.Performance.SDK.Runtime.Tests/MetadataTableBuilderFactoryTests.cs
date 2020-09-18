// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.

using System;
using System.Linq;
using Microsoft.Performance.Testing;
using Microsoft.Performance.Testing.SDK;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace Microsoft.Performance.SDK.Runtime.Tests
{
    [TestClass]
    public class MetadataTableBuilderFactoryTests
    {
        public MetadataTableBuilderFactory Sut { get; set; }

        [TestInitialize]
        public void Initialize()
        {
            this.Sut = new MetadataTableBuilderFactory();
        }

        [TestMethod]
        [UnitTest]
        public void WhenConstructedHasNoCreatedTables()
        {
            Assert.AreEqual(0, this.Sut.CreatedTables.Count());
        }

        [TestMethod]
        [UnitTest]
        public void WhenCreateCalledReturnsNonNullTable()
        {
            var descriptor = Any.TableDescriptor();

            var created = this.Sut.Create(descriptor);

            Assert.IsNotNull(created);
        }

        [TestMethod]
        [UnitTest]
        public void WhenTableIsCreatedAddedToCreatedTables()
        {
            var descriptor = Any.TableDescriptor();

            var created = this.Sut.Create(descriptor);

            Assert.IsTrue(this.Sut.CreatedTables.Contains(created));
        }

        [TestMethod]
        [UnitTest]
        public void CreateReturnsTableBuilderForDescriptor()
        {
            var descriptor = Any.TableDescriptor();

            var created = this.Sut.Create(descriptor);

            var toCheck = this.Sut.CreatedTables.SingleOrDefault(x => x.TableDescriptor == descriptor);
            Assert.IsNotNull(toCheck);
        }
    }
}
