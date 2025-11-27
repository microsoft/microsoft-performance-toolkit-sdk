// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.

using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.Performance.SDK.Options.Definitions;
using Microsoft.Performance.SDK.Processing;
using Microsoft.Performance.SDK.Processing.DataSourceGrouping;
using Microsoft.Performance.Testing;
using Microsoft.Performance.Testing.SDK;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using DataSourceAttribute = Microsoft.Performance.SDK.Processing.DataSourceAttribute;

namespace Microsoft.Performance.SDK.Runtime.Tests
{
    [TestClass]
    public class ProcessingSourceReferenceTests
    {
        [TestMethod]
        [UnitTest]
        public void TryCreateReferenceForNullTypeThrows()
        {
            Assert.ThrowsException<ArgumentNullException>(
                () => ProcessingSourceReference.TryCreateReference(
                    null,
                    out ProcessingSourceReference _));
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
        public void TryCreateReferenceMultipleDataSourcesSucceeds()
        {
            RunCreateSuccessTest(typeof(MultiDataSourceCds));
        }

        [TestMethod]
        [UnitTest]
        public void CloneClones()
        {
            var result = ProcessingSourceReference.TryCreateReference(
                typeof(SampleCds),
                out ProcessingSourceReference reference);
            Assert.IsTrue(result);
            Assert.IsNotNull(reference);

            var clone = reference.CloneT();

            Assert.IsNotNull(clone);
            Assert.AreEqual(reference.AssemblyPath, clone.AssemblyPath);
            CollectionAssert.AreEquivalent(reference.DataSources.ToList(), clone.DataSources.ToList());
            Assert.AreEqual(reference.Description, clone.Description);
            Assert.AreEqual(reference.Guid, clone.Guid);
            Assert.AreEqual(reference.Name, clone.Name);
            Assert.AreEqual(reference.Type, clone.Type);

            Assert.IsTrue(reference.AvailableTables.All(x => clone.AvailableTables.Contains(x)));
            Assert.IsTrue(clone.AvailableTables.All(x => reference.AvailableTables.Contains(x)));
        }

        [TestMethod]
        [UnitTest]
        public void WhenDisposed_EverythingThrows()
        {
            ProcessingSourceReference sut = null;
            try
            {
                var result = ProcessingSourceReference.TryCreateReference(
                    typeof(SampleCds),
                    out sut);
                Assert.IsTrue(result);

                sut.Dispose();

                Assert.ThrowsException<ObjectDisposedException>(() => sut.AssemblyPath);
                Assert.ThrowsException<ObjectDisposedException>(() => sut.AvailableTables);
                Assert.ThrowsException<ObjectDisposedException>(() => sut.CommandLineOptions);
                Assert.ThrowsException<ObjectDisposedException>(() => sut.TrackedProcessors);
                Assert.ThrowsException<ObjectDisposedException>(() => sut.DataSources);
                Assert.ThrowsException<ObjectDisposedException>(() => sut.Description);
                Assert.ThrowsException<ObjectDisposedException>(() => sut.Guid);
                Assert.ThrowsException<ObjectDisposedException>(() => sut.Instance);
                Assert.ThrowsException<ObjectDisposedException>(() => sut.Name);
                Assert.ThrowsException<ObjectDisposedException>(() => sut.Type);
                Assert.ThrowsException<ObjectDisposedException>(() => sut.Version);

                Assert.ThrowsException<ObjectDisposedException>(() => sut.AreDirectoriesSupported());
                Assert.ThrowsException<ObjectDisposedException>(() => sut.AreExtensionlessFilesSupported());
                Assert.ThrowsException<ObjectDisposedException>(() => sut.Clone());
                Assert.ThrowsException<ObjectDisposedException>(() => sut.CloneT());
                Assert.ThrowsException<ObjectDisposedException>(() => sut.CreateProcessor((IDataSourceGroup)null, null, null));
                Assert.ThrowsException<ObjectDisposedException>(() => sut.Supports(Any.DataSource()));
                Assert.ThrowsException<ObjectDisposedException>(() => sut.TryGetCanonicalFileExtensions());
                Assert.ThrowsException<ObjectDisposedException>(() => sut.TryGetDirectoryDescription());
                Assert.ThrowsException<ObjectDisposedException>(() => sut.TryGetExtensionlessFileDescription());
                Assert.ThrowsException<ObjectDisposedException>(() => sut.TryGetFileDescription(".txt"));
                Assert.ThrowsException<ObjectDisposedException>(() => sut.TryGetFileExtensions());
            }
            finally
            {
                sut?.Dispose();
            }
        }

        [TestMethod]
        [UnitTest]
        public void CanDisposeMultipleTimes()
        {
            ProcessingSourceReference sut = null;
            try
            {
                var result = ProcessingSourceReference.TryCreateReference(
                    typeof(SampleCds),
                    out sut);
                Assert.IsTrue(result);

                sut.Dispose();
                sut.Dispose();
                sut.Dispose();
            }
            finally
            {
                sut?.Dispose();
            }
        }

        [TestMethod]
        [UnitTest]
        public void WhenDisposed_InstanceDisposed()
        {
            ProcessingSourceReference sut = null;
            try
            {
                var result = ProcessingSourceReference.TryCreateReference(
                    typeof(DisposableProcessingSource),
                    out sut);
                Assert.IsTrue(result);

                var instance = sut.Instance as DisposableProcessingSource;
                Assert.IsNotNull(instance);
                Assert.AreEqual(0, instance.DisposeCalls);

                sut.Dispose();

                Assert.AreEqual(1, instance.DisposeCalls);
            }
            finally
            {
                sut?.Dispose();
            }
        }

        [TestMethod]
        [UnitTest]
        public void WhenDisposed_CreatedProcessorsPassedToCdsForCleanup()
        {
            ProcessingSourceReference sut = null;
            try
            {
                var result = ProcessingSourceReference.TryCreateReference(
                    typeof(DisposableProcessingSource),
                    out sut);
                Assert.IsTrue(result);

                var instance = sut.Instance as DisposableProcessingSource;
                Assert.IsNotNull(instance);
                Assert.AreEqual(0, instance.DisposeCalls);

                var processors = new ICustomDataProcessor[]
                {
                    new FakeCustomDataProcessor(),
                    new FakeCustomDataProcessor(),
                    new DisposableCustomDataProcessor(),
                };

                var processorIndex = 0;
                instance.ProcessorCreateFactory = () =>
                {
                    return processors[processorIndex++];
                };

                for (var i = 0; i < processors.Length; ++i)
                {
                    sut.CreateProcessor((IDataSourceGroup)null, null, null);
                }

                sut.Dispose();

                CollectionAssert.AreEquivalent(
                    processors,
                    instance.ProcessorsDisposed);

                Assert.IsTrue(processors.OfType<DisposableCustomDataProcessor>().All(x => x.DisposeCalls > 0));
            }
            finally
            {
                sut?.Dispose();
            }
        }

        [TestMethod]
        [UnitTest]
        public void CreateProcessor_CreatedProcessorTracked()
        {
            var fakeProcessor = new FakeCustomDataProcessor();

            var result = ProcessingSourceReference.TryCreateReference(
                typeof(DisposableProcessingSource),
                out var sut);

            Assert.IsTrue(result);
            Assert.IsNotNull(sut);
            Assert.IsFalse(sut.TrackedProcessors.Any());

            var instance = sut.Instance as DisposableProcessingSource;
            Assert.IsNotNull(instance);
            instance.ProcessorCreateFactory = () => fakeProcessor;

            var p = sut.CreateProcessor(null, null, null);

            Assert.AreEqual(fakeProcessor, p);
            Assert.AreEqual(fakeProcessor, sut.TrackedProcessors.Single());
        }

        [TestMethod]
        [UnitTest]
        public void Supports_AttributeSaysNo_ReturnsFalseWithoutDelegating()
        {
            var cds = new FakeProcessingSource();
            var dataSource = new FileDataSource("test.txt");

            var cdsr = new ProcessingSourceReference(
                typeof(FakeProcessingSource),
                () => cds,
                Any.ProcessingSourceAttribute(),
                new HashSet<DataSourceAttribute>
                {
                    new FileDataSourceAttribute(".csv"),
                    new FileDataSourceAttribute(".etl"),
                });

            Assert.IsFalse(cdsr.Supports(dataSource));
            Assert.AreEqual(0, cds.IsDataSourceSupportedCalls.Count);
        }

        [TestMethod]
        [UnitTest]
        public void Supports_AtLeastOneAttributeSaysYes_Delegates()
        {
            var cds = new FakeProcessingSource();

            var cdsr = new ProcessingSourceReference(
                typeof(FakeProcessingSource),
                () => cds,
                Any.ProcessingSourceAttribute(),
                new HashSet<DataSourceAttribute>
                {
                    new FileDataSourceAttribute(".csv"),
                    new FileDataSourceAttribute(".txt"),
                    new FileDataSourceAttribute(".etl"),
                });

            var dataSource = new FileDataSource("test.txt");
            cds.IsDataSourceSupportedReturnValue[dataSource] = true;

            Assert.IsTrue(cdsr.Supports(dataSource));
            Assert.AreEqual(1, cds.IsDataSourceSupportedCalls.Count);
            Assert.AreEqual(dataSource, cds.IsDataSourceSupportedCalls[0]);
        }

        [TestMethod]
        [UnitTest]
        public void Supports_NoAttributes_ReturnsFalse()
        {
            var cds = new FakeProcessingSource();

            var cdsr = new ProcessingSourceReference(
                typeof(FakeProcessingSource),
                () => cds,
                Any.ProcessingSourceAttribute(),
                new HashSet<DataSourceAttribute>());

            var dataSource = new FileDataSource("test.txt");

            Assert.IsFalse(cdsr.Supports(dataSource));
            Assert.AreEqual(0, cds.IsDataSourceSupportedCalls.Count);
        }

        [TestMethod]
        [UnitTest]
        public void Supports_Throws_ReturnsFalse()
        {
            var cds = new FakeProcessingSource
            {
                IsDataSourceSupportedError = new ArithmeticException(),
            };

            var cdsr = new ProcessingSourceReference(
                typeof(FakeProcessingSource),
                () => cds,
                Any.ProcessingSourceAttribute(),
                new HashSet<DataSourceAttribute>
                {
                    new FileDataSourceAttribute(".csv"),
                    new FileDataSourceAttribute(".txt"),
                    new FileDataSourceAttribute(".etl"),
                });

            var dataSource = new FileDataSource("test.txt");

            Assert.IsFalse(cdsr.Supports(dataSource));
        }

        private static void RunCreateSuccessTest(Type type)
        {
            var result = ProcessingSourceReference.TryCreateReference(
                type,
                out ProcessingSourceReference reference);

            Assert.IsTrue(result);
            Assert.IsNotNull(reference);

            var metadata = type.GetCustomAttribute<ProcessingSourceAttribute>();
            var dataSources = type.GetCustomAttributes<DataSourceAttribute>();
            var instance = ((IProcessingSource)Activator.CreateInstance(type));
            var tables = instance.DataTables.Union(instance.MetadataTables);

            Assert.IsNotNull(metadata);
            Assert.IsNotNull(dataSources);
            Assert.IsNotNull(tables);

            Assert.AreEqual(type.Assembly.Location, reference.AssemblyPath);
            Assert.AreEqual(metadata.Description, reference.Description);
            Assert.AreEqual(metadata.Guid, reference.Guid);
            Assert.AreEqual(metadata.Name, reference.Name);
            Assert.AreEqual(type, reference.Type);
            CollectionAssert.AreEquivalent(dataSources.ToList(), reference.DataSources.ToList());
            Assert.AreEqual(tables.Count(), reference.AvailableTables.Count());
            Assert.IsTrue(tables.All(x => reference.AvailableTables.Contains(x)));
        }

        private static void RunCreateFailTest(Type type)
        {
            var result = ProcessingSourceReference.TryCreateReference(
                type,
                out ProcessingSourceReference reference);

            Assert.IsFalse(result);
            Assert.IsNull(reference);
        }

        [ProcessingSource("{8BAACFC9-CCBD-4856-A705-CA4C1CE28533}", "What", "Test")]
        [FileDataSource("ext")]
        protected class ProtectedType
            : IProcessingSource
        {
            public IEnumerable<TableDescriptor> DataTables => throw new NotImplementedException();

            public IEnumerable<TableDescriptor> MetadataTables { get; }

            public IEnumerable<Option> CommandLineOptions => Enumerable.Empty<Option>();

            public IEnumerable<PluginOptionDefinition> PluginOptions => Enumerable.Empty<PluginOptionDefinition>();

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

            public ICustomDataProcessor CreateProcessor(IDataSourceGroup dataSourceGroup, IProcessorEnvironment processorEnvironment,
                ProcessorOptions options)
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

            public ProcessingSourceInfo GetAboutInfo()
            {
                throw new NotImplementedException();
            }

            public void SetLogger(ILogger logger)
            {
                throw new NotImplementedException();
            }

            public bool IsDataSourceSupported(IDataSource dataSource)
            {
                throw new NotImplementedException();
            }

            public void DisposeProcessor(ICustomDataProcessor processor)
            {
                throw new NotImplementedException();
            }
        }

        [ProcessingSource("{29EF5347-53FD-49A0-8A03-C0262DE07BD4}", "What", "Test")]
        [FileDataSource("ext")]
        public class NestedPublicType
            : IProcessingSource
        {
            public IEnumerable<TableDescriptor> DataTables => new TableDescriptor[0];

            public IEnumerable<TableDescriptor> MetadataTables => new TableDescriptor[0];

            public IEnumerable<Option> CommandLineOptions => Enumerable.Empty<Option>();

            public IEnumerable<PluginOptionDefinition> PluginOptions => Enumerable.Empty<PluginOptionDefinition>();

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

            public ICustomDataProcessor CreateProcessor(IDataSourceGroup dataSourceGroup, IProcessorEnvironment processorEnvironment,
                ProcessorOptions options)
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

            public ProcessingSourceInfo GetAboutInfo()
            {
                throw new NotImplementedException();
            }

            public void SetLogger(ILogger logger)
            {
                throw new NotImplementedException();
            }

            public bool IsDataSourceSupported(IDataSource dataSource)
            {
                throw new NotImplementedException();
            }

            public void DisposeProcessor(ICustomDataProcessor processor)
            {
                throw new NotImplementedException();
            }
        }

        [ProcessingSource("{2D5E3373-88DA-4640-BD19-99FA8C437EB1}", "What", "Test")]
        [FileDataSource(Extension)]
        public class SampleCds
            : IProcessingSource
        {
            public const string Extension = "ext";

            private static readonly TableDescriptor[] tableDescriptors = new[]
            {
                Any.TableDescriptor(),
                Any.TableDescriptor(),
            };

            private static readonly TableDescriptor[] metadataTableDescriptors = new[]
            {
                Any.TableDescriptor(),
                Any.TableDescriptor(),
            };

            public SampleCds()
            {
                this.DataTables = tableDescriptors;
                this.MetadataTables = metadataTableDescriptors;
            }

            public IEnumerable<TableDescriptor> DataTables { get; }

            public IEnumerable<TableDescriptor> MetadataTables { get; }

            public IEnumerable<Option> CommandLineOptions => Enumerable.Empty<Option>();

            public IEnumerable<PluginOptionDefinition> PluginOptions => Enumerable.Empty<PluginOptionDefinition>();

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

            public ICustomDataProcessor CreateProcessor(IDataSourceGroup dataSourceGroup, IProcessorEnvironment processorEnvironment,
                ProcessorOptions options)
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

            public ProcessingSourceInfo GetAboutInfo()
            {
                throw new NotImplementedException();
            }

            public void SetLogger(ILogger logger)
            {
                throw new NotImplementedException();
            }

            public bool IsDataSourceSupported(IDataSource dataSource)
            {
                throw new NotImplementedException();
            }

            public void DisposeProcessor(ICustomDataProcessor processor)
            {
                throw new NotImplementedException();
            }
        }

        [ProcessingSource("{2D5E3373-88DA-4640-BD19-99FA8C437EB1}", "What", "Test")]
        [FileDataSource("ext1")]
        [FileDataSource("ext2")]
        [ExtensionlessFileDataSource("No description")]
        [DirectoryDataSource("No description")]
        public class MultiDataSourceCds
            : IProcessingSource
        {
            private static readonly TableDescriptor[] tableDescriptors = new[]
            {
                Any.TableDescriptor(),
                Any.TableDescriptor(),
            };

            private static readonly TableDescriptor[] metadataTableDescriptors = new[]
            {
                Any.TableDescriptor(),
                Any.TableDescriptor(),
            };

            public MultiDataSourceCds()
            {
                this.DataTables = tableDescriptors;
                this.MetadataTables = metadataTableDescriptors;
            }

            public IEnumerable<TableDescriptor> DataTables { get; }

            public IEnumerable<TableDescriptor> MetadataTables { get; }

            public IEnumerable<Option> CommandLineOptions => Enumerable.Empty<Option>();

            public IEnumerable<PluginOptionDefinition> PluginOptions => Enumerable.Empty<PluginOptionDefinition>();

            public ICustomDataProcessor CreateProcessor(IDataSource dataSource, IProcessorEnvironment processorEnvironment, ProcessorOptions options)
            {
                throw new NotImplementedException();
            }

            public ICustomDataProcessor CreateProcessor(IEnumerable<IDataSource> dataSources, IProcessorEnvironment processorEnvironment, ProcessorOptions options)
            {
                throw new NotImplementedException();
            }

            public ICustomDataProcessor CreateProcessor(IDataSourceGroup dataSourceGroup, IProcessorEnvironment processorEnvironment,
                ProcessorOptions options)
            {
                throw new NotImplementedException();
            }

            public void DisposeProcessor(ICustomDataProcessor processor)
            {
                throw new NotImplementedException();
            }

            public ProcessingSourceInfo GetAboutInfo()
            {
                throw new NotImplementedException();
            }

            public Stream GetSerializationStream(SerializationSource source)
            {
                throw new NotImplementedException();
            }

            public bool IsDataSourceSupported(IDataSource dataSource)
            {
                throw new NotImplementedException();
            }

            public void SetApplicationEnvironment(IApplicationEnvironment applicationEnvironment)
            {
                throw new NotImplementedException();
            }

            public void SetLogger(ILogger logger)
            {
                throw new NotImplementedException();
            }
        }

        [ProcessingSource("{2D5E3373-88DA-4640-BD19-99FA8C437EB1}", "What", "Test")]
        [FileDataSource("ext")]
        public class DisposableProcessingSource
            : IProcessingSource,
              IDisposable
        {
            public DisposableProcessingSource()
            {
                this.DataTables = Enumerable.Empty<TableDescriptor>();
                this.MetadataTables = Enumerable.Empty<TableDescriptor>();
                this.CommandLineOptions = Enumerable.Empty<Option>();
            }

            public IEnumerable<TableDescriptor> DataTables { get; set; }

            public IEnumerable<TableDescriptor> MetadataTables { get; set; }

            public IEnumerable<Option> CommandLineOptions { get; set; }

            public IEnumerable<PluginOptionDefinition> PluginOptions { get; set; }

            public ICustomDataProcessor CreateProcessor(IDataSource dataSource, IProcessorEnvironment processorEnvironment, ProcessorOptions options)
            {
                return this.CreateProcessor(new[] { dataSource, }, processorEnvironment, options);
            }

            public Func<ICustomDataProcessor> ProcessorCreateFactory { get; set; }
            public ICustomDataProcessor CreateProcessor(IEnumerable<IDataSource> dataSources, IProcessorEnvironment processorEnvironment, ProcessorOptions options)
            {
                return this.ProcessorCreateFactory();
            }

            public ICustomDataProcessor CreateProcessor(IDataSourceGroup dataSourceGroup, IProcessorEnvironment processorEnvironment,
                ProcessorOptions options)
            {
                return this.ProcessorCreateFactory();
            }

            public int DisposeCalls { get; private set; }

            public void Dispose()
            {
                ++this.DisposeCalls;
            }

            public ProcessingSourceInfo GetAboutInfo()
            {
                throw new NotImplementedException();
            }

            public Stream GetSerializationStream(SerializationSource source)
            {
                throw new NotImplementedException();
            }

            public bool IsDataSourceSupported(IDataSource dataSource)
            {
                throw new NotImplementedException();
            }

            public void SetApplicationEnvironment(IApplicationEnvironment applicationEnvironment)
            {
                throw new NotImplementedException();
            }

            public void SetLogger(ILogger logger)
            {
                throw new NotImplementedException();
            }

            public List<ICustomDataProcessor> ProcessorsDisposed { get; set; }
            public void DisposeProcessor(ICustomDataProcessor processor)
            {
                if (this.ProcessorsDisposed is null)
                {
                    this.ProcessorsDisposed = new List<ICustomDataProcessor>
                    {
                        processor,
                    };
                }
                else
                {
                    this.ProcessorsDisposed.Add(processor);
                }
            }
        }

        public sealed class DisposableCustomDataProcessor
            : ICustomDataProcessor,
              IDisposable
        {
            public void BuildTable(TableDescriptor table, ITableBuilder tableBuilder)
            {
                throw new NotImplementedException();
            }

            public ITableService CreateTableService(TableDescriptor table)
            {
                throw new NotImplementedException();
            }

            public int DisposeCalls { get; set; }
            public void Dispose()
            {
                ++this.DisposeCalls;
            }

            public bool DoesTableHaveData(TableDescriptor table)
            {
                throw new NotImplementedException();
            }

            public void EnableTable(TableDescriptor tableDescriptor)
            {
                throw new NotImplementedException();
            }

            public bool TryEnableTable(TableDescriptor tableDescriptor)
            {
                return false;
            }

            public DataSourceInfo GetDataSourceInfo()
            {
                throw new NotImplementedException();
            }

            public Task ProcessAsync(IProgress<int> progress, CancellationToken cancellationToken)
            {
                throw new NotImplementedException();
            }

            public IEnumerable<TableDescriptor> GetEnabledTables()
            {
                throw new NotImplementedException();
            }
        }
    }
}
