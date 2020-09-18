// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.

using Microsoft.Performance.SDK.Extensibility;
using Microsoft.Performance.SDK.Processing;
using Microsoft.Performance.Testing;
using Microsoft.Performance.Testing.SDK;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;

namespace Microsoft.Performance.SDK.Tests
{
    [TestClass]
    public class CustomDataSourceBaseTests
    {
        private ISerializer serializer = new FakeSerializer();
        private IApplicationEnvironment applicationEnvironment = new StubApplicationEnvironment() { Serializer = new FakeSerializer() };

        [TestCleanup]
        public void Cleanup()
        {
            StubDataSource.Assembly = typeof(StubDataSource).Assembly;
        }

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

            StubDataSource.Assembly = assembly;

            var sut = new StubDataSource();
            sut.SetApplicationEnvironment(applicationEnvironment);

            Assert.IsFalse(sut.DataTables.Any());
            Assert.IsFalse(sut.MetadataTables.Any());
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

            StubDataSource.Assembly = assembly;

            var sut = new StubDataSource();
            sut.SetApplicationEnvironment(applicationEnvironment);

            Assert.AreEqual(3, sut.DataTables.Count());
            Assert.IsTrue(sut.DataTables.Contains(expectedDescriptor1));
            Assert.IsTrue(sut.DataTables.Contains(expectedDescriptor2));
            Assert.IsTrue(sut.DataTables.Contains(expectedDescriptor3));

            Assert.IsFalse(sut.MetadataTables.Any());
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

            StubDataSource.Assembly = assembly;

            var sut = new StubDataSource();
            sut.SetApplicationEnvironment(applicationEnvironment);

            Assert.IsFalse(sut.DataTables.Any());

            Assert.AreEqual(2, sut.MetadataTables.Count());
            Assert.IsTrue(sut.MetadataTables.Contains(expectedDescriptor1));
            Assert.IsTrue(sut.MetadataTables.Contains(expectedDescriptor2));
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

            StubDataSource.Assembly = assembly;

            var sut = new StubDataSource();
            sut.SetApplicationEnvironment(applicationEnvironment);

            Assert.AreEqual(3, sut.DataTables.Count());
            Assert.IsTrue(sut.DataTables.Contains(expectedDescriptor1));
            Assert.IsTrue(sut.DataTables.Contains(expectedDescriptor2));
            Assert.IsTrue(sut.DataTables.Contains(expectedDescriptor3));

            Assert.AreEqual(2, sut.MetadataTables.Count());
            Assert.IsTrue(sut.MetadataTables.Contains(expectedDescriptor4));
            Assert.IsTrue(sut.MetadataTables.Contains(expectedDescriptor5));
        }

        [TestMethod]
        [UnitTest]
        public void AllTablesIsUnionOfDataAndMetadata()
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

            StubDataSource.Assembly = assembly;

            var sut = new StubDataSource();
            sut.SetApplicationEnvironment(applicationEnvironment);

            Assert.AreEqual(5, sut.AllTablesExposed.Count);

            // call the build action of each of our tables
            foreach (var kvp in sut.AllTablesExposed)
            {
                kvp.Value.Invoke(null, null);
            }

