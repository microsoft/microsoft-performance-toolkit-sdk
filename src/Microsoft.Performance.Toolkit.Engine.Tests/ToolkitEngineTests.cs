// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.

using System;
using System.Collections;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Reflection;
using System.Threading.Tasks;
using Microsoft.Performance.SDK.Auth;
using Microsoft.Performance.SDK.Extensibility;
using Microsoft.Performance.SDK.Options;
using Microsoft.Performance.SDK.Options.Values;
using Microsoft.Performance.SDK.Processing;
using Microsoft.Performance.SDK.Runtime;
using Microsoft.Performance.SDK.Runtime.NetCoreApp.Discovery;
using Microsoft.Performance.SDK.Runtime.Options;
using Microsoft.Performance.Testing;
using Microsoft.Performance.Testing.SDK;
using Microsoft.Performance.Toolkit.Engine.Tests.TestCookers.Interactive;
using Microsoft.Performance.Toolkit.Engine.Tests.TestCookers.Source123;
using Microsoft.Performance.Toolkit.Engine.Tests.TestCookers.Source4;
using Microsoft.Performance.Toolkit.Engine.Tests.TestData;
using Microsoft.Performance.Toolkit.Engine.Tests.TestTables;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace Microsoft.Performance.Toolkit.Engine.Tests
{
    [TestClass]
    public class ToolkitEngineTests
        : EngineFixture
    {
        private DataSourceSet DefaultSet { get; set; }

        public override void OnInitialize()
        {
            this.DefaultSet = DataSourceSet.Create();
            base.OnInitialize();
        }

        public override void OnCleanup()
        {
            this.DefaultSet.SafeDispose();
        }

        #region Create

        [TestMethod]
        [UnitTest]
        public void Create_NullParameters_Throw()
        {
            Assert.ThrowsException<ArgumentNullException>(
                () => Engine.Create((EngineCreateInfo)null));

            Assert.ThrowsException<ArgumentNullException>(
                () => Engine.Create((IDataSource)null));

            Assert.ThrowsException<ArgumentNullException>(
                () => Engine.Create((IDataSource)null, typeof(Source123Processor)));
            Assert.ThrowsException<ArgumentNullException>(
                () => Engine.Create(Any.DataSource(), null));

            Assert.ThrowsException<ArgumentNullException>(
                () => Engine.Create((IEnumerable<IDataSource>)null, typeof(Source123Processor)));
            Assert.ThrowsException<ArgumentNullException>(
                () => Engine.Create(new[] { Any.DataSource(), }, null));
        }

        [TestMethod]
        [IntegrationTest]
        public void Create_IsProcessed_False()
        {
            var info = new EngineCreateInfo(this.DefaultSet.AsReadOnly());
            using var sut = Engine.Create(info);
            Assert.IsFalse(sut.IsProcessed);
        }

        [TestMethod]
        [IntegrationTest]
        public void Create_DataSourcesCloned()
        {
            var file = new FileDataSource("test" + Source123DataSource.Extension);
            using var dataSources = DataSourceSet.Create();
            dataSources.AddDataSource(new FileDataSource("test" + Source123DataSource.Extension));

            var info = new EngineCreateInfo(dataSources.AsReadOnly());
            using var sut = Engine.Create(info);

            Assert.AreNotSame(dataSources, sut.DataSourcesToProcess);
            Assert.AreEqual(1, sut.DataSourcesToProcess.FreeDataSourcesToProcess.Count());
            Assert.AreEqual(file, sut.DataSourcesToProcess.FreeDataSourcesToProcess.Single());
        }

        [TestMethod]
        [IntegrationTest]
        public void Create_WithInvalidOptions()
        {
            var invalidProcessorOptions = new ProcessorOptions(
                new[]
                {
                    new OptionInstance(
                        new object(),
                        "oops"),
                });

            using var plugins = PluginSet.Load(Environment.CurrentDirectory);
            using var dataSources = DataSourceSet.Create(plugins);
            dataSources.AddDataSource(new FileDataSource("test" + Source123DataSource.Extension));
            var engineCreateInfo = new EngineCreateInfo(dataSources.AsReadOnly()).WithProcessorOptions(invalidProcessorOptions);

            using var sut = Engine.Create(engineCreateInfo);

            var processingSourceReference = plugins.ProcessingSourceReferences.Single(psr => psr.Guid.Equals(Source123DataSource.Guid));
            var processingSourceInstance = processingSourceReference.Instance as Source123DataSource;
            Assert.IsNull(processingSourceInstance.UserSpecifiedOptions, "User specified supportedOptions were found, despite expecting null, as passed supportedOptions are invalid");
        }

        [TestMethod]
        [IntegrationTest]
        public void Create_DataSource_DisposesDataSet()
        {
            // Engine.Create(IDataSourceSet) owns the data source set and should dispose of it

            Source123DataSource processingSource = null;

            using (Engine sut = Engine.Create(new FileDataSource("test" + Source123DataSource.Extension)))
            {
                processingSource = sut.Plugins.ProcessingSourceReferences
                    .Where(x => x.Name == nameof(Source123DataSource))
                    .First()
                    .Instance as Source123DataSource;

                Assert.IsNotNull(processingSource, $"{nameof(processingSource)} is null.");
            }

            Assert.IsTrue(processingSource.IsDisposed, $"{processingSource} is not disposed.");
        }

        [TestMethod]
        [IntegrationTest]
        public void Create_DataSourceAndType_DisposesDataSet()
        {
            // Engine.Create(IDataSourceSet, Type) owns the data source set and should dispose of it

            Source123DataSource processingSource = null;

            using (Engine sut = Engine.Create(new FileDataSource("test" + Source123DataSource.Extension), typeof(Source123DataSource)))
            {
                processingSource = sut.Plugins.ProcessingSourceReferences
                    .Where(x => x.Name == nameof(Source123DataSource))
                    .First()
                    .Instance as Source123DataSource;

                Assert.IsNotNull(processingSource, $"{nameof(processingSource)} is null.");
            }

            Assert.IsTrue(processingSource.IsDisposed, $"{processingSource} is not disposed.");
        }

        [TestMethod]
        [IntegrationTest]
        public void Create_DataSourcesAndType_DisposesDataSet()
        {
            // Engine.Create(IEnumerable<IDataSourceSet>, Type) owns the data source set and should dispose of it

            Source123DataSource processingSource = null;

            IDataSource[] sources = new[]
            {
                new FileDataSource("test1" + Source123DataSource.Extension),
                new FileDataSource("test2" + Source123DataSource.Extension),
            };

            using (Engine sut = Engine.Create(sources, typeof(Source123DataSource)))
            {
                processingSource = sut.Plugins.ProcessingSourceReferences
                    .Where(x => x.Name == nameof(Source123DataSource))
                    .First()
                    .Instance as Source123DataSource;

                Assert.IsNotNull(processingSource, $"{nameof(processingSource)} is null.");
            }

            Assert.IsTrue(processingSource.IsDisposed, $"{processingSource} is not disposed.");
        }

        [TestMethod]
        [IntegrationTest]
        public void Create_WithValidOptions()
        {
            var expectedProcessorOptions = new ProcessorOptions(
                new[]
                {
                    new OptionInstance(
                        FakeProcessingSourceOptions.Ids.One,
                        "valid"),
                });

            using var plugins = PluginSet.Load(Environment.CurrentDirectory);
            using var dataSources = DataSourceSet.Create(plugins);
            dataSources.AddDataSource(new FileDataSource("test" + Source123DataSource.Extension));
            var engineCreateInfo = new EngineCreateInfo(dataSources.AsReadOnly()).WithProcessorOptions(expectedProcessorOptions);

            using var sut = Engine.Create(engineCreateInfo);

            var processingSourceReference = plugins.ProcessingSourceReferences.Single(psr => psr.Guid.Equals(Source123DataSource.Guid));
            var processingSourceInstance = processingSourceReference.Instance as Source123DataSource;
            Assert.AreEqual(expectedProcessorOptions, processingSourceInstance.UserSpecifiedOptions, "User specified supportedOptions do not match");
        }

        #endregion Create

        #region Enable Cooker

        [TestMethod]
        [IntegrationTest]
        public void EnableCooker_Known_Enables()
        {
            this.DefaultSet.AddDataSource(new FileDataSource("test" + Source123DataSource.Extension));
            using var sut = Engine.Create(new EngineCreateInfo(this.DefaultSet.AsReadOnly()));

            var cooker = Source1DataCooker.DataCookerPath;

            sut.EnableCooker(cooker);

            Assert.AreEqual(1, sut.EnabledCookers.Count());
            Assert.AreEqual(cooker, sut.EnabledCookers.ElementAt(0));
        }

        [TestMethod]
        [IntegrationTest]
        public void EnableCooker_NotKnown_Throws()
        {
            using var sut = Engine.Create(new EngineCreateInfo(this.DefaultSet.AsReadOnly()));

            var cooker = DataCookerPath.ForComposite("not-there-id");

            var e = Assert.ThrowsException<CookerNotFoundException>(() => sut.EnableCooker(cooker));
            Assert.AreEqual(cooker, e.DataCookerPath);
        }

        [TestMethod]
        [IntegrationTest]
        public void EnableCooker_AlreadyProcessed_Throws()
        {
            using var sut = Engine.Create(new EngineCreateInfo(this.DefaultSet.AsReadOnly()));

            sut.Process();

            Assert.ThrowsException<InstanceAlreadyProcessedException>(() => sut.EnableCooker(this.DefaultSet.Plugins.AllCookers.First()));
        }

        [TestMethod]
        [IntegrationTest]
        public void EnableCooker_NoData_Throws()
        {
            this.DefaultSet.AddDataSource(new FileDataSource("test" + Source123DataSource.Extension));
            var createInfo = new EngineCreateInfo(this.DefaultSet.AsReadOnly());

            using var sut = Engine.Create(createInfo);

            Assert.ThrowsException<NoDataSourceException>(() => sut.EnableCooker(Source4DataCooker.DataCookerPath));
        }

        #endregion Enable Cooker

        #region TryEnableCooker

        [TestMethod]
        [IntegrationTest]
        public void TryEnableCooker_Known_Enables()
        {
            this.DefaultSet.AddDataSource(new FileDataSource("test" + Source123DataSource.Extension));
            using var sut = Engine.Create(new EngineCreateInfo(this.DefaultSet.AsReadOnly()));

            var cooker = Source1DataCooker.DataCookerPath;

            sut.TryEnableCooker(cooker);

            Assert.AreEqual(1, sut.EnabledCookers.Count());
            Assert.AreEqual(cooker, sut.EnabledCookers.ElementAt(0));
        }

        [TestMethod]
        [IntegrationTest]
        public void TryEnableCooker_Known_True()
        {
            this.DefaultSet.AddDataSource(new FileDataSource("test" + Source123DataSource.Extension));
            using var sut = Engine.Create(new EngineCreateInfo(this.DefaultSet.AsReadOnly()));

            var cooker = Source1DataCooker.DataCookerPath;

            Assert.IsTrue(sut.TryEnableCooker(cooker));

            Assert.AreEqual(1, sut.EnabledCookers.Count());
            Assert.AreEqual(cooker, sut.EnabledCookers.ElementAt(0));
        }

        [TestMethod]
        [IntegrationTest]
        public void TryEnableCooker_NotKnown_False()
        {
            using var sut = Engine.Create(new EngineCreateInfo(this.DefaultSet.AsReadOnly()));

            var cooker = DataCookerPath.ForComposite("not-there-id");

            Assert.IsFalse(sut.TryEnableCooker(cooker));
        }

        [TestMethod]
        [IntegrationTest]
        public void TryEnableCooker_AlreadyProcessed_False()
        {
            using var sut = Engine.Create(new EngineCreateInfo(this.DefaultSet.AsReadOnly()));

            sut.Process();

            Assert.IsFalse(sut.TryEnableCooker(this.DefaultSet.Plugins.AllCookers.First()));
        }

        [TestMethod]
        [IntegrationTest]
        public void TryEnableCooker_NoData_False()
        {
            this.DefaultSet.AddDataSource(new FileDataSource("test" + Source123DataSource.Extension));
            var createInfo = new EngineCreateInfo(this.DefaultSet.AsReadOnly());

            using var sut = Engine.Create(createInfo);

            Assert.IsFalse(sut.TryEnableCooker(Source4DataCooker.DataCookerPath));
        }

        #endregion TryEnableCooker

        #region EnableTable

        [TestMethod]
        [IntegrationTest]
        public void EnableTable_Known_Enables()
        {
            using var sut = Engine.Create(new EngineCreateInfo(this.DefaultSet.AsReadOnly()));

            var table = sut.AvailableTables.FirstOrDefault();

            sut.EnableTable(table);

            Assert.AreEqual(1, sut.EnabledTables.Count());
            Assert.AreEqual(table, sut.EnabledTables.ElementAt(0));
        }

        [TestMethod]
        [IntegrationTest]
        public void EnableTable_Known_EmptyTable()
        {
            using var sut = Engine.Create(new EngineCreateInfo(this.DefaultSet.AsReadOnly()));

            sut.EnableTable(EmptyTable.TableDescriptor);

            Assert.AreEqual(1, sut.EnabledTables.Count());
            Assert.AreEqual(EmptyTable.TableDescriptor, sut.EnabledTables.ElementAt(0));
        }

        [TestMethod]
        [IntegrationTest]
        public void EnableTable_NotKnown_Throws()
        {
            using var sut = Engine.Create(new EngineCreateInfo(this.DefaultSet.AsReadOnly()));

            var table = new TableDescriptor(Guid.NewGuid(), "Unknown Table", "Unknown Table");

            var e = Assert.ThrowsException<TableNotFoundException>(() => sut.EnableTable(table));
            Assert.AreEqual(table, e.Descriptor);
        }

        [TestMethod]
        [IntegrationTest]
        public void EnableTable_AlreadyProcessed_Throws()
        {
            using var sut = Engine.Create(new EngineCreateInfo(this.DefaultSet.AsReadOnly()));

            sut.Process();

            Assert.ThrowsException<InstanceAlreadyProcessedException>(() => sut.EnableTable(sut.AvailableTables.First()));
        }

        #endregion EnableTable

        #region TryEnableTable

        [TestMethod]
        [IntegrationTest]
        public void TryEnableTable_Known_Enables()
        {
            using var sut = Engine.Create(new EngineCreateInfo(this.DefaultSet.AsReadOnly()));

            var table = sut.AvailableTables.FirstOrDefault();

            sut.TryEnableTable(table);

            Assert.AreEqual(1, sut.EnabledTables.Count());
            Assert.AreEqual(table, sut.EnabledTables.ElementAt(0));
        }

        [TestMethod]
        [IntegrationTest]
        public void TryEnableTable_Known_Enables_True()
        {
            using var sut = Engine.Create(new EngineCreateInfo(this.DefaultSet.AsReadOnly()));

            var table = sut.AvailableTables.FirstOrDefault();

            Assert.IsTrue(sut.TryEnableTable(table));

            Assert.AreEqual(1, sut.EnabledTables.Count());
            Assert.AreEqual(table, sut.EnabledTables.ElementAt(0));
        }

        [TestMethod]
        [IntegrationTest]
        public void TryEnableTable_Known_EmptyTable()
        {
            using var sut = Engine.Create(new EngineCreateInfo(this.DefaultSet.AsReadOnly()));

            sut.TryEnableTable(EmptyTable.TableDescriptor);

            Assert.AreEqual(1, sut.EnabledTables.Count());
            Assert.AreEqual(EmptyTable.TableDescriptor, sut.EnabledTables.ElementAt(0));
        }

        [TestMethod]
        [IntegrationTest]
        public void TryEnableTable_Known_EmptyTable_True()
        {
            using var sut = Engine.Create(new EngineCreateInfo(this.DefaultSet.AsReadOnly()));

            Assert.IsTrue(sut.TryEnableTable(EmptyTable.TableDescriptor));

            Assert.AreEqual(1, sut.EnabledTables.Count());
            Assert.AreEqual(EmptyTable.TableDescriptor, sut.EnabledTables.ElementAt(0));
        }

        [TestMethod]
        [IntegrationTest]
        public void TryEnableTable_NotKnown_Throws()
        {
            using var sut = Engine.Create(new EngineCreateInfo(this.DefaultSet.AsReadOnly()));

            var table = new TableDescriptor(Guid.NewGuid(), "Unknown Table", "Unknown Table");

            Assert.IsFalse(sut.TryEnableTable(table));
        }

        [TestMethod]
        [IntegrationTest]
        public void TryEnableTable_AlreadyProcessed_Throws()
        {
            using var sut = Engine.Create(new EngineCreateInfo(this.DefaultSet.AsReadOnly()));

            sut.Process();

            Assert.IsFalse(sut.TryEnableTable(sut.AvailableTables.First()));
        }

        #endregion TryEnableTable

        #region BuildTable

        [TestMethod]
        [IntegrationTest]
        public void BuildTable_EmptyTable()
        {
            using var sut = Engine.Create(new EngineCreateInfo(this.DefaultSet.AsReadOnly()));

            sut.EnableTable(EmptyTable.TableDescriptor);

            var result = sut.Process();

            var hasData = result.IsTableDataAvailable(EmptyTable.TableDescriptor);

            Assert.IsFalse(hasData.HasValue);

            var builtTable = result.BuildTable(EmptyTable.TableDescriptor);

            Assert.AreEqual(0, builtTable.RowCount);
            Assert.AreEqual(0, builtTable.Columns.Count());
            Assert.AreEqual(0, builtTable.BuiltInTableConfigurations.Count());
            Assert.AreEqual(0, builtTable.TableCommands.Count());
            Assert.IsNull(builtTable.DefaultConfiguration);
            Assert.IsNull(builtTable.TableRowDetailsGenerator);
        }

        [TestMethod]
        [IntegrationTest]
        public void BuildTable_EmptyTable_IsDataAvailable()
        {
            using var sut = Engine.Create(new EngineCreateInfo(this.DefaultSet.AsReadOnly()));

            sut.EnableTable(EmptyTableWithIsData.TableDescriptor);

            var result = sut.Process();

            var hasData = result.IsTableDataAvailable(EmptyTableWithIsData.TableDescriptor);

            Assert.IsTrue(hasData.HasValue);
            Assert.IsFalse(hasData.Value);

            var builtTable = result.BuildTable(EmptyTableWithIsData.TableDescriptor);

            Assert.AreEqual(0, builtTable.RowCount);
            Assert.AreEqual(0, builtTable.Columns.Count());
            Assert.AreEqual(0, builtTable.BuiltInTableConfigurations.Count());
            Assert.AreEqual(0, builtTable.TableCommands.Count());
            Assert.IsNull(builtTable.DefaultConfiguration);
            Assert.IsNull(builtTable.TableRowDetailsGenerator);
        }

        [TestMethod]
        [IntegrationTest]
        public void BuildTable_SingleRowTable()
        {
            using var sut = Engine.Create(new EngineCreateInfo(this.DefaultSet.AsReadOnly()));

            sut.EnableTable(SingleRowTable.TableDescriptor);

            var result = sut.Process();

            var builtTable = result.BuildTable(SingleRowTable.TableDescriptor);

            Assert.AreEqual(1, builtTable.RowCount);
            Assert.AreEqual(3, builtTable.Columns.Count());
            Assert.AreEqual(0, builtTable.BuiltInTableConfigurations.Count());
            Assert.AreEqual(0, builtTable.TableCommands.Count());
            Assert.IsNull(builtTable.DefaultConfiguration);
            Assert.IsNull(builtTable.TableRowDetailsGenerator);

            for (int i = 0; i < builtTable.Columns.Count; i++)
            {
                Assert.AreEqual(i + 1, builtTable.Columns.ElementAt(i).Project(0));
            }
        }

        [TestMethod]
        [FunctionalTest]
        [DeploymentItem(@"TestData/source123_test_data.s123d")]
        [DeploymentItem(@"TestData/source4_test_data.s4d")]
        [DeploymentItem(@"TestData/source5_test_data.s5d")]
        [DeploymentItem(@"TestData/BuildTableTestSuite.json")]
        [DynamicData(nameof(BuildTableTestData), DynamicDataSourceType.Method)]
        public void BuildTable_ProcessorSingleRowTable(
            EngineBuildTableTestCaseDto testCase)
        {
            if (testCase.DebugBreak)
            {
                System.Diagnostics.Debugger.Break();
            }

            using var plugins = PluginSet.Load(
                new[]
                {
                    Environment.CurrentDirectory,
                },
                new IsolationAssemblyLoader());
            using var dataSources = DataSourceSet.Create(plugins);

            foreach (var file in testCase.FilePaths)
            {
                dataSources.AddFile(file);
            }

            using var sut = Engine.Create(new EngineCreateInfo(dataSources.AsReadOnly()));

            sut.EnableTable(Source123Table.TableDescriptor);

            var result = sut.Process();

            var builtTable = result.BuildTable(Source123Table.TableDescriptor);

            Assert.AreEqual(1, builtTable.RowCount);
            Assert.AreEqual(3, builtTable.Columns.Count());
            Assert.AreEqual(0, builtTable.BuiltInTableConfigurations.Count());
            Assert.AreEqual(0, builtTable.TableCommands.Count());
            Assert.IsNull(builtTable.DefaultConfiguration);
            Assert.IsNull(builtTable.TableRowDetailsGenerator);

            for (int i = 0; i < builtTable.Columns.Count; i++)
            {
                Assert.AreEqual(i + 1, builtTable.Columns.ElementAt(i).Project(0));
            }
        }

        [TestMethod]
        [FunctionalTest]
        [DeploymentItem(@"TestData/source123_test_data.s123d")]
        [DeploymentItem(@"TestData/source4_test_data.s4d")]
        [DeploymentItem(@"TestData/source5_test_data.s5d")]
        [DeploymentItem(@"TestData/BuildTableTestSuite.json")]
        [DynamicData(nameof(BuildTableTestData), DynamicDataSourceType.Method)]
        public void BuildTable_MultiRowFromCookerTable(
            EngineBuildTableTestCaseDto testCase)
        {
            if (testCase.DebugBreak)
            {
                Debugger.Break();
            }

            using var plugins = PluginSet.Load(
                new[]
                {
                    Environment.CurrentDirectory,
                },
                new IsolationAssemblyLoader());
            using var dataSources = DataSourceSet.Create(plugins);

            foreach (var file in testCase.FilePaths)
            {
                dataSources.AddFile(file);
            }

            using var sut = Engine.Create(new EngineCreateInfo(dataSources.AsReadOnly()));

            foreach (var tableGuid in testCase.TablesToEnable)
            {
                sut.EnableTable(sut.AvailableTables.Single(x => x.Guid == Guid.Parse(tableGuid)));
            }

            var result = sut.Process();

            foreach (var expectedData in testCase.ExpectedOutputs)
            {
                var tableGuid = expectedData.Key;
                var dataPoints = expectedData.Value;

                var builtTable = result.BuildTable(sut.EnabledTables.Single(x => x.Guid == Guid.Parse(tableGuid)));

                Assert.IsNotNull(builtTable);

                for (int i = 0; i < dataPoints.Length; ++i)
                {
                    var dataPoint = dataPoints[i];

                    foreach (var data in dataPoint)
                    {
                        var column = builtTable.Columns.SingleOrDefault(x => x.Configuration.Metadata.Name == data.Key);

                        Assert.IsNotNull(column);

                        Assert.AreEqual(data.Value, column.Project(i).ToString());
                    }
                }
            }

            foreach (var throwingTable in testCase.ThrowingTables)
            {
                Assert.ThrowsException<TableNotEnabledException>(() => result.BuildTable(sut.AvailableTables.Single(x => x.Guid == Guid.Parse(throwingTable))));
            }
        }

        [TestMethod]
        [FunctionalTest]
        [DeploymentItem(@"TestData/source123_test_data.s123d")]
        [DeploymentItem(@"TestData/source4_test_data.s4d")]
        [DeploymentItem(@"TestData/source5_test_data.s5d")]
        [DeploymentItem(@"TestData/BuildTableTestSuite.json")]
        [DynamicData(nameof(BuildTableTestData), DynamicDataSourceType.Method)]
        public void BuildTable_InternalTables(EngineBuildTableTestCaseDto testCase)
        {
            if (testCase.DebugBreak)
            {
                Debugger.Break();
            }

            using var plugins = PluginSet.Load(
                new[]
                {
                    Environment.CurrentDirectory,
                },
                new IsolationAssemblyLoader());
            using var dataSources = DataSourceSet.Create(plugins);

            foreach (var file in testCase.FilePaths)
            {
                dataSources.AddFile(file);
            }

            using var sut = Engine.Create(new EngineCreateInfo(dataSources.AsReadOnly()));

            Assert.IsTrue(sut.TryEnableTable(Source5MetadataTable.TableDescriptor));
            Assert.IsTrue(sut.TryEnableTable(CrossParserMetadataTable.TableDescriptor));

            var result = sut.Process();

            var builtTable1 = result.BuildTable(Source5MetadataTable.TableDescriptor);
            result.BuildTable(CrossParserMetadataTable.TableDescriptor);

            Assert.AreEqual(1, builtTable1.RowCount);
            Assert.AreEqual(2, builtTable1.Columns.Count());
            Assert.AreEqual(0, builtTable1.BuiltInTableConfigurations.Count());
            Assert.AreEqual(0, builtTable1.TableCommands.Count());
            Assert.IsNull(builtTable1.DefaultConfiguration);
            Assert.IsNull(builtTable1.TableRowDetailsGenerator);

            for (int i = 0; i < builtTable1.Columns.Count; i++)
            {
                Assert.AreEqual(5, builtTable1.Columns.ElementAt(i).Project(0));
            }

            // Metadata tables are no longer enabled by default and this should fail as it was not explicitly enabled
            Assert.IsFalse(result.TryBuildTable(Source5MetadataTable2.TableDescriptor, out var builtTable2));
        }

        private static IEnumerable<object[]> BuildTableTestData()
        {
            var suite = EngineTestsLoader.Load<EngineBuildTableTestSuiteDto>("TestData/BuildTableTestSuite.json");
            foreach (var testCase in suite.TestCases)
            {
                yield return new[] { testCase, };
            }
        }

        #endregion BuildTable

        #region Process

        [TestMethod]
        [FunctionalTest]
        public void Process_WhenComplete_IsProcessedSetToTrue()
        {
            using var sut = Engine.Create(new EngineCreateInfo(this.DefaultSet.AsReadOnly()));

            sut.Process();

            Assert.IsTrue(sut.IsProcessed);
        }

        [TestMethod]
        [FunctionalTest]
        public void Process_NothingEnabled_DoesNothing()
        {
            using var sut = Engine.Create(new EngineCreateInfo(this.DefaultSet.AsReadOnly()));

            var results = sut.Process();

            Assert.IsNotNull(results);
            Assert.IsTrue(sut.IsProcessed);
        }

        [TestMethod]
        [FunctionalTest]
        public void Process_AlreadyProcessed_Throws()
        {
            using var sut = Engine.Create(new EngineCreateInfo(this.DefaultSet.AsReadOnly()));

            sut.Process();

            Assert.ThrowsException<InstanceAlreadyProcessedException>(() => sut.Process());
        }

        [TestMethod]
        [FunctionalTest]
        [DeploymentItem(@"TestData/source123_test_data.s123d")]
        [DeploymentItem(@"TestData/source4_test_data.s4d")]
        [DeploymentItem(@"TestData/source5_test_data.s5d")]
        [DeploymentItem(@"TestData/ProcessTestSuite.json")]
        [DynamicData(nameof(ProcessTestData), DynamicDataSourceType.Method)]
        public void Process_WhenComplete_DataWasProcessed(
            EngineProcessTestCaseDto testCase)
        {
            if (testCase.DebugBreak)
            {
                System.Diagnostics.Debugger.Break();
            }

            using var plugins = PluginSet.Load(
                new[]
                {
                    Environment.CurrentDirectory,
                },
                new IsolationAssemblyLoader());
            using var dataSources = DataSourceSet.Create(plugins);

            foreach (var file in testCase.FilePaths)
            {
                dataSources.AddDataSource(new FileDataSource(file));
            }

            using var sut = Engine.Create(new EngineCreateInfo(dataSources.AsReadOnly()));

            foreach (var cooker in testCase.SourceCookersToEnable ?? Array.Empty<EngineProcessDataCookerPathDto>())
            {
                var cookerPath = DataCookerPath.ForSource(cooker.SourceParserId, cooker.DataCookerId);
                Assert.IsTrue(sut.TryEnableCooker(cookerPath), "Unable to enable cooker '{0}'", cookerPath);
            }

            foreach (var cooker in testCase.CompositeCookersToEnable ?? Array.Empty<EngineProcessDataCookerPathDto>())
            {
                var cookerPath = DataCookerPath.ForComposite(cooker.DataCookerId);
                Assert.IsTrue(sut.TryEnableCooker(cookerPath), "Unable to enable cooker '{0}'", cookerPath);
            }

            var results = sut.Process();

            void assertOutput(
                Dictionary<string, string>[] expectedDataPoints,
                object data,
                DataOutputPath dataOutputPath)
            {
                var enumerableData = data as IEnumerable;
                Assert.IsNotNull(
                    enumerableData,
                    "Test output data must implement IEnumerable<> (type wasn't enumerable): '{0}'",
                    data.GetType());
                var enumerableType = enumerableData.GetType();
                var eInterface = enumerableType.GetInterface(typeof(IEnumerable<>).Name);
                Assert.IsNotNull(
                    eInterface,
                    "Test output data must implement IEnumerable<> (interface wasn't found): {0}",
                    string.Join(", ", data.GetType().GetInterfaces().Select(x => x.FullName)));
                var collectionType = eInterface.GetGenericArguments()[0];
                Assert.IsNotNull(collectionType, "Unable to retrieve collection type for {0}", data.GetType());

                var enumeratedData = new List<object>();
                foreach (var o in enumerableData)
                {
                    enumeratedData.Add(o);
                }

                Assert.AreEqual(
                    expectedDataPoints.Length,
                    enumeratedData.Count,
                    "The processor did not process the correct amount of data: {0}",
                    dataOutputPath);

                var properties = collectionType.GetProperties()
                    .ToDictionary(x => x.Name, x => x, StringComparer.InvariantCultureIgnoreCase);

                for (var i = 0; i < expectedDataPoints.Length; ++i)
                {
                    var expectedObject = expectedDataPoints[i];
                    var actualObject = enumeratedData[i];
                    foreach (var kvp in expectedObject)
                    {
                        var propertyName = kvp.Key;
                        var expectedValue = kvp.Value;

                        Assert.IsTrue(properties.TryGetValue(propertyName, out PropertyInfo property));
                        var actualValue = property.GetValue(actualObject)?.ToString();
                        Assert.AreEqual(expectedValue, actualValue, propertyName);
                    }
                }
            }

            foreach (var expectedData in testCase.ExpectedOutputs)
            {
                string dataOutputPathRaw = expectedData.Key;
                var expectedDataPoints = expectedData.Value;

                DataOutputPath dataOutputPath = Parse(dataOutputPathRaw);

                Assert.IsTrue(
                    results.TryQueryOutput(dataOutputPath, out object data),
                    "Output for {0} not found.",
                    dataOutputPathRaw);
                Assert.IsNotNull(data, "output for {0} was null ???", dataOutputPathRaw);
                assertOutput(expectedDataPoints, data, dataOutputPath);

                Assert.IsTrue(
                    results.TryGetCookedData(dataOutputPath.CookerPath, out var co),
                    "Unable to retrieve CookerOutput: {0}",
                    dataOutputPath.CookerPath);
                Assert.IsTrue(
                    co.TryQueryOutput(dataOutputPath.OutputId, out data),
                    "Output on CookedData {0} not found.",
                    dataOutputPathRaw);
                assertOutput(expectedDataPoints, data, dataOutputPath);
            }

            var throwingSourcePaths = testCase.ThrowingOutputs
                .Select(x => (x, Parse(x)))
                .Where(x => x.Item2.CookerPath.DataCookerType == DataCookerType.SourceDataCooker)
                .ToDictionary(x => x.Item1, x => x.Item2);
            var throwingCompositePaths = testCase.ThrowingOutputs
                .Select(x => (x, Parse(x)))
                .Where(x => x.Item2.CookerPath.DataCookerType == DataCookerType.CompositeDataCooker)
                .ToDictionary(x => x.Item1, x => x.Item2);

            foreach (var kvp in throwingSourcePaths)
            {
                string dataOutputPathRaw = kvp.Key;
                DataOutputPath dataOutputPath = kvp.Value;

                Assert.IsFalse(
                    results.TryQueryOutput(dataOutputPath, out var _),
                    "Output should not have been available: {0}",
                    dataOutputPathRaw);
                Assert.IsTrue(
                    results.TryGetCookedData(dataOutputPath.CookerPath, out var co),
                    "Source Output should have been available: {0}",
                    dataOutputPathRaw);
                Assert.IsFalse(
                    co.TryQueryOutput(dataOutputPath.OutputId, out var _),
                    "Output should not have been available: {0}",
                    dataOutputPathRaw);
            }

            foreach (var kvp in throwingCompositePaths)
            {
                string dataOutputPathRaw = kvp.Key;
                DataOutputPath dataOutputPath = kvp.Value;

                Assert.IsFalse(
                    results.TryQueryOutput(dataOutputPath, out var _),
                    "Output should not have been available: {0}",
                    dataOutputPathRaw);
                Assert.IsFalse(
                    results.TryGetCookedData(dataOutputPath.CookerPath, out var _),
                    "Composite Output should not have been available: {0}",
                    dataOutputPathRaw);
            }
        }

        private static IEnumerable<object[]> ProcessTestData()
        {
            var suite = EngineTestsLoader.Load<EngineProcessTestSuiteDto>("TestData/ProcessTestSuite.json");
            foreach (var testCase in suite.TestCases)
            {
                yield return new[] { testCase, };
            }
        }

        #endregion

        #region Isolation

        [TestMethod]
        [FunctionalTest]
        [DeploymentItem(@"TestData/source123_test_data.s123d")]
        [DeploymentItem(@"TestData/source4_test_data.s4d")]
        [DeploymentItem(@"TestData/source5_test_data.s5d")]
        [DeploymentItem(@"TestData/ProcessTestSuite.json")]
        [DynamicData(nameof(ProcessTestData), DynamicDataSourceType.Method)]
        public void Process_Isolated_WhenComplete_DataWasProcessed(
            EngineProcessTestCaseDto testCase)
        {
            if (testCase.DebugBreak)
            {
                Debugger.Break();
            }

            using var plugins = PluginSet.Load(
                new[]
                {
                    Environment.CurrentDirectory,
                },
                new IsolationAssemblyLoader());
            using var dataSources = DataSourceSet.Create(plugins);

            foreach (var file in testCase.FilePaths)
            {
                dataSources.AddDataSource(new FileDataSource(file));
            }

            var runtime = Engine.Create(new EngineCreateInfo(dataSources.AsReadOnly()));

            foreach (var cooker in testCase.SourceCookersToEnable ?? Array.Empty<EngineProcessDataCookerPathDto>())
            {
                var cookerPath = DataCookerPath.ForSource(cooker.SourceParserId, cooker.DataCookerId);
                Assert.IsTrue(runtime.TryEnableCooker(cookerPath), "Unable to enable cooker '{0}'", cookerPath);
            }

            foreach (var cooker in testCase.CompositeCookersToEnable ?? Array.Empty<EngineProcessDataCookerPathDto>())
            {
                var cookerPath = DataCookerPath.ForComposite(cooker.DataCookerId);
                Assert.IsTrue(runtime.TryEnableCooker(cookerPath), "Unable to enable cooker '{0}'", cookerPath);
            }

            var results = runtime.Process();

            foreach (var expectedData in testCase.ExpectedOutputs)
            {
                var dataOutputPathRaw = expectedData.Key;
                var expectedDataPoints = expectedData.Value;
                DataOutputPath dataOutputPath = Parse(dataOutputPathRaw);

                Assert.IsTrue(
                    results.TryQueryOutput(dataOutputPath, out object data), "Output for {0} not found.", dataOutputPathRaw);
                Assert.IsNotNull(data, "output for {0} was null ???", dataOutputPathRaw);

                var enumerableData = data as IEnumerable;
                Assert.IsNotNull(
                    enumerableData,
                    "Test output data must implement IEnumerable<> (type wasn't enumerable): '{0}'",
                    data.GetType());
                var enumerableType = enumerableData.GetType();
                var eInterface = enumerableType.GetInterface(typeof(IEnumerable<>).Name);
                Assert.IsNotNull(
                    eInterface,
                    "Test output data must implement IEnumerable<> (interface wasn't found): {0}",
                    string.Join(", ", data.GetType().GetInterfaces().Select(x => x.FullName)));
                var collectionType = eInterface.GetGenericArguments()[0];
                Assert.IsNotNull(collectionType, "Unable to retrieve collection type for {0}", data.GetType());

                var enumeratedData = new List<object>();
                foreach (var o in enumerableData)
                {
                    enumeratedData.Add(o);
                }

                Assert.AreEqual(
                    expectedDataPoints.Length,
                    enumeratedData.Count,
                    "The processor did not process the correct amount of data: {0}",
                    dataOutputPath);

                var properties = collectionType.GetProperties()
                    .ToDictionary(x => x.Name, x => x, StringComparer.InvariantCultureIgnoreCase);

                for (var i = 0; i < expectedDataPoints.Length; ++i)
                {
                    var expectedObject = expectedDataPoints[i];
                    var actualObject = enumeratedData[i];
                    foreach (var kvp in expectedObject)
                    {
                        var propertyName = kvp.Key;
                        var expectedValue = kvp.Value;

                        Assert.IsTrue(properties.TryGetValue(propertyName, out PropertyInfo property));
                        var actualValue = property.GetValue(actualObject)?.ToString();
                        Assert.AreEqual(expectedValue, actualValue, propertyName);
                    }
                }
            }

            foreach (var dataOutputPathRaw in testCase.ThrowingOutputs)
            {
                DataOutputPath dataOutputPath = Parse(dataOutputPathRaw);
                Assert.IsFalse(
                    results.TryQueryOutput(dataOutputPath, out var _),
                    "Output should not have been available: {0}",
                    dataOutputPathRaw);
            }
        }

        #endregion

        #region Dispose

        [TestMethod]
        [UnitTest]
        public void WhenDisposed_EverythingThrows()
        {
            using var sut = Engine.Create(new EngineCreateInfo(this.DefaultSet.AsReadOnly()));

            sut.Dispose();

            Assert.ThrowsException<ObjectDisposedException>(() => sut.AvailableTables);
            Assert.ThrowsException<ObjectDisposedException>(() => sut.CreateInfo);
            Assert.ThrowsException<ObjectDisposedException>(() => sut.DataSourcesToProcess);
            Assert.ThrowsException<ObjectDisposedException>(() => sut.EnabledCookers);
            Assert.ThrowsException<ObjectDisposedException>(() => sut.EnabledTables);
            Assert.ThrowsException<ObjectDisposedException>(() => sut.IsProcessed);
            Assert.ThrowsException<ObjectDisposedException>(() => sut.Plugins);

            Assert.ThrowsException<ObjectDisposedException>(() => sut.EnableCooker(Any.DataCookerPath()));
            Assert.ThrowsException<ObjectDisposedException>(() => sut.Process());
            Assert.ThrowsException<ObjectDisposedException>(() => sut.TryEnableCooker(Any.DataCookerPath()));
        }

        [TestMethod]
        [UnitTest]
        public void CanDisposeMultipleTimes()
        {
            using var sut = Engine.Create(new EngineCreateInfo(this.DefaultSet.AsReadOnly()));

            sut.Dispose();
            sut.Dispose();
            sut.Dispose();
        }

        [TestMethod]
        [IntegrationTest]
        public void Dispose_NeverTakesOwnershipOfPlugins()
        {
            using var plugins = PluginSet.Load();

            var file = new FileDataSource("test" + Source123DataSource.Extension);
            using var dataSources = DataSourceSet.Create(plugins, true);
            dataSources.AddDataSource(new FileDataSource("test" + Source123DataSource.Extension));

            var info = new EngineCreateInfo(dataSources.AsReadOnly());

            {
                using var sut = Engine.Create(info);
                var clone = sut.DataSourcesToProcess;

                sut.Dispose();

                //
                // Touch the plugins to make sure they stayed around.
                //

                var cookers = plugins.AllCookers;
                Assert.IsNotNull(cookers);
            }
        }

        #endregion

        #region Interactive

        [TestMethod]
        [FunctionalTest]
        [DeploymentItem(@"TestData/interactive.ips")]
        public void NonInteractive_ProcessThrows()
        {
            using var set = DataSourceSet.Create();

            var file = new FileDataSource(@"TestData\interactive.ips");
            set.AddDataSource(file);

            using var sut = Engine.Create(
                new EngineCreateInfo(set.AsReadOnly())
                {
                    IsInteractive = false,
                });

            var rer = sut.Process();

            Assert.AreEqual(1, rer.ProcessingErrors.Count);
            var error = rer.ProcessingErrors.Single();

            Assert.IsInstanceOfType(error.Processor, typeof(InteractiveProcessor));
            Assert.AreEqual(file, error.DataSources.Single());
            Assert.IsInstanceOfType(error.ProcessFault, typeof(InvalidOperationException));
        }

        #endregion

        #region Auth

        [TestMethod]
        [UnitTest]
        public void ProcessingSources_AreGiven_SpecifiedAuthProviders()
        {
            var givenProvider = new StubAuthProvider();

            var info = new EngineCreateInfo(this.DefaultSet.AsReadOnly());
            info.WithAuthProvider(givenProvider);
            using var sut = Engine.Create(info);

            var spy = sut.Plugins.ProcessingSourceReferences.First(psr => psr.Guid == Source123DataSource.Guid).Instance as Source123DataSource;
            var success = spy.ApplicationEnvironmentSpy.TryGetAuthProvider<StubAuthMethod, int>(out var foundProvider);

            Assert.IsTrue(success);
            Assert.AreEqual(givenProvider, foundProvider);
        }

        [TestMethod]
        [UnitTest]
        public void TryGetAuthProvider_Fails_WhenNoProviderIsRegistered()
        {
            var info = new EngineCreateInfo(this.DefaultSet.AsReadOnly());
            using var sut = Engine.Create(info);

            var spy = sut.Plugins.ProcessingSourceReferences.First(psr => psr.Guid == Source123DataSource.Guid).Instance as Source123DataSource;
            var success = spy.ApplicationEnvironmentSpy.TryGetAuthProvider<StubAuthMethod, int>(out var foundProvider);

            Assert.IsFalse(success);
            Assert.IsNull(foundProvider);
        }

        [TestMethod]
        [UnitTest]
        [DataRow("test value")]
        [DataRow(Source123DataSource.FieldOptionDefaultValue)] // Even the default value should be settable, which should also result in IsUsingDefault being false
        public void WithPluginOptionValue_UpdatesPluginOption(string valueToSet)
        {
            var info = new EngineCreateInfo(this.DefaultSet.AsReadOnly());
            info.WithPluginOptionValue<FieldOptionValue, string>(Source123DataSource.FieldOptionGuid, valueToSet);
            using var sut = Engine.Create(info);

            var spy = sut.Plugins.ProcessingSourceReferences.First(psr => psr.Guid == Source123DataSource.Guid).Instance as Source123DataSource;

            var success =
                spy.ApplicationEnvironmentSpy.TryGetPluginOption(
                    Source123DataSource.FieldOptionGuid,
                    out FieldOptionValue option);

            Assert.IsTrue(success, "Expected to successfully get the plugin option from the application environment");
            Assert.AreEqual(valueToSet, option.CurrentValue, $"Expected the plugin option value to be '{valueToSet}' but found '{option.CurrentValue}'");
        }

        [TestMethod]
        [UnitTest]
        public void WithPluginOptionValue_DoesNotAlter_UnspecifiedPluginOptionValues()
        {
            var info = new EngineCreateInfo(this.DefaultSet.AsReadOnly());
            info.WithPluginOptionValue<FieldOptionValue, string>(Source123DataSource.FieldOptionGuid, "random value");
            using var sut = Engine.Create(info);

            var spy = sut.Plugins.ProcessingSourceReferences.First(psr => psr.Guid == Source123DataSource.Guid).Instance as Source123DataSource;

            var success =
                spy.ApplicationEnvironmentSpy.TryGetPluginOption(
                    Source123DataSource.BooleanOptionGuid,
                    out BooleanOptionValue option);

            Assert.IsTrue(success, "Expected to successfully get the plugin option from the application environment");
            Assert.AreEqual(Source123DataSource.BooleanOptionDefaultValue, option.CurrentValue, $"Expected the plugin option value to be its default value but found '{option.CurrentValue}'");
        }

        private class StubAuthMethod
            : IAuthMethod<int>
        {
        }

        private class StubAuthProvider
            : IAuthProvider<StubAuthMethod, int>
        {
            public Task<int> TryGetAuth(StubAuthMethod authRequest)
            {
                throw new NotImplementedException();
            }
        }

        #endregion

        public static DataOutputPath Parse(string dataCookerOutputPath)
        {
            var split = dataCookerOutputPath.Split('/');
            Debug.Assert(split.Length == 3);

            string parserId = split[0];
            string cookerId = split[1];
            string outputId = split[2];

            if (string.IsNullOrWhiteSpace(parserId))
            {
                return DataOutputPath.ForComposite(cookerId, outputId);
            }

            return DataOutputPath.ForSource(parserId, cookerId, outputId);
        }
    }
}
