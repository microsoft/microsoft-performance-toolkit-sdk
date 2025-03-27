// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.

using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.Performance.SDK.Options;
using Microsoft.Performance.SDK.Options.Definitions;
using Microsoft.Performance.SDK.Processing;
using Microsoft.Performance.SDK.Processing.DataSourceGrouping;
using Microsoft.Performance.SDK.Runtime.Options;
using Microsoft.Performance.Testing;
using Microsoft.Performance.Testing.SDK;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace Microsoft.Performance.SDK.Runtime.Tests
{
    [TestClass]
    public class ProcessingSourceExecutorTests
    {
        private CancellationTokenSource Cts { get; set; }
        private ProcessingSourceExecutor Sut { get; set; }

        [TestInitialize]
        public void Initialize()
        {
            this.Cts = new CancellationTokenSource();
            this.Sut = new ProcessingSourceExecutor();
        }

        [TestCleanup]
        public void Cleanup()
        {
            this.Cts.Dispose();
        }

        [TestMethod]
        [UnitTest]
        public void ExecuteWithNullExecutionContextThrows()
        {
            Assert.ThrowsException<ArgumentNullException>(
                () => this.Sut.InitializeCustomDataProcessor(null));
        }

        [TestMethod]
        [UnitTest]
        public void ExecuteSourceThatReturnsNullThrows()
        {
            var progress = new DataProcessorProgress();

            var fakeProcessingSource = new MockProcessingSource()
            {
                DataProcessor = null,
            };

            var dataSourceGroup = Any.DataSourceGroup();

            var tables = new[]
            {
                Any.TableDescriptor(),
                Any.TableDescriptor(),
                Any.TableDescriptor(),
            };

            var executionContext = new ExecutionContext(
                progress,
                _ => new NullLogger(),
                CreateReference(fakeProcessingSource),
                dataSourceGroup,
                tables,
                Any.ProcessorEnvironment(),
                ProcessorOptions.Default);

            Assert.ThrowsException<InvalidOperationException>(
                () => this.Sut.InitializeCustomDataProcessor(executionContext));
        }

        [TestMethod]
        [UnitTest]
        public void ExecutePassesEnvironmentToProcessor()
        {
            var progress = new DataProcessorProgress();

            var mockProcessor = new MockCustomDataProcessor();
            var fakeProcessingSource = new MockProcessingSource()
            {
                DataProcessor = mockProcessor,
            };

            var dataSourceGroup = Any.DataSourceGroup();

            var tables = new[]
            {
                Any.TableDescriptor(),
                Any.TableDescriptor(),
                Any.TableDescriptor(),
            };

            var env = Any.ProcessorEnvironment();
            var options = ProcessorOptions.Default;

            var executionContext = new ExecutionContext(
                progress,
                _ => new NullLogger(),
                CreateReference(fakeProcessingSource),
                dataSourceGroup,
                tables,
                env,
                options);

            this.Sut.InitializeCustomDataProcessor(executionContext);

            Assert.AreEqual(
                env,
                fakeProcessingSource.CreateProcessorCalls.Single().Item2);
        }

        [TestMethod]
        [UnitTest]
        public void ExecutePassesOptionsToProcessor()
        {
            var progress = new DataProcessorProgress();

            var mockProcessor = new MockCustomDataProcessor();
            var fakeProcessingSource = new MockProcessingSource()
            {
                DataProcessor = mockProcessor,
            };

            var dataSourceGroup = Any.DataSourceGroup();

            var tables = new[]
            {
                Any.TableDescriptor(),
                Any.TableDescriptor(),
                Any.TableDescriptor(),
            };

            var options = ProcessorOptions.Default;

            var executionContext = new ExecutionContext(
                progress,
                _ => new NullLogger(),
                CreateReference(fakeProcessingSource),
                dataSourceGroup,
                tables,
                Any.ProcessorEnvironment(),
                options);

            this.Sut.InitializeCustomDataProcessor(executionContext);

            Assert.AreEqual(
                executionContext.CommandLineOptions,
                fakeProcessingSource.CreateProcessorCalls.Single().Item3);
        }

        [TestMethod]
        [UnitTest]
        public void ExecuteEnablesTheCorrectTables()
        {
            var progress = new DataProcessorProgress();

            var mockProcessor = new MockCustomDataProcessor();
            var fakeProcessingSource = new MockProcessingSource()
            {
                DataProcessor = mockProcessor,
            };

            var dataSourceGroup = Any.DataSourceGroup();

            var tables = new[]
            {
                Any.TableDescriptor(),
                Any.TableDescriptor(),
                Any.TableDescriptor(),
            };

            var executionContext = new ExecutionContext(
                progress,
                _ => new NullLogger(),
                CreateReference(fakeProcessingSource),
                dataSourceGroup,
                tables,
                Any.ProcessorEnvironment(),
                ProcessorOptions.Default);

            this.Sut.InitializeCustomDataProcessor(executionContext);

            Assert.AreEqual(tables.Length, mockProcessor.EnableTableCalls.Count);
            foreach (var table in tables)
            {
                Assert.IsTrue(mockProcessor.EnableTableCalls.Contains(table));
            }
        }

        [TestMethod]
        [UnitTest]
        public async Task ExecuteRequestedtablesSetCorrectly()
        {
            var progress = new DataProcessorProgress();

            var mockProcessor = new MockCustomDataProcessor();
            var fakeProcessingSource = new MockProcessingSource()
            {
                DataProcessor = mockProcessor,
            };

            var dataSourceGroup = Any.DataSourceGroup();

            var tables = new[]
            {
                Any.TableDescriptor(),
                Any.TableDescriptor(),
                Any.TableDescriptor(),
            };

            var executionContext = new ExecutionContext(
                progress,
                _ => new NullLogger(),
                CreateReference(fakeProcessingSource),
                dataSourceGroup,
                tables,
                Any.ProcessorEnvironment(),
                ProcessorOptions.Default);

            this.Sut.InitializeCustomDataProcessor(executionContext);
            var result = await this.Sut.ExecuteAsync(CancellationToken.None);

            Assert.AreEqual(tables.Length, result.RequestedTables.Count());
            foreach (var table in tables)
            {
                Assert.IsTrue(result.RequestedTables.Contains(table));
            }
        }

        [TestMethod]
        [UnitTest]
        public void ExecuteEnableTableFailureContinuesAnyway()
        {
            var progress = new DataProcessorProgress();

            var mockProcessor = new MockCustomDataProcessor();
            var fakeProcessingSource = new MockProcessingSource()
            {
                DataProcessor = mockProcessor,
            };

            var dataSourceGroup = Any.DataSourceGroup();

            var tables = new[]
            {
                Any.TableDescriptor(),
                Any.TableDescriptor(),
                Any.TableDescriptor(),
            };

            mockProcessor.EnableFailures[tables[1]] = new Exception();

            var executionContext = new ExecutionContext(
                progress,
                _ => new NullLogger(),
                CreateReference(fakeProcessingSource),
                dataSourceGroup,
                tables,
                Any.ProcessorEnvironment(),
                ProcessorOptions.Default);

            this.Sut.InitializeCustomDataProcessor(executionContext);

            Assert.AreEqual(tables.Length, mockProcessor.EnableTableCalls.Count);
            foreach (var table in tables)
            {
                Assert.IsTrue(mockProcessor.EnableTableCalls.Contains(table));
            }
        }

        [TestMethod]
        [UnitTest]
        public async Task ExecuteEnableTableFailureNotedCorrectly()
        {
            var progress = new DataProcessorProgress();

            var mockProcessor = new MockCustomDataProcessor();
            var fakeProcessingSource = new MockProcessingSource()
            {
                DataProcessor = mockProcessor,
            };

            var dataSourceGroup = Any.DataSourceGroup();

            var tables = new[]
            {
                Any.TableDescriptor(),
                Any.TableDescriptor(),
                Any.TableDescriptor(),
            };

            mockProcessor.EnableFailures[tables[1]] = new ArithmeticException();

            var executionContext = new ExecutionContext(
                progress,
                _ => new NullLogger(),
                CreateReference(fakeProcessingSource),
                dataSourceGroup,
                tables,
                Any.ProcessorEnvironment(),
                ProcessorOptions.Default);

            this.Sut.InitializeCustomDataProcessor(executionContext);
            var result = await this.Sut.ExecuteAsync(CancellationToken.None);

            Assert.AreEqual(mockProcessor.EnableFailures.Count, result.EnableFailures.Count);
            foreach (var expectedFailure in mockProcessor.EnableFailures)
            {
                Assert.AreEqual(
                    expectedFailure.Value,
                    result.EnableFailures[expectedFailure.Key]);
            }
        }

        [TestMethod]
        [UnitTest]
        public async Task ExecuteDoesNotBuildTables()
        {
            var progress = new DataProcessorProgress();

            var mockProcessor = new MockCustomDataProcessor();
            var fakeProcessingSource = new MockProcessingSource()
            {
                DataProcessor = mockProcessor,
            };

            var dataSourceGroup = Any.DataSourceGroup();

            var tables = new[]
            {
                Any.TableDescriptor(),
                Any.TableDescriptor(),
                Any.TableDescriptor(),
            };

            var executionContext = new ExecutionContext(
                progress,
                _ => new NullLogger(),
                CreateReference(fakeProcessingSource),
                dataSourceGroup,
                tables,
                Any.ProcessorEnvironment(),
                ProcessorOptions.Default);

            this.Sut.InitializeCustomDataProcessor(executionContext);
            var result = await this.Sut.ExecuteAsync(CancellationToken.None);

            Assert.AreEqual(0, mockProcessor.BuildTableCalls.Count);
        }

        [TestMethod]
        [UnitTest]
        public async Task ExecuteFailureToBuildMetadataTablesNoted()
        {
            var progress = new DataProcessorProgress();

            var mockProcessor = new MockCustomDataProcessor
            {
            };

            var fakeProcessingSource = new MockProcessingSource()
            {
                DataProcessor = mockProcessor,
            };

            var dataSourceGroup = Any.DataSourceGroup();

            var tablesToEnable = new[]
            {
                Any.TableDescriptor(),
                Any.TableDescriptor(),
                Any.TableDescriptor(),
            };

            var executionContext = new ExecutionContext(
                progress,
                _ => new NullLogger(),
                CreateReference(fakeProcessingSource),
                dataSourceGroup,
                tablesToEnable,
                Any.ProcessorEnvironment(),
                ProcessorOptions.Default);

            this.Sut.InitializeCustomDataProcessor(executionContext);
            var result = await this.Sut.ExecuteAsync(CancellationToken.None);
        }

        [TestMethod]
        [UnitTest]
        public async Task ExecuteDataSourceInfoSetCorrectly()
        {
            var progress = new DataProcessorProgress();

            var mockProcessor = new MockCustomDataProcessor
            {
                DataSourceInfo = new DataSourceInfo(3, 4, DateTime.UnixEpoch),
            };

            var fakeProcessingSource = new MockProcessingSource
            {
                DataProcessor = mockProcessor,
            };

            var dataSourceGroup = Any.DataSourceGroup();

            var tables = new[]
            {
                Any.TableDescriptor(),
            };

            var executionContext = new ExecutionContext(
                progress,
                _ => new NullLogger(),
                CreateReference(fakeProcessingSource),
                dataSourceGroup,
                tables,
                Any.ProcessorEnvironment(),
                ProcessorOptions.Default);

            this.Sut.InitializeCustomDataProcessor(executionContext);
            var result = await this.Sut.ExecuteAsync(CancellationToken.None);

            Assert.AreEqual(mockProcessor.DataSourceInfo, result.DataSourceInfo);
        }

        [TestMethod]
        [UnitTest]
        public async Task ExecuteDataSourceInfoIsNullCoalescesIntoDefault()
        {
            var progress = new DataProcessorProgress();

            var mockProcessor = new MockCustomDataProcessor
            {
                DataSourceInfo = null,
            };

            var fakeProcessingSource = new MockProcessingSource
            {
                DataProcessor = mockProcessor,
            };

            var dataSourceGroup = Any.DataSourceGroup();

            var tables = new[]
            {
                Any.TableDescriptor(),
            };

            var executionContext = new ExecutionContext(
                progress,
                _ => new NullLogger(),
                CreateReference(fakeProcessingSource),
                dataSourceGroup,
                tables,
                Any.ProcessorEnvironment(),
                ProcessorOptions.Default);

            this.Sut.InitializeCustomDataProcessor(executionContext);

            var result = await this.Sut.ExecuteAsync(CancellationToken.None);

            Assert.AreEqual(DataSourceInfo.Default, result.DataSourceInfo);
        }

        [TestMethod]
        [UnitTest]
        public async Task ExecuteDatSourceInfoFailureNoted()
        {
            var progress = new DataProcessorProgress();

            var mockProcessor = new MockCustomDataProcessor
            {
                DataSourceInfoFailure = new Exception(),
            };

            var fakeProcessingSource = new MockProcessingSource
            {
                DataProcessor = mockProcessor,
            };

            var dataSourceGroup = Any.DataSourceGroup();

            var tables = new[]
            {
                Any.TableDescriptor(),
            };

            var executionContext = new ExecutionContext(
                progress,
                _ => new NullLogger(),
                CreateReference(fakeProcessingSource),
                dataSourceGroup,
                tables,
                Any.ProcessorEnvironment(),
                ProcessorOptions.Default);

            this.Sut.InitializeCustomDataProcessor(executionContext);
            var result = await this.Sut.ExecuteAsync(CancellationToken.None);

            Assert.AreEqual(DataSourceInfo.Default, result.DataSourceInfo);
            Assert.AreEqual(mockProcessor.DataSourceInfoFailure, result.DataSourceInfoFailure);
        }

        [TestMethod]
        [UnitTest]
        public async Task ExecuteMetadataNameSetToSourceName()
        {
            var progress = new DataProcessorProgress();

            var mockProcessor = new MockCustomDataProcessor();
            var fakeProcessingSource = new MockProcessingSource
            {
                DataProcessor = mockProcessor,
            };

            var dataSourceGroup = Any.DataSourceGroup();

            var tables = new[]
            {
                Any.TableDescriptor(),
            };

            var executionContext = new ExecutionContext(
                progress,
                _ => new NullLogger(),
                CreateReference(fakeProcessingSource),
                dataSourceGroup,
                tables,
                Any.ProcessorEnvironment(),
                ProcessorOptions.Default);

            this.Sut.InitializeCustomDataProcessor(executionContext);
            var result = await this.Sut.ExecuteAsync(CancellationToken.None);

            Assert.AreEqual(fakeProcessingSource.TryGetName(), result.MetadataName);
        }

        [TestMethod]
        [UnitTest]
        public async Task ExecuteContextSetToArgument()
        {
            var progress = new DataProcessorProgress();

            var mockProcessor = new MockCustomDataProcessor();
            var fakeProcessingSource = new MockProcessingSource
            {
                DataProcessor = mockProcessor,
            };

            var dataSourceGroup = Any.DataSourceGroup();

            var tables = new[]
            {
                Any.TableDescriptor(),
            };

            var executionContext = new ExecutionContext(
                progress,
                _ => new NullLogger(),
                CreateReference(fakeProcessingSource),
                dataSourceGroup,
                tables,
                Any.ProcessorEnvironment(),
                ProcessorOptions.Default);

            this.Sut.InitializeCustomDataProcessor(executionContext);
            var result = await this.Sut.ExecuteAsync(CancellationToken.None);

            Assert.AreEqual(executionContext, result.Context);
        }

        [TestMethod]
        [UnitTest]
        public async Task ExecuteCallsProcessCorrectly()
        {
            var progress = new DataProcessorProgress();

            var logger = new NullLogger();

            var mockProcessor = new MockCustomDataProcessor();

            var fakeProcessingSource = new MockProcessingSource
            {
                DataProcessor = mockProcessor,
            };

            var dataSourceGroup = Any.DataSourceGroup();

            var tables = new[]
            {
                Any.TableDescriptor(),
            };

            var executionContext = new ExecutionContext(
                progress,
                _ => logger,
                CreateReference(fakeProcessingSource),
                dataSourceGroup,
                tables,
                Any.ProcessorEnvironment(),
                ProcessorOptions.Default);

            this.Sut.InitializeCustomDataProcessor(executionContext);
            var result = await this.Sut.ExecuteAsync(this.Cts.Token);

            Assert.AreEqual(1, mockProcessor.ProcessCalls.Count);
            Assert.AreEqual(progress, mockProcessor.ProcessCalls[0].Item1);
            Assert.AreEqual(this.Cts.Token, mockProcessor.ProcessCalls[0].Item2);
        }

        [TestMethod]
        [UnitTest]
        public async Task ExecuteProcessorFailsSetsFaultProperties()
        {
            var progress = new DataProcessorProgress();

            var mockProcessor = new MockCustomDataProcessor
            {
                ProcessFailure = new Exception(),
            };

            var fakeProcessingSource = new MockProcessingSource
            {
                DataProcessor = mockProcessor,
            };

            var dataSourceGroup = Any.DataSourceGroup();

            var tables = new[]
            {
                Any.TableDescriptor(),
            };

            var executionContext = new ExecutionContext(
                progress,
                _ => new NullLogger(),
                CreateReference(fakeProcessingSource),
                dataSourceGroup,
                tables,
                Any.ProcessorEnvironment(),
                ProcessorOptions.Default);

            this.Sut.InitializeCustomDataProcessor(executionContext);
            var result = await this.Sut.ExecuteAsync(CancellationToken.None);

            Assert.IsNotNull(result);
            Assert.AreEqual(DataSourceInfo.None, result.DataSourceInfo);
            Assert.AreEqual(mockProcessor.ProcessFailure, result.ProcessorFault);
            Assert.IsTrue(result.IsProcessorFaulted);
        }

        private static ProcessingSourceReference CreateReference(IProcessingSource source)
        {
            return new ProcessingSourceReference(source);
        }

        [ProcessingSource("{D6E5DC8D-E19D-4E55-99D9-746813C55A97}", "Test", "Test")]
        private sealed class MockProcessingSource
            : IProcessingSource
        {
            public MockProcessingSource()
            {
                this.CreateProcessorCalls = new List<Tuple<IEnumerable<IDataSource>, IProcessorEnvironment, ProcessorOptions>>();
            }

            public ICustomDataProcessor DataProcessor { get; set; }

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
                return this.CreateProcessor(
                    new[] { dataSource, },
                    processorEnvironment,
                    options);
            }

            public List<Tuple<IEnumerable<IDataSource>, IProcessorEnvironment, ProcessorOptions>> CreateProcessorCalls { get; }
            public ICustomDataProcessor CreateProcessor(IEnumerable<IDataSource> dataSources, IProcessorEnvironment processorEnvironment, ProcessorOptions options)
            {
                this.CreateProcessorCalls.Add(Tuple.Create(dataSources, processorEnvironment, options));
                return this.DataProcessor;
            }

            public ICustomDataProcessor CreateProcessor(IDataSourceGroup dataSourceGroup, IProcessorEnvironment processorEnvironment,
                ProcessorOptions options)
            {
                this.CreateProcessorCalls.Add(Tuple.Create((IEnumerable<IDataSource>)dataSourceGroup.DataSources, processorEnvironment, options));
                return this.DataProcessor;
            }

            public Stream GetSerializationStream(SerializationSource source)
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
    }
}
