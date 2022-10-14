// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.

using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.Performance.SDK.Extensibility;
using Microsoft.Performance.SDK.Processing;
using Microsoft.Performance.Testing;
using Microsoft.Performance.Testing.SDK;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace Microsoft.Performance.SDK.Tests
{
    [TestClass]
    public class CustomDataProcessorTests
    {
        private ProcessorOptions Options { get; set; }
        private IApplicationEnvironment ApplicationEnvironment { get; set; }
        private HashSet<TableDescriptor> Tables { get; set; }
        private IProcessorEnvironment ProcessorEnvironment { get; set; }
        private List<TableDescriptor> MetadataTables { get; set; }

        private MockProcessor Sut { get; set; }

        [TestInitialize]
        public void Setup()
        {
            this.Options = new ProcessorOptions(
                new[]
                {
                    new OptionInstance(
                        FakeProcessingSourceOptions.Ids.One,
                        "face"),
                });
            this.ApplicationEnvironment = new StubApplicationEnvironment();
            this.ProcessorEnvironment = Any.ProcessorEnvironment();
            this.Tables = new HashSet<TableDescriptor>
            {
                Any.TableDescriptor(),
                Any.TableDescriptor(),
                Any.TableDescriptor(),
                Any.MetadataTableDescriptor(),
                Any.MetadataTableDescriptor(),
            };

            this.MetadataTables = this.Tables.Where(x => x.IsMetadataTable).ToList();

            this.Sut = new MockProcessor(
                this.Options,
                this.ApplicationEnvironment,
                this.ProcessorEnvironment);
        }

        [TestMethod]
        [UnitTest]
        public void OptionsHydratedCorrectly()
        {
            Assert.AreEqual(this.Options, this.Sut.ExposedOptions);
        }

        [TestMethod]
        [UnitTest]
        public void ApplicationEnvironmentHydratedCorrectly()
        {
            Assert.AreEqual(this.ApplicationEnvironment, this.Sut.ExposedApplicationEnvironment);
        }

        [TestMethod]
        [UnitTest]
        public void MetadataTablesNotEnabledByDefault()
        {
            Assert.IsTrue(this.MetadataTables.Count != 0);
            Assert.AreEqual(0, this.Sut.ExposedEnabledTables.Count);
            foreach (var table in this.MetadataTables)
            {
                Assert.IsFalse(this.Sut.ExposedEnabledTables.Contains(table));
            }
        }

        [TestMethod]
        [UnitTest]
        public void EnableTableAddsToCorrectCollection()
        {
            var tableToEnable = Any.TableDescriptor();

            this.Sut.EnableTable(tableToEnable);

            Assert.AreEqual(1, this.Sut.ExposedEnabledTables.Count);
            Assert.IsTrue(this.Sut.ExposedEnabledTables.Contains(tableToEnable));
        }

        [TestMethod]
        [UnitTest]
        public void BuildTableDelegatesCorrectly()
        {
            var tableToBuild = this.Tables.First();
            var tableBuilder = new FakeTableBuilder();

            this.Sut.EnableTable(tableToBuild);
            this.Sut.BuildTable(tableToBuild, tableBuilder);

            Assert.AreEqual(1, this.Sut.BuildTableCoreCalls.Count);
            Assert.AreEqual(tableToBuild, this.Sut.BuildTableCoreCalls[0].Item1);
            Assert.AreEqual(tableBuilder, this.Sut.BuildTableCoreCalls[0].Item2);
        }

        [TestMethod]
        [UnitTest]
        public void BuildMetaTableDelegatesCorrectly()
        {
            var tableToBuild = this.MetadataTables.First();
            var tableBuilder = new FakeTableBuilder();

            this.Sut.EnableTable(tableToBuild);
            this.Sut.BuildTable(tableToBuild, tableBuilder);

            Assert.AreEqual(1, this.Sut.BuildTableCoreCalls.Count);
            Assert.AreEqual(tableToBuild, this.Sut.BuildTableCoreCalls[0].Item1);
            Assert.AreEqual(tableBuilder, this.Sut.BuildTableCoreCalls[0].Item2);
        }

        private sealed class MockProcessor
            : CustomDataProcessor
        {
            public MockProcessor(
                ProcessorOptions options,
                IApplicationEnvironment applicationEnvironment,
                IProcessorEnvironment processorEnvironment)
                : base(options, applicationEnvironment, processorEnvironment)
            {
                this.BuildTableCoreCalls = new List<Tuple<TableDescriptor, ITableBuilder>>();
            }

            public ProcessorOptions ExposedOptions => this.Options;

            public IApplicationEnvironment ExposedApplicationEnvironment => this.ApplicationEnvironment;

            public ReadOnlyHashSet<TableDescriptor> ExposedEnabledTables => this.EnabledTables;

            public override DataSourceInfo GetDataSourceInfo()
            {
                return new DataSourceInfo(0, 1, DateTime.UnixEpoch);
            }

            protected override Task ProcessAsyncCore(IProgress<int> progress, CancellationToken cancellationToken)
            {
                return Task.CompletedTask;
            }

            public List<Tuple<TableDescriptor, ITableBuilder>> BuildTableCoreCalls { get; }
            protected override void BuildTableCore(
                TableDescriptor tableDescriptor,
                ITableBuilder tableBuilder)
            {
                this.BuildTableCoreCalls.Add(Tuple.Create(tableDescriptor, tableBuilder));
            }
        }

        private sealed class FakeTableBuilder
            : ITableBuilder
        {
            public IEnumerable<TableConfiguration> BuiltInTableConfigurations => throw new NotImplementedException();

            public TableConfiguration DefaultConfiguration => throw new NotImplementedException();

            public ITableService Service => throw new NotImplementedException();

            public ITableBuilder AddTableCommand(string commandName, TableCommandCallback callback)
            {
                throw new NotImplementedException();
            }

            public ITableBuilder AddTableConfiguration(TableConfiguration configuration)
            {
                throw new NotImplementedException();
            }

            public ITableBuilder SetDefaultTableConfiguration(TableConfiguration configuration)
            {
                throw new NotImplementedException();
            }

            public ITableBuilderWithRowCount SetRowCount(int numberOfRows)
            {
                throw new NotImplementedException();
            }

            public ITableBuilder SetService(ITableService service)
            {
                throw new NotImplementedException();
            }
        }

        [Table]
        public class TestTable1
        {
            public static readonly string SourceParserId = "SourceId1";

            public static readonly DataCookerPath RequiredDataCooker =
                DataCookerPath.ForSource(SourceParserId, "CookerId1");

            public static TableDescriptor TableDescriptor = new TableDescriptor(
                    Guid.Parse("{C2AC873F-B88E-46A1-A08B-0CDF1D0C9F18}"),
                    "Test Table with Requirements",
                    "This table has required data extensions",
                    "Other",
                    isMetadataTable: true,
                    requiredDataCookers: new List<DataCookerPath> { RequiredDataCooker })
            { Type = typeof(TestTable1) };

            public static void BuildTable(ITableBuilder tableBuilder, IDataExtensionRetrieval dataRetrieval)
            {
            }
        }


    }
}
