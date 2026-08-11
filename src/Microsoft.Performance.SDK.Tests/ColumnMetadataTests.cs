// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.

using System;
using Microsoft.Performance.SDK.Processing;
using Microsoft.Performance.Testing;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace Microsoft.Performance.SDK.Tests
{
    [TestClass]
    [UnitTest]
    public class ColumnMetadataTests
    {
        private Guid ColumnGuid { get; set; } = Guid.NewGuid();

        [TestMethod]
        public void ConstantStringName_NameAlwaysReturnsSameValue()
        {
            var metadata = new ColumnMetadata(this.ColumnGuid, "name");

            for (var i = 0; i < 1000; ++i)
            {
                Assert.AreEqual("name", metadata.Name);
            }
        }

        [TestMethod]
        public void ConstantStringName_NameIsConstantReturnsTrue()
        {
            var metadata = new ColumnMetadata(this.ColumnGuid, "name");

            Assert.IsTrue(metadata.IsNameConstant);
        }

        [TestMethod]
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
        public void ConstantTitleProjection_NameIsConstantReturnsTrue()
        {
            var projection = Projection.Constant("name");
            var metadata = new ColumnMetadata(this.ColumnGuid, "default", projection, "test column");

            Assert.IsTrue(metadata.IsNameConstant);
        }

        [TestMethod]
        public void DynamicTitleProjection_NameIsConstantReturnsFalse()
        {
            var projection = Projection.CreateUsingFuncAdaptor<int, string>(i => i.ToString());

            var metadata = new ColumnMetadata(this.ColumnGuid, "default", projection, "test column");

            Assert.IsFalse(metadata.IsNameConstant);
        }

        [TestMethod]
        public void DynamicTitleProjection_ProjectorIsExposed()
        {
            var projection = Projection.CreateUsingFuncAdaptor<int, string>(i => i.ToString());

            var metadata = new ColumnMetadata(this.ColumnGuid, "default", projection, "test column");

            Assert.AreSame(projection, metadata.NameProjection);
        }

        [TestMethod]
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

        [TestMethod]
        public void CloneT_PreservesProperties()
        {
            var metadata = new ColumnMetadata(this.ColumnGuid, "name")
            {
                IsDeprecated = true,
            };

            ColumnMetadata clone = metadata.CloneT();

            Assert.IsTrue(clone.IsDeprecated);
        }

        [TestMethod]
        public void CopyConstructor_CopyProperties()
        {
            var metadata = new ColumnMetadata(this.ColumnGuid, "name")
            {
                IsDeprecated = true,
            };

            var copy = new ColumnMetadata(metadata);

            Assert.IsTrue(copy.IsDeprecated);
        }

        [TestMethod]
        public void NoDescription_ShortDescriptionIsNull()
        {
            var metadata = new ColumnMetadata(this.ColumnGuid, "name");

            Assert.AreEqual("name", metadata.Description);
            Assert.IsNull(metadata.ShortDescription);
        }

        [TestMethod]
        public void ShortDescriptionIsDerivedFromDescription()
        {
            var metadata = new ColumnMetadata(this.ColumnGuid, "name", "a real description");

            Assert.AreEqual("a real description", metadata.Description);
            Assert.AreEqual("a real description", metadata.ShortDescription);
        }

        [TestMethod]
        public void DescriptionOverThreshold_IsTruncatedWithEllipsis()
        {
            var description = new string('a', 51);
            var metadata = new ColumnMetadata(this.ColumnGuid, "name", description);

            Assert.AreEqual(description, metadata.Description);
            Assert.AreEqual(40, metadata.ShortDescription.Length);
            Assert.AreEqual(new string('a', 39) + "\u2026", metadata.ShortDescription);
        }

        [TestMethod]
        public void SettingShortDescription_PopulatesAbsentDescription()
        {
            var metadata = new ColumnMetadata(this.ColumnGuid, "name")
            {
                ShortDescription = "a tooltip",
            };

            Assert.AreEqual("a tooltip", metadata.ShortDescription);
            Assert.AreEqual("a tooltip", metadata.Description);
        }

        [TestMethod]
        public void SettingShortDescription_DoesNotOverwriteRealDescription()
        {
            var metadata = new ColumnMetadata(this.ColumnGuid, "name", "a real description")
            {
                ShortDescription = "short",
            };

            Assert.AreEqual("a real description", metadata.Description);
            Assert.AreEqual("short", metadata.ShortDescription);
        }

        [TestMethod]
        public void NullShortDescription_DoesNotClearDescription()
        {
            var metadata = new ColumnMetadata(this.ColumnGuid, "name", "long")
            {
                ShortDescription = null,
            };

            Assert.AreEqual("long", metadata.Description);
            Assert.AreEqual("long", metadata.ShortDescription);

            // Doesn't overwrite default description either
            var fallback = new ColumnMetadata(this.ColumnGuid, "name")
            {
                ShortDescription = null,
            };

            Assert.AreEqual("name", fallback.Description);
        }
    }
}
