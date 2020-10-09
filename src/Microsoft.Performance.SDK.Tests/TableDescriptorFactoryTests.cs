// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.

using Microsoft.Performance.SDK.Extensibility;
using Microsoft.Performance.SDK.Processing;
using Microsoft.Performance.Testing;
using Microsoft.Performance.Testing.SDK;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using System;
using System.Reflection;

namespace Microsoft.Performance.SDK.Tests
{
    [TestClass]
    public class TableDescriptorFactoryTests
    {
        private static ISerializer serializer = new FakeSerializer();

        [TestMethod]
        [UnitTest]
        public void TryCreateFromNullFails()
        {
            var result = TableDescriptorFactory.TryCreate(null, serializer, out TableDescriptor descriptor);

            Assert.IsFalse(result);
            Assert.IsNull(descriptor);
        }

        [TestMethod]
        [UnitTest]
        public void TryCreateFromTypeWithoutTableAttributeFails()
        {
            var result = TableDescriptorFactory.TryCreate(typeof(DateTime), serializer, out TableDescriptor descriptor);

            Assert.IsFalse(result);
            Assert.IsNull(descriptor);
        }

        [TestMethod]
        [UnitTest]
        public void TryCreateTypeFromNonConcreteTypeFails()
        {
            var result = TableDescriptorFactory.TryCreate(typeof(AbstractType), serializer, out TableDescriptor descriptor);

            Assert.IsFalse(result);
            Assert.IsNull(descriptor);
        }

        [TestMethod]
        [UnitTest]
        public void TryCreateFromTypeWithAttributeAndInterfaceCreates()
        {
            var result = TableDescriptorFactory.TryCreate
                (typeof(TableWithNoColumns),
                serializer,
                out TableDescriptor descriptor);

            Assert.IsTrue(result);
            Assert.IsNotNull(descriptor);

            AssertAttributeTranslated(TableWithNoColumns.TableDescriptor, descriptor);
        }

        [TestMethod]
        [UnitTest]
        public void TryCreateFromTypeWithPrebuiltTableConfig()
        {
            var result = TableDescriptorFactory.TryCreate
                (typeof(TableWithPrebuiltConfiguration),
                serializer,
                out TableDescriptor descriptor);

            Assert.IsTrue(result);
            Assert.IsNotNull(descriptor);

            Assert.IsNotNull(descriptor.PrebuiltTableConfigurations);
        }

        [TestMethod]
        [UnitTest]
        public void TryCreateFromSubtypeOfTableAttributeCreates()
        {
            var result = TableDescriptorFactory.TryCreate
                (typeof(TableWithSubAttribute),
                serializer,
                out TableDescriptor descriptor);

            Assert.IsTrue(result);
            Assert.IsNotNull(descriptor);

            AssertAttributeTranslated(TableWithSubAttribute.Descriptor, descriptor);
        }

        private static void AssertAttributeTranslated(
            TableDescriptor original,
            TableDescriptor returned)
        {
            Assert.IsNotNull(original);
            Assert.IsNotNull(returned);

            Assert.AreEqual(original, returned);
        }

        private static ColumnConfiguration GetPrivateColumnField(
            Type type,
            string name)
        {
            var field = type.GetField(
                name,
                BindingFlags.Static | BindingFlags.NonPublic);
            return (field?.GetValue(null) as ColumnConfiguration);
        }

        [Table]
        private sealed class TableWithNoColumns
        {
            public bool TryCreateTable(ITableBuilder tableBuilder)
            {
                throw new NotImplementedException();
            }

            public static TableDescriptor TableDescriptor { get; } = new TableDescriptor(
                Guid.Parse("{F3F7B534-5DC5-40FB-93D9-07FDAC073A13}"),
                "Name",
                "Description",
                "Category",
                false,
                TableLayoutStyle.Table);

            public static void BuildTable(ITableBuilder tableBuilder, IDataExtensionRetrieval cookedData)
            {
                throw new NotImplementedException();
            }
        }

        [Table]
        [PrebuiltConfigurationsEmbeddedResource("Microsoft.Performance.SDK.Tests.Resources.TestTableConfigurations.json")]
        private sealed class TableWithPrebuiltConfiguration
        {
            public bool TryCreateTable(ITableBuilder tableBuilder)
            {
                throw new NotImplementedException();
            }

            public static TableDescriptor TableDescriptor { get; } = new TableDescriptor(
                Guid.Parse("{F3F7B534-5DC5-40FB-93D9-07FDAC073A13}"),
                "Name",
                "Description",
                "Category");

