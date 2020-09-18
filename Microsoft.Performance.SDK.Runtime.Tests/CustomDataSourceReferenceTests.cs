// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.

using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using Microsoft.Performance.SDK.Processing;
using Microsoft.Performance.Testing;
using Microsoft.Performance.Testing.SDK;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using DataSourceAttribute = Microsoft.Performance.SDK.Processing.DataSourceAttribute;

namespace Microsoft.Performance.SDK.Runtime.Tests
{
    [TestClass]
    public class CustomDataSourceReferenceTests
    {
        [TestMethod]
        [UnitTest]
        public void TryCreateReferenceForNullTypeThrows()
        {
            Assert.ThrowsException<ArgumentNullException>(
                () => CustomDataSourceReference.TryCreateReference(
                    null,
                    out CustomDataSourceReference _));
        }

        [TestMethod]
        [UnitTest]
        public void TryCreateReferenceForNoAttributesFails()
        {
            RunCreateFailTest(typeof(CdsWithNoAttributes));
        }

        [TestMethod]
        [UnitTest]
        public void TryCreateReferenceForMissingCdsAttributeFails()
        {
            RunCreateFailTest(typeof(CdsWithoutCdsAttribute));
        }

        [TestMethod]
        [UnitTest]
        public void TryCreateReferenceForMissingDataSourceAttributeFails()
        {
            RunCreateFailTest(typeof(CdsWithoutDataSourceAttribute));
        }

        [TestMethod]
        [UnitTest]
        public void TryCreateReferenceForNoInterfaceImplementationFails()
        {
            RunCreateFailTest(typeof(CdsWithoutInterface));
        }

        [TestMethod]
        [UnitTest]
        public void TryCreateReferenceForInaccessibleConstructorFails()
        {
            RunCreateFailTest(typeof(CdsWithInaccessibleConstructor));
        }

        [TestMethod]
        [UnitTest]
        public void TryCreateReferenceWithoutParameterlessConstructorFails()
        {
            RunCreateFailTest(typeof(CdsWithParameterizedConstructor));
        }

        [TestMethod]
        [UnitTest]
        public void TryCreateReferenceForInaccessibleTypeFails()
        {
            RunCreateFailTest(typeof(ProtectedType));
        }

        [TestMethod]
        [UnitTest]
        public void TryCreateReferenceNominal()
        {
            RunCreateSuccessTest(typeof(SampleCds));
        }

        [TestMethod]
        [UnitTest]
        public void TryCreateReferenceNestedPublicSucceeds()
        {
            RunCreateSuccessTest(typeof(NestedPublicType));
        }

        [TestMethod]
        [UnitTest]
        public void CloneClones()
        {
            var result = CustomDataSourceReference.TryCreateReference(
                typeof(SampleCds),
                out CustomDataSourceReference reference);
            Assert.IsTrue(result);
            Assert.IsNotNull(reference);

            var clone = reference.CloneT();

            Assert.IsNotNull(clone);
            Assert.AreEqual(reference.AssemblyPath, clone.AssemblyPath);
            Assert.AreEqual(reference.DataSource, clone.DataSource);
            Assert.AreEqual(reference.Description, clone.Description);
            Assert.AreEqual(reference.Guid, clone.Guid);
            Assert.AreEqual(reference.Name, clone.Name);
            Assert.AreEqual(reference.Type, clone.Type);

            Assert.IsTrue(reference.AvailableTables.All(x => clone.AvailableTables.Contains(x)));
            Assert.IsTrue(clone.AvailableTables.All(x => reference.AvailableTables.Contains(x)));
        }

        private static void RunCreateSuccessTest(Type type)
        {
            var result = CustomDataSourceReference.TryCreateReference(
                type,
                out CustomDataSourceReference reference);

            Assert.IsTrue(result);
            Assert.IsNotNull(reference);

            var metadata = type.GetCustomAttribute<CustomDataSourceAttribute>();
            var dataSource = type.GetCustomAttribute<DataSourceAttribute>();
            var tables = ((ICustomDataSource)Activator.CreateInstance(type)).DataTables;
            Assert.IsNotNull(metadata);
            Assert.IsNotNull(dataSource);
            Assert.IsNotNull(tables);

            Assert.AreEqual(type.Assembly.Location, reference.AssemblyPath);
            Assert.AreEqual(metadata.Description, reference.Description);
            Assert.AreEqual(metadata.Guid, reference.Guid);
            Assert.AreEqual(metadata.Name, reference.Name);
            Assert.AreEqual(type, reference.Type);
            Assert.AreEqual(dataSource, reference.DataSource);
            Assert.AreEqual(tables.Count(), reference.AvailableTables.Count());
            Assert.IsTrue(tables.All(x => reference.AvailableTables.Contains(x)));
        }

        private static void RunCreateFailTest(Type type)
        {
            var result = CustomDataSourceReference.TryCreateReference(
                type,
                out CustomDataSourceReference reference);

            Assert.IsFalse(result);
            Assert.IsNull(reference);
        }

