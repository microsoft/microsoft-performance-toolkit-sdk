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

            var expectedDescriptors = TableDescriptorUtils.CreateTableDescriptors(
                serializer,
                typeof(StubDataTableOne),
                typeof(StubDataTableTwo),
                typeof(StubDataTableThree),
                typeof(StubMetadataTableOne),
                typeof(StubMetadataTableTwo));

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
        public void WhenTableDiscoveryProvidedUsesDiscovery()
        {
            TableDescriptorUtils.CreateTableDescriptors(
                serializer,
                out var expectedDescriptors,
                out var buildActions,
                out _,
                typeof(StubDataTableOne),
                typeof(StubDataTableTwo),
                typeof(StubDataTableThree),
                typeof(StubMetadataTableOne),
                typeof(StubMetadataTableTwo));

            var discovery = new FakeTableProvider();
            discovery.DiscoverReturnValue = new HashSet<DiscoveredTable>
            {
                new DiscoveredTable(expectedDescriptors[0], buildActions[0]),
                new DiscoveredTable(expectedDescriptors[1], buildActions[1]),
                new DiscoveredTable(expectedDescriptors[2], buildActions[2]),
                new DiscoveredTable(expectedDescriptors[3], buildActions[3]),
                new DiscoveredTable(expectedDescriptors[4], buildActions[4]),
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

        [TestMethod]
        [UnitTest]
        public void WhenDiscoveryProvidesDuplicateTables_DiscoveryThrows()
        {
            TableDescriptorUtils.CreateTableDescriptors(
                serializer,
                out var expectedDescriptors,
                out var buildActions,
                out _,
                typeof(StubDataTableOne),

                typeof(StubDataTableTwo),
                typeof(StubDataTableTwo),

                typeof(StubMetadataTableOne),
                typeof(StubMetadataTableTwo));

            var discovery = new FakeTableProvider();
            discovery.DiscoverReturnValue = new HashSet<DiscoveredTable>
            {
                new DiscoveredTable(expectedDescriptors[0], buildActions[0]),
                new DiscoveredTable(expectedDescriptors[1], buildActions[1]),
                new DiscoveredTable(expectedDescriptors[2], buildActions[2]),
                new DiscoveredTable(expectedDescriptors[3], buildActions[3]),
                new DiscoveredTable(expectedDescriptors[4], buildActions[4]),
            };

            var sut = new StubDataSource(discovery);

            Assert.ThrowsException<InvalidOperationException>(() => sut.SetApplicationEnvironment(applicationEnvironment));
        }

        [TestMethod]
        [UnitTest]
        public void WhenBuildActionIsNull_DelegatesToOverriddenGetBuildTable()
        {
            TableDescriptorUtils.CreateTableDescriptors(
                   serializer,
                   out var expectedDescriptors,
                   out var buildActions,
                   out _,
                   typeof(StubDataTableOne));

            var discovery = new FakeTableProvider();
            discovery.DiscoverReturnValue = new HashSet<DiscoveredTable>
            {
                //
                // Ignore the discovered build action, if any, as we are explciitly testing
                // that missing build actions cause a delegation to the processing source.
                //

                new DiscoveredTable(expectedDescriptors[0]),
            };

            Assert.IsNull(discovery.DiscoverReturnValue.Single().BuildTable);

            var sut = new GetBuildTableActionDataSource(discovery);

            //
            // When we invoke build table later in the test, in the GetBuildTableAction method was called, then our
            // delegate would have been assigned, and thus invoked when BuildTable is invoked. This is how we can
            // sense that our override is being respected in the null case.
            //

            var processingSourceGetBuildTableCalled = false;
            sut.GetBuildTableActionReturnValues[expectedDescriptors.Single().Type] = (_, __) => processingSourceGetBuildTableCalled = true;

            sut.SetApplicationEnvironment(applicationEnvironment);

            foreach (var kvp in sut.AllTablesExposed)
            {
                kvp.Value.Invoke(null, null);
            }

            Assert.IsTrue(processingSourceGetBuildTableCalled);
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

        [ProcessingSource("{293DA3BE-7ED2-4FD7-B5E4-8BCCAADD23AD}", "Two", "Two")]
        [FileDataSource(".csv")]
        private sealed class GetBuildTableActionDataSource
            : ProcessingSource
        {
            static GetBuildTableActionDataSource()
            {
                Assembly = typeof(GetBuildTableActionDataSource).Assembly;
            }

            public GetBuildTableActionDataSource()
                : this(TableDiscovery.CreateForAssembly(Assembly))
            {
            }

            public GetBuildTableActionDataSource(ITableProvider discovery)
               : base(discovery)
            {
                this.CommandLineOptionsToReturn = new List<Option>();
                this.SetApplicationEnvironmentCalls = new List<IApplicationEnvironment>();
                this.ProcessorToReturn = new MockCustomDataProcessor();
                this.CreateProcessorCoreCalls =
                    new List<Tuple<IEnumerable<IDataSource>, IProcessorEnvironment, ProcessorOptions>>();

                this.GetBuildTableActionReturnValues = new Dictionary<Type, Action<ITableBuilder, IDataExtensionRetrieval>>();
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

            public Dictionary<Type, Action<ITableBuilder, IDataExtensionRetrieval>> GetBuildTableActionReturnValues { get; }

            protected override Action<ITableBuilder, IDataExtensionRetrieval> GetTableBuildAction(Type type)
            {
                if (this.GetBuildTableActionReturnValues.TryGetValue(type, out var v))
                {
                    return v;
                }

                return base.GetTableBuildAction(type);
            }
        }
    }
}