            public static void Builder(ITableBuilder tableBuilder, IDataExtensionRetrieval cookedData)
            {
                throw new NotImplementedException();
            }
        }

        private sealed class SubTableAttribute
            : TableAttribute
        {
            public SubTableAttribute(
                string tableDesriptorProperty,
                string tableBuilderMethodName)
                : base(tableDesriptorProperty, tableBuilderMethodName)
            {
            }
        }

        [SubTable("Descriptor", "Builder")]
        private sealed class TableWithSubAttribute
        {
            public bool TryCreateTable(ITableBuilder tableBuilder)
            {
                throw new NotImplementedException();
            }

            public static TableDescriptor Descriptor { get; } = new TableDescriptor(
                Guid.Parse("{416A5545-941E-461B-9CB0-04446373AE08}"),
                "SubTableAttribute",
                "Description");

            public static void Builder(ITableBuilder tableBuilder, IDataExtensionRetrieval cookedData)
            {
                throw new NotImplementedException();
            }
        }

        [Table]
        private sealed class MissingInterface
        {
            public static TableDescriptor TableDescriptor { get; } = new TableDescriptor(
                Guid.Parse("{F3F7B534-5DC5-40FB-93D9-07FDAC073A13}"),
                "Name",
                "Description",
                "Category");
        }

        [Table]
        private abstract class AbstractType
        {
            public bool TryCreateTable(ITableBuilder tableBuilder)
            {
                throw new NotImplementedException();
            }

            public static TableDescriptor TableDescriptor { get; } = new TableDescriptor(
                Guid.Parse("{F3F7B534-5DC5-40FB-93D9-07FDAC073A13}"),
                "Name",
                "Description",
                "Category");
        }

        [Table]
        private sealed class TableWithOnlyInaccessibleColumns
        {
            private static readonly ColumnConfiguration ColumnOne =
                new ColumnConfiguration(new ColumnMetadata(Guid.NewGuid(), "One"));

            private static readonly ColumnConfiguration ColumnTwo =
                new ColumnConfiguration(new ColumnMetadata(Guid.NewGuid(), "Two"));

            private static readonly ColumnConfiguration ColumnThree =
                new ColumnConfiguration(new ColumnMetadata(Guid.NewGuid(), "Three"));

            public bool TryCreateTable(ITableBuilder tableBuilder)
            {
                throw new NotImplementedException();
            }

            public static TableDescriptor TableDescriptor { get; } = new TableDescriptor(
                Guid.Parse("{F3F7B534-5DC5-40FB-93D9-07FDAC073A13}"),
                "Name",
                "Description",
                "Category");
        }

        [Table]
        private sealed class TableWithOnlyPublicColumns
        {
            public static readonly ColumnConfiguration ColumnOne =
                new ColumnConfiguration(new ColumnMetadata(Guid.NewGuid(), "One"));

            public static readonly ColumnConfiguration ColumnTwo =
                new ColumnConfiguration(new ColumnMetadata(Guid.NewGuid(), "Two"));

            public static readonly ColumnConfiguration ColumnThree =
                new ColumnConfiguration(new ColumnMetadata(Guid.NewGuid(), "Three"));

            public bool TryCreateTable(ITableBuilder tableBuilder)
            {
                throw new NotImplementedException();
            }

            public static TableDescriptor TableDescriptor = new TableDescriptor(
                Guid.Parse("{F3F7B534-5DC5-40FB-93D9-07FDAC073A13}"),
                "Name",
                "Description",
                "Category");
        }

        [Table]
        private sealed class TableWithNonStaticColumns
        {
            private readonly ColumnConfiguration ColumnOne =
                new ColumnConfiguration(new ColumnMetadata(Guid.NewGuid(), "One"));

            private readonly ColumnConfiguration ColumnTwo =
                new ColumnConfiguration(new ColumnMetadata(Guid.NewGuid(), "Two"));

            private readonly ColumnConfiguration ColumnThree =
                new ColumnConfiguration(new ColumnMetadata(Guid.NewGuid(), "Three"));

            public bool TryCreateTable(ITableBuilder tableBuilder)
            {
                throw new NotImplementedException();
            }

            public static TableDescriptor TableDescriptor { get; } = new TableDescriptor(
                Guid.Parse("{F3F7B534-5DC5-40FB-93D9-07FDAC073A13}"),
                "Name",
                "Description",
                "Category");
        }
    }
}
