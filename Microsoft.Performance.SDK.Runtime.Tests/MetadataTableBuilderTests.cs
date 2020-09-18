// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.

using System;
using System.Linq;
using Microsoft.Performance.SDK.Processing;
using Microsoft.Performance.Testing;
using Microsoft.Performance.Testing.SDK;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace Microsoft.Performance.SDK.Runtime.Tests
{
    [TestClass]
    public class MetadataTableBuilderTests
    {
        private TableDescriptor Descriptor { get; set; }

        private MetadataTableBuilder Sut { get; set; }

        [TestInitialize]
        public void Initialize()
        {
            this.Descriptor = Any.TableDescriptor();
            this.Sut = new MetadataTableBuilder(this.Descriptor);
        }

        [TestMethod]
        [UnitTest]
        public void ConstructorDoesNotAllowNull()
        {
            Assert.ThrowsException<ArgumentNullException>(
                () => new MetadataTableBuilder(null));
        }

        [TestMethod]
        [UnitTest]
        public void DescriptionProperlySet()
        {
            Assert.AreEqual(this.Descriptor.Description, this.Sut.Description);
        }

        [TestMethod]
        [UnitTest]
        public void GuidProperlySet()
        {
            Assert.AreEqual(this.Descriptor.Guid, this.Sut.Guid);
        }

        [TestMethod]
        [UnitTest]
        public void NameProperlySet()
        {
            Assert.AreEqual(this.Descriptor.Name, this.Sut.Name);
        }

        [TestMethod]
        [UnitTest]
        public void DescriptorProperlySet()
        {
            Assert.AreEqual(this.Descriptor, this.Sut.TableDescriptor);
        }

        [TestMethod]
        [UnitTest]
        public void WhenConstructedHasNoRowsOrColumns()
        {
            Assert.AreEqual(0, this.Sut.RowCount);
            Assert.AreEqual(0, this.Sut.Columns.Count());
        }

        [TestMethod]
        [UnitTest]
        public void SetRowCountSets()
        {
            this.Sut.SetRowCount(23);

            Assert.AreEqual(23, this.Sut.RowCount);
        }

        [TestMethod]
        [UnitTest]
        public void SetRowCountReturnsBuilder()
        {
            Assert.AreEqual(this.Sut, this.Sut.SetRowCount(23));
        }

        [TestMethod]
        [UnitTest]
        public void SetRowCountDoesNotAllowNegativeNumber()
        {
            Assert.ThrowsException<ArgumentOutOfRangeException>(
                () => this.Sut.SetRowCount(-1));
        }

        [TestMethod]
        [UnitTest]
        public void AddColumnDoesNotAllowNulls()
        {
            Assert.ThrowsException<ArgumentNullException>(
                () => this.Sut.AddColumn(null));
        }

        [TestMethod]
        [UnitTest]
        public void AddColumnAdds()
        {
            var column = new BaseDataColumn<string>(
                new ColumnMetadata(Guid.NewGuid(), "name"),
                new UIHints { Width = 100, },
                Projection.CreateUsingFuncAdaptor(i => "test"));

            this.Sut.AddColumn(column);

            Assert.IsTrue(this.Sut.Columns.Contains(column));
        }

        [TestMethod]
        [UnitTest]
        public void AddColumnReturnsBuilder()
        {
            var column = new BaseDataColumn<string>(
               new ColumnMetadata(Guid.NewGuid(), "name"),
               new UIHints { Width = 100, },
               Projection.CreateUsingFuncAdaptor(i => "test"));

            Assert.AreEqual(this.Sut, this.Sut.AddColumn(column));
        }
    }
}