        [CustomDataSource("{8BAACFC9-CCBD-4856-A705-CA4C1CE28533}", "What", "Test")]
        [FileDataSource("ext")]
        protected class ProtectedType
            : ICustomDataSource
        {
            public IEnumerable<TableDescriptor> DataTables => throw new NotImplementedException();

            public IEnumerable<TableDescriptor> MetadataTables { get; }

            public IEnumerable<Option> CommandLineOptions => Enumerable.Empty<Option>();

            public void SetApplicationEnvironment(IApplicationEnvironment applicationEnvironment)
            {
                throw new NotImplementedException();
            }

            public ICustomDataProcessor CreateProcessor(IDataSource dataSource, IProcessorEnvironment processorEnvironment, ProcessorOptions options)
            {
                throw new NotImplementedException();
            }

            public ICustomDataProcessor CreateProcessor(IEnumerable<IDataSource> dataSources, IProcessorEnvironment processorEnvironment, ProcessorOptions options)
            {
                throw new NotImplementedException();
            }

            public Stream GetSerializationStream(SerializationSource source)
            {
                throw new NotImplementedException();
            }

            public IEnumerable<TableConfiguration> GetTableConfigurations(TableDescriptor descriptor)
            {
                throw new NotImplementedException();
            }

            public CustomDataSourceInfo GetAboutInfo()
            {
                throw new NotImplementedException();
            }

            public bool IsFileSupported(string path)
            {
                throw new NotImplementedException();
            }

            public void SetLogger(ILogger logger)
            {
                throw new NotImplementedException();
            }
        }

        [CustomDataSource("{29EF5347-53FD-49A0-8A03-C0262DE07BD4}", "What", "Test")]
        [FileDataSource("ext")]
        public class NestedPublicType
            : ICustomDataSource
        {
            public IEnumerable<TableDescriptor> DataTables => new TableDescriptor[0];

            public IEnumerable<TableDescriptor> MetadataTables { get; }

            public IEnumerable<Option> CommandLineOptions => Enumerable.Empty<Option>();

            public void SetApplicationEnvironment(IApplicationEnvironment applicationEnvironment)
            {
                throw new NotImplementedException();
            }

            public ICustomDataProcessor CreateProcessor(IDataSource dataSource, IProcessorEnvironment processorEnvironment, ProcessorOptions options)
            {
                throw new NotImplementedException();
            }

            public ICustomDataProcessor CreateProcessor(IEnumerable<IDataSource> dataSources, IProcessorEnvironment processorEnvironment, ProcessorOptions options)
            {
                throw new NotImplementedException();
            }

            public Stream GetSerializationStream(SerializationSource source)
            {
                throw new NotImplementedException();
            }

            public IEnumerable<TableConfiguration> GetTableConfigurations(TableDescriptor descriptor)
            {
                throw new NotImplementedException();
            }

            public CustomDataSourceInfo GetAboutInfo()
            {
                throw new NotImplementedException();
            }

            public bool IsFileSupported(string path)
            {
                throw new NotImplementedException();
            }

            public void SetLogger(ILogger logger)
            {
                throw new NotImplementedException();
            }
        }

        [CustomDataSource("{2D5E3373-88DA-4640-BD19-99FA8C437EB1}", "What", "Test")]
        [FileDataSource("ext")]
        public class SampleCds
            : ICustomDataSource
        {
            private static readonly TableDescriptor[] tableDescriptors = new[]
            {
                Any.TableDescriptor(),
                Any.TableDescriptor(),
            };

            public SampleCds()
            {
                this.DataTables = tableDescriptors;
            }

            public IEnumerable<TableDescriptor> DataTables { get; }

            public IEnumerable<TableDescriptor> MetadataTables { get; }

            public IEnumerable<Option> CommandLineOptions => Enumerable.Empty<Option>();

            public void SetApplicationEnvironment(IApplicationEnvironment applicationEnvironment)
            {
                throw new NotImplementedException();
            }

            public ICustomDataProcessor CreateProcessor(IDataSource dataSource, IProcessorEnvironment processorEnvironment, ProcessorOptions options)
            {
                throw new NotImplementedException();
            }

            public ICustomDataProcessor CreateProcessor(IEnumerable<IDataSource> dataSources, IProcessorEnvironment processorEnvironment, ProcessorOptions options)
            {
                throw new NotImplementedException();
            }

            public Stream GetSerializationStream(SerializationSource source)
            {
                throw new NotImplementedException();
            }

            public IEnumerable<TableConfiguration> GetTableConfigurations(TableDescriptor descriptor)
            {
                throw new NotImplementedException();
            }

            public CustomDataSourceInfo GetAboutInfo()
            {
                throw new NotImplementedException();
            }

            public bool IsFileSupported(string path)
            {
                throw new NotImplementedException();
            }

            public void SetLogger(ILogger logger)
            {
                throw new NotImplementedException();
            }
        }
    }
}
