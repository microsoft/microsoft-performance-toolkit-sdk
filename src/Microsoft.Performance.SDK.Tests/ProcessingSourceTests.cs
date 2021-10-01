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
    public class ProcessingSourceTests
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
        [Obsolete("Remove when the deprectated constructor is removed.")]
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

        [TestMethod]
        [UnitTest]
        public void WhenTableDiscoveryProvidedUsesDiscovery()
        {
            Assert.IsTrue(
                TableDescriptorFactory.TryCreate(
                    typeof(StubDataTableOne),
                    serializer,
                    out _,
                    out TableDescriptor expectedDescriptor1,
                    out var buildTable1));
            Assert.IsTrue(
                TableDescriptorFactory.TryCreate(
                    typeof(StubDataTableTwo),
                    serializer,
                    out _,
                    out TableDescriptor expectedDescriptor2,
                    out var buildTable2));
            Assert.IsTrue(
                TableDescriptorFactory.TryCreate(
                    typeof(StubDataTableThree),
                    serializer,
                    out _,
                    out TableDescriptor expectedDescriptor3,
                    out var buildTable3));

            Assert.IsTrue(
                TableDescriptorFactory.TryCreate(
                    typeof(StubMetadataTableOne),
                    serializer,
                    out _,
                    out TableDescriptor expectedDescriptor4,
                    out var buildTable4));
            Assert.IsTrue(
                TableDescriptorFactory.TryCreate(
                    typeof(StubMetadataTableTwo),
                    serializer,
                    out _,
                    out TableDescriptor expectedDescriptor5,
                    out var buildTable5));

            var discovery = new FakeTableProvider();
            discovery.DiscoverReturnValue = new HashSet<DiscoveredTable>
            {
                new DiscoveredTable(expectedDescriptor1, buildTable1),
                new DiscoveredTable(expectedDescriptor2, buildTable2),
                new DiscoveredTable(expectedDescriptor3, buildTable3),
                new DiscoveredTable(expectedDescriptor4, buildTable4),
                new DiscoveredTable(expectedDescriptor5, buildTable5),
            };

            var sut = new StubDataSource(discovery);
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

        [ProcessingSource("{CABDB99F-F182-457B-B0B4-AD3DD62272D8}", "One", "One")]
        [FileDataSource(".csv")]
        private sealed class StubDataSource
            : ProcessingSource
        {
            static StubDataSource()
            {
                Assembly = typeof(StubDataSource).Assembly;
            }

            public StubDataSource()
                : this(TableDiscovery.CreateForAssembly(Assembly))
            {
            }

            public StubDataSource(ITableProvider discovery)
               : base(discovery)
            {
                this.CommandLineOptionsToReturn = new List<Option>();
                this.SetApplicationEnvironmentCalls = new List<IApplicationEnvironment>();
                this.ProcessorToReturn = new MockCustomDataProcessor();
                this.CreateProcessorCoreCalls =
                    new List<Tuple<IEnumerable<IDataSource>, IProcessorEnvironment, ProcessorOptions>>();
            }

            [Obsolete("Remove when deprecated constructor is removed.")]
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

            protected override bool IsDataSourceSupportedCore(IDataSource dataSource)
            {
                return true;
            }
        }
    }
}
