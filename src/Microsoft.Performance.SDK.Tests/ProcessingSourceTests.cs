// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.

using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using Microsoft.Performance.SDK.Processing;
using Microsoft.Performance.Testing;
using Microsoft.Performance.Testing.SDK;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace Microsoft.Performance.SDK.Tests
{
    [TestClass]
    public class ProcessingSourceTests
    {
        private ITableConfigurationsSerializer serializer = new FakeSerializer();
        private IApplicationEnvironment applicationEnvironment = new StubApplicationEnvironment() { Serializer = new FakeSerializer() };

        [TestCleanup]
        public void Cleanup()
        {
            AssemblyBasedStubDataSource.Assembly = typeof(AssemblyBasedStubDataSource).Assembly;
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
                    typeof(StubMetadataTableNoBuildMethod),
                    typeof(StubDataTableOneNoBuildMethod),
                    typeof(StubDataTableTwoNoBuildMethod),
                }
            };

            var expectedDescriptors = TableDescriptorUtils.CreateTableDescriptors(
                serializer,
                typeof(StubMetadataTableNoBuildMethod),
                typeof(StubDataTableOneNoBuildMethod),
                typeof(StubDataTableTwoNoBuildMethod));

            AssemblyBasedStubDataSource.Assembly = assembly;

            var sut = new AssemblyBasedStubDataSource();
            sut.SetApplicationEnvironment(applicationEnvironment);

            Assert.AreEqual(expectedDescriptors.Count, sut.AllTablesExposed.Count);
            foreach (var td in expectedDescriptors)
            {
                Assert.IsTrue(sut.AllTablesExposed.Contains(td));
            }
        }

        [TestMethod]
        [UnitTest]
        public void CreateProcessorWithOneDataSourceCallsSubClass()
        {
            var dataSource = Any.DataSource();
            var env = Any.ProcessorEnvironment();
            var options = ProcessorOptions.Default;

            var sut = new AssemblyBasedStubDataSource();
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

            var sut = new AssemblyBasedStubDataSource
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

            var sut = new AssemblyBasedStubDataSource();
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

            var sut = new AssemblyBasedStubDataSource
            {
                ProcessorToReturn = null,
            };

            sut.SetApplicationEnvironment(applicationEnvironment);

            Assert.ThrowsException<InvalidOperationException>(
                () => sut.CreateProcessor(dataSources, Any.ProcessorEnvironment(), ProcessorOptions.Default));
        }

        [TestMethod]
        [UnitTest]
        public void WhenTableDiscoveryPassedInConstructorUsesDiscovery()
        {
            TableDescriptorUtils.CreateTableDescriptors(
                serializer,
                out var expectedDescriptors,
                out var _,
                typeof(StubDataTableOne),
                typeof(StubDataTableTwo),
                typeof(StubDataTableThree),
                typeof(StubMetadataTableOne),
                typeof(StubMetadataTableTwo));

            var discovery = new FakeTableProvider();
            discovery.DiscoverReturnValue = new HashSet<TableDescriptor>
            {
                expectedDescriptors[0],
                expectedDescriptors[1],
                expectedDescriptors[2],
                expectedDescriptors[3],
                expectedDescriptors[4],
            };

            var sut = new DeprecatedStubDataSource(discovery);
            sut.SetApplicationEnvironment(applicationEnvironment);

            Assert.AreEqual(5, sut.AllTablesExposed.Count);

            foreach (var td in expectedDescriptors)
            {
                Assert.IsTrue(sut.AllTablesExposed.Contains(td));
            }
        }

        [TestMethod]
        [UnitTest]
        public void WhenTableDiscoveryProvidedIsNull_DiscoveryThrows()
        {
            var sut = new CreateTableProviderBasedStubDataSource(null);
            Assert.ThrowsException<InvalidOperationException>(() => sut.SetApplicationEnvironment(applicationEnvironment));
        }

        [TestMethod]
        [UnitTest]
        public void WhenTableDiscoveryNotProvided_DefaultUsedForDiscovery()
        {
            var sut = new DefaultTableProviderStubDataSource();
            sut.SetApplicationEnvironment(applicationEnvironment);

            // note: the table provider is assembly based so we can't easily check the contents in the unit test,
            // but other tests check the contents, and the purpose of this test is just to check that a default
            // provider was created.
            Assert.IsNotNull(sut.TableProvider, "A default table provider should be used, but is null.");
        }

        [TestMethod]
        [UnitTest]
        public void WhenDiscoveryProvidesDuplicateTables_DiscoveryThrows()
        {
            TableDescriptorUtils.CreateTableDescriptors(
                serializer,
                out var allDescriptors,
                out var buildTableActions,
                typeof(StubDataTableOne),

                typeof(StubDataTableTwo),

                typeof(StubMetadataTableOne),
                typeof(StubMetadataTableTwo),

                typeof(StubMetadataTableNoBuildMethod),
                typeof(StubDataTableOneNoBuildMethod),
                typeof(StubDataTableOneNoBuildMethod),
                typeof(StubDataTableTwoNoBuildMethod));

            var expectedDescriptors = new List<TableDescriptor>();
            for (int x = 0; x < allDescriptors.Count; x++)
            {
                if (buildTableActions[x] is null && !allDescriptors[x].RequiresDataExtensions())
                {
                    expectedDescriptors.Add(allDescriptors[x]);
                }
            }

            var discovery = new FakeTableProvider();
            discovery.DiscoverReturnValue = expectedDescriptors;

            var sut = new CreateTableProviderBasedStubDataSource(discovery);

            Assert.ThrowsException<InvalidOperationException>(() => sut.SetApplicationEnvironment(applicationEnvironment));
        }

        private abstract class StubDataSource
            : ProcessingSource
        {
            protected StubDataSource()
                : base()
            {
                this.CommandLineOptionsToReturn = new List<Option>();
                this.SetApplicationEnvironmentCalls = new List<IApplicationEnvironment>();
                this.ProcessorToReturn = new MockCustomDataProcessor();
                this.CreateProcessorCoreCalls =
                    new List<Tuple<IEnumerable<IDataSource>, IProcessorEnvironment, ProcessorOptions>>();
            }

            protected StubDataSource(IProcessingSourceTableProvider tableProvider)
                : base(tableProvider)
            {
                this.CommandLineOptionsToReturn = new List<Option>();
                this.SetApplicationEnvironmentCalls = new List<IApplicationEnvironment>();
                this.ProcessorToReturn = new MockCustomDataProcessor();
                this.CreateProcessorCoreCalls =
                    new List<Tuple<IEnumerable<IDataSource>, IProcessorEnvironment, ProcessorOptions>>();
            }

            public HashSet<TableDescriptor> AllTablesExposed => new HashSet<TableDescriptor>(this.AllTables);

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

        [ProcessingSource("{CABDB99F-F182-457B-B0B4-AD3DD62272D8}", "One", "One")]
        [FileDataSource(".csv")]
        private sealed class AssemblyBasedStubDataSource
            : StubDataSource
        {
            static AssemblyBasedStubDataSource()
            {
                Assembly = typeof(AssemblyBasedStubDataSource).Assembly;
            }

            public AssemblyBasedStubDataSource()
                : base()
            {
            }

            public static Assembly Assembly { get; set; }

            protected override IProcessingSourceTableProvider CreateTableProvider()
            {
                return TableDiscovery.CreateForAssembly(Assembly);
            }
        }

        [ProcessingSource("{9F196984-3614-418B-9471-CA3D7F593A3B}", "One", "One")]
        [FileDataSource(".csv")]
        private sealed class CreateTableProviderBasedStubDataSource
            : StubDataSource
        {
            private readonly IProcessingSourceTableProvider tableProvider;

            public CreateTableProviderBasedStubDataSource(IProcessingSourceTableProvider tableProvider)
                : base()
            {
                this.tableProvider = tableProvider;
            }

            protected override IProcessingSourceTableProvider CreateTableProvider()
            {
                return this.tableProvider;
            }
        }

        [ProcessingSource("{CB8E1000-527C-4C2E-AA49-7D3678FB80B4}", "One", "One")]
        [FileDataSource(".csv")]
        private sealed class DeprecatedStubDataSource
            : StubDataSource
        {
            public DeprecatedStubDataSource(IProcessingSourceTableProvider tableProvider)
                : base(tableProvider)
            {
            }
        }

        [ProcessingSource("{90B38463-201A-4C64-83E6-83BE7AE1FC1A}", "One", "One")]
        [FileDataSource(".csv")]
        private sealed class DefaultTableProviderStubDataSource
            : StubDataSource
        {
            public DefaultTableProviderStubDataSource()
                : base()
            {
            }

            public IProcessingSourceTableProvider TableProvider { get; private set; }

            // override this, but just return the base implementation after storing the value and making it accessible
            // to the test.
            protected override IProcessingSourceTableProvider CreateTableProvider()
            {
                this.TableProvider = base.CreateTableProvider();
                return this.TableProvider;
            }
        }
    }
}