            Assert.IsTrue(StubDataTableOne.BuildTableWasCalled);
            Assert.IsTrue(StubDataTableTwo.BuildTableWasCalled);
            Assert.IsTrue(StubDataTableThree.BuildTableWasCalled);
            Assert.IsTrue(StubMetadataTableOne.BuildTableWasCalled);
            Assert.IsTrue(StubMetadataTableTwo.BuildTableWasCalled);
        }

        [TestMethod]
        [UnitTest]
        public void CreateProcessorWithOneDataSourceCallsSubClass()
        {
            var dataSource = Any.DataSource();
            var env = Any.ProcessorEnvironment();
            var options = ProcessorOptions.Default;

            var sut = new StubDataSource();
            sut.SetApplicationEnvironment(applicationEnvironment);
            var result = sut.CreateProcessor(dataSource, env, options);

            Assert.AreEqual(1, sut.CreateProcessorCoreCalls.Count);
            Assert.AreEqual(1, sut.CreateProcessorCoreCalls[0].Item1.Count());
            Assert.AreEqual(dataSource, sut.CreateProcessorCoreCalls[0].Item1.ElementAt(0));
            Assert.AreEqual(env, sut.CreateProcessorCoreCalls[0].Item2);
            Assert.AreEqual(options, sut.CreateProcessorCoreCalls[0].Item3);
        }

        [TestMethod]
        [UnitTest]
        public void WhenSubClassReturnsNullProcessorThenThrows()
        {
            var dataSource = Any.DataSource();

            var sut = new StubDataSource
            {
                ProcessorToReturn = null,
            };
            sut.SetApplicationEnvironment(applicationEnvironment);

            Assert.ThrowsException<InvalidOperationException>(
                () => sut.CreateProcessor(dataSource, Any.ProcessorEnvironment(), ProcessorOptions.Default));
        }

        [TestMethod]
        [UnitTest]
        public void CreateProcessorWithManyDataSourcesCallsSubClass()
        {
            var dataSource = Any.DataSource();
            var dataSources = new[] { dataSource, };
            var env = Any.ProcessorEnvironment();
            var options = ProcessorOptions.Default;

            var sut = new StubDataSource();
            sut.SetApplicationEnvironment(applicationEnvironment);
            var result = sut.CreateProcessor(dataSources, env, options);

            Assert.AreEqual(1, sut.CreateProcessorCoreCalls.Count);
            Assert.AreEqual(dataSources, sut.CreateProcessorCoreCalls[0].Item1);
            Assert.AreEqual(env, sut.CreateProcessorCoreCalls[0].Item2);
            Assert.AreEqual(options, sut.CreateProcessorCoreCalls[0].Item3);
        }

        [TestMethod]
        [UnitTest]
        public void ManyDataSourcesWhenSubClassReturnsNullProcessorThenThrows()
        {
            var dataSource = Any.DataSource();
            var dataSources = new[] { dataSource, };

            var sut = new StubDataSource
            {
                ProcessorToReturn = null,
            };
            sut.SetApplicationEnvironment(applicationEnvironment);

            Assert.ThrowsException<InvalidOperationException>(
                () => sut.CreateProcessor(dataSources, Any.ProcessorEnvironment(), ProcessorOptions.Default));
        }

        [TestMethod]
        [UnitTest]
        public void WhenAdditionalTableProviderProvidedThenAdditionalTablesAdded()
        {
            var assembly = new FakeAssembly
            {
                TypesToReturn = new[]
                {
                    typeof(DateTime),
                    typeof(StubDataTableOne),
                    typeof(StubMetadataTableOne),
                    typeof(Exception),
                }
            };

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

            StubDataSource.Assembly = assembly;

            bool descriptor2BuildWasCalled = false;
            bool descriptor3BuildWasCalled = false;
            bool descriptor5BuildWasCalled = false;

            var additionalTables = new Dictionary<TableDescriptor, Action<ITableBuilder, IDataExtensionRetrieval>>
            {
                { expectedDescriptor2, (tableBuilder, cookedData) => { descriptor2BuildWasCalled = true;} },
                { expectedDescriptor3, (tableBuilder, cookedData) => { descriptor3BuildWasCalled = true;} },
                { expectedDescriptor5, (tableBuilder, cookedData) => { descriptor5BuildWasCalled = true;} },
            };

            var sut = new StubDataSource(() => additionalTables);
            sut.SetApplicationEnvironment(applicationEnvironment);

            Assert.AreEqual(5, sut.AllTablesExposed.Count);

            // call the build action of each of our tables
            foreach (var kvp in sut.AllTablesExposed)
            {
                kvp.Value.Invoke(null, null);
            }

            Assert.IsTrue(StubDataTableOne.BuildTableWasCalled);
            Assert.IsTrue(descriptor2BuildWasCalled);
            Assert.IsTrue(descriptor3BuildWasCalled);
            Assert.IsTrue(StubMetadataTableOne.BuildTableWasCalled);
            Assert.IsTrue(descriptor5BuildWasCalled);
        }

        [CustomDataSource("{CABDB99F-F182-457B-B0B4-AD3DD62272D8}", "One", "One")]
        [FileDataSource(".csv")]
        private sealed class StubDataSource
            : CustomDataSourceBase
        {
            static StubDataSource()
            {
                Assembly = typeof(StubDataSource).Assembly;
            }

            public StubDataSource()
                : this(() => new Dictionary<TableDescriptor, Action<ITableBuilder, IDataExtensionRetrieval>>())
            {
            }

            public StubDataSource(
                Func<IDictionary<TableDescriptor, Action<ITableBuilder, IDataExtensionRetrieval>>> additionalTableProvider)
                : base(additionalTableProvider, () => Assembly)
            {
                this.CommandLineOptionsToReturn = new List<Option>();
                this.SetApplicationEnvironmentCalls = new List<IApplicationEnvironment>();
                this.ProcessorToReturn = new MockCustomDataProcessor();
                this.CreateProcessorCoreCalls =
                    new List<Tuple<IEnumerable<IDataSource>, IProcessorEnvironment, ProcessorOptions>>();
            }

            public static Assembly Assembly { get; set; }

            public IReadOnlyDictionary<TableDescriptor, Action<ITableBuilder, IDataExtensionRetrieval>> AllTablesExposed => this.AllTables;

            public List<Option> CommandLineOptionsToReturn { get; set; }
            public override IEnumerable<Option> CommandLineOptions => this.CommandLineOptionsToReturn ?? Enumerable.Empty<Option>();

            public List<IApplicationEnvironment> SetApplicationEnvironmentCalls { get; }
            protected override void SetApplicationEnvironmentCore(IApplicationEnvironment applicationEnvironment)
            {
                this.SetApplicationEnvironmentCalls.Add(applicationEnvironment);
            }

            public ICustomDataProcessor ProcessorToReturn { get; set; }
            public List<Tuple<IEnumerable<IDataSource>, IProcessorEnvironment, ProcessorOptions>> CreateProcessorCoreCalls { get; }
            protected override ICustomDataProcessor CreateProcessorCore(
                IEnumerable<IDataSource> dataSources,
                IProcessorEnvironment processorEnvironment,
                ProcessorOptions options)
            {
                this.CreateProcessorCoreCalls.Add(
                    Tuple.Create(
                        dataSources,
                        processorEnvironment,
                        options));

                return this.ProcessorToReturn;
            }

            protected override bool IsFileSupportedCore(string path)
            {
                return true;
            }
        }

        [Table(InternalTable = true)]
        private sealed class StubDataTableOne
        {
            public bool TryCreateTable(ITableBuilder tableBuilder)
            {
                throw new NotImplementedException();
            }

            public static TableDescriptor TableDescriptor { get; } = new TableDescriptor(
                Guid.Parse("{F3F7B534-5DC5-40FB-93D9-07FDAC073A13}"),
                "Name0",
                "Description",
                "Category");

            public static bool BuildTableWasCalled { get; private set; }

            public static void BuildTable(ITableBuilder tableBuilder, IDataExtensionRetrieval cookedData)
            {
                BuildTableWasCalled = true;
            }
        }

        [Table(InternalTable = true)]
        private sealed class StubDataTableTwo
        {
            public bool TryCreateTable(ITableBuilder tableBuilder)
            {
                throw new NotImplementedException();
            }

            public static TableDescriptor TableDescriptor { get; } = new TableDescriptor(
                Guid.Parse("{677CA54E-45D2-46B1-80BE-6DBA96597435}"),
                "Name1",
                "Description",
                "Category");

            public static bool BuildTableWasCalled { get; private set; }

            public static void BuildTable(ITableBuilder tableBuilder, IDataExtensionRetrieval cookedData)
            {
                BuildTableWasCalled = true;
            }
        }

        [Table(InternalTable = true)]
        private sealed class StubDataTableThree
        {
            public bool TryCreateTable(ITableBuilder tableBuilder)
            {
                throw new NotImplementedException();
            }

            public static TableDescriptor TableDescriptor { get; } = new TableDescriptor(
                Guid.Parse("{96D8DD5E-C0FC-4681-85E2-CFAFD1A0803C}"),
                "Name2",
                "Description",
                "Category");

            public static bool BuildTableWasCalled { get; private set; }

            public static void BuildTable(ITableBuilder tableBuilder, IDataExtensionRetrieval cookedData)
            {
                BuildTableWasCalled = true;
            }
        }

        [Table(InternalTable = true)]
        private sealed class StubMetadataTableOne
        {
            public bool TryCreateTable(ITableBuilder tableBuilder)
            {
                throw new NotImplementedException();
            }

            public static TableDescriptor TableDescriptor { get; } = new TableDescriptor(
                Guid.Parse("{824C0827-40E8-4DE7-ACD2-C6614E916D86}"),
                "Metadata1",
                "Description",
                TableDescriptor.DefaultCategory,
                true);

            public static bool BuildTableWasCalled { get; private set; }

            public static void BuildTable(ITableBuilder tableBuilder, IDataExtensionRetrieval cookedData)
            {
                BuildTableWasCalled = true;
            }
        }

        [Table(InternalTable = true)]
        private sealed class StubMetadataTableTwo
        {
            public bool TryCreateTable(ITableBuilder tableBuilder)
            {
                throw new NotImplementedException();
            }

            public static TableDescriptor TableDescriptor { get; } = new TableDescriptor(
                Guid.Parse("{2072CA7C-79F0-4FA5-9DBD-1453D117629F}"),
                "Metadata2",
                "Description",
                TableDescriptor.DefaultCategory,
                true);

            public static bool BuildTableWasCalled { get; private set; }

            public static void BuildTable(ITableBuilder tableBuilder, IDataExtensionRetrieval cookedData)
            {
                BuildTableWasCalled = true;
            }
        }
    }
}
