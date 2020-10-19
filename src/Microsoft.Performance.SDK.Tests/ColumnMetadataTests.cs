// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.

using System;
using Microsoft.Performance.SDK.Processing;
using Microsoft.Performance.Testing;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace Microsoft.Performance.SDK.Tests
{
    [TestClass]
    public class ColumnMetadataTests
    {
        private Guid ColumnGuid { get; set; } = Guid.NewGuid();

        [TestMethod]
        [UnitTest]
        public void ConstantStringName_NameAlwaysReturnsSameValue()
        {
            var metadata = new ColumnMetadata(this.ColumnGuid, "name");

            for (var i = 0; i < 1000; ++i)
            {
                Assert.AreEqual("name", metadata.Name);
            }
        }

        [TestMethod]
        [UnitTest]
        public void ConstantStringName_NameIsConstantReturnsTrue()
        {
            var metadata = new ColumnMetadata(this.ColumnGuid, "name");

            Assert.IsTrue(metadata.IsNameConstant);
        }

        [TestMethod]
        [UnitTest]
        public void ConstantTitleProjection_NameStillRespectsDefault()
        {
            var projection = Projection.Constant("name");
            var metadata = new ColumnMetadata(this.ColumnGuid, "default", projection, "test column");

            for (var i = 0; i < 1000; ++i)
            {
                Assert.AreEqual("default", metadata.Name);
            }
        }

        [TestMethod]
        [UnitTest]
        public void ConstantTitleProjection_NameIsConstantReturnsTrue()
        {
            var projection = Projection.Constant("name");
            var metadata = new ColumnMetadata(this.ColumnGuid, "default", projection, "test column");

            Assert.IsTrue(metadata.IsNameConstant);
        }

        [TestMethod]
        [UnitTest]
        public void DynamicTitleProjection_NameIsConstantReturnsFalse()
        {
            var projection = Projection.CreateUsingFuncAdaptor<int, string>(i => i.ToString());

            var metadata = new ColumnMetadata(this.ColumnGuid, "default", projection, "test column");

            Assert.IsFalse(metadata.IsNameConstant);
        }

        [TestMethod]
        [UnitTest]
        public void DynamicTitleProjection_ProjectorIsExposed()
        {
            var projection = Projection.CreateUsingFuncAdaptor<int, string>(i => i.ToString());

            var metadata = new ColumnMetadata(this.ColumnGuid, "default", projection, "test column");

            Assert.AreSame(projection, metadata.NameProjection);
        }

        [TestMethod]
        [UnitTest]
        public void DynamicTitleProjection_NameAlwaysReturnsDefaultValue()
        {
            var projection = Projection.CreateUsingFuncAdaptor<int, string>(i => i.ToString());

            var metadata = new ColumnMetadata(
                this.ColumnGuid,
                "default",
                projection,
                "test column");

            for (var i = 0; i < 1000; ++i)
            {
                Assert.AreEqual("default", metadata.Name);
            }
        }
    }
}
