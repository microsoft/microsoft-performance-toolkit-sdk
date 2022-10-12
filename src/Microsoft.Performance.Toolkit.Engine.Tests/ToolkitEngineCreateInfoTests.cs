// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.

using Microsoft.Performance.SDK.Processing;
using Microsoft.Performance.SDK.Processing.DataSourceGrouping;
using Microsoft.Performance.Testing;
using Microsoft.Performance.Testing.SDK;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using System;
using System.Collections.Generic;

namespace Microsoft.Performance.Toolkit.Engine.Tests
{
    [TestClass]
    public class ToolkitEngineCreateInfoTests
    {
        [TestMethod]
        [IntegrationTest]
        public void DefaultRuntime_Set()
        {
            var sut = new EngineCreateInfo(DataSourceSet.Create().AsReadOnly());

            string expectedName = typeof(EngineCreateInfo).Assembly.GetName().Name;
            Assert.AreEqual(expectedName, sut.RuntimeName);
        }

        [TestMethod]
        [IntegrationTest]
        public void DefaultProcessorOptionsResolver_Set()
        {
            var sut = new EngineCreateInfo(DataSourceSet.Create().AsReadOnly());
            Assert.IsNotNull(sut.OptionsResolver, "Options Resolver is null when a default is expected");

            var dsg = new DataSourceGroup(new[] { new FileDataSource("sample") }, new DefaultProcessingMode());

            var guidDataSourcePairs = new[]
            {
                new Tuple<Guid, IDataSourceGroup>(Guid.NewGuid(), dsg),
                new Tuple<Guid, IDataSourceGroup>(Guid.NewGuid(), Any.DataSourceGroup()),
                new Tuple<Guid, IDataSourceGroup>(Guid.NewGuid(), Any.DataSourceGroup()),
            };

            ProcessingOptionsResolverTests.AssertExpectedProcessorOptions(sut.OptionsResolver, guidDataSourcePairs, ProcessorOptions.Default);
        }

        [TestMethod]
        [IntegrationTest]
        public void GlobalProcessorOptionsResolver_Test()
        {
            var processorOptions = new ProcessorOptions(
                new[]
                {
                    new OptionInstance(
                        FakeProcessingSourceOptions.Ids.Two,
                        "arg1"),
                });

            var dsg = new DataSourceGroup(new[] { new FileDataSource("sample") }, new DefaultProcessingMode());

            var sut = new EngineCreateInfo(DataSourceSet.Create().AsReadOnly()).WithProcessorOptions(processorOptions);

            Assert.IsNotNull(sut.OptionsResolver, "Options Resolver is null when a default is expected");

            var guidDataSourcePairs = new[]
            {
                new Tuple<Guid, IDataSourceGroup>(Guid.NewGuid(), dsg),
                new Tuple<Guid, IDataSourceGroup>(Guid.NewGuid(), dsg),
                new Tuple<Guid, IDataSourceGroup>(Guid.NewGuid(), Any.DataSourceGroup()),
                new Tuple<Guid, IDataSourceGroup>(Guid.NewGuid(), Any.DataSourceGroup()),
            };

            ProcessingOptionsResolverTests.AssertExpectedProcessorOptions(sut.OptionsResolver, guidDataSourcePairs, processorOptions);
        }

        [TestMethod]
        [IntegrationTest]
        public void ProcessingSourceOptionsResolver_Test()
        {
            var processorOptions = new ProcessorOptions(
                new[]
                {
                    new OptionInstance(
                        FakeProcessingSourceOptions.Ids.Two,
                        "arg1"),
                });

            var processingGuid = Guid.NewGuid();
            var processorOptionsMap = new Dictionary<Guid, ProcessorOptions>()
            {
                { processingGuid, processorOptions }
            };

            var dsg = new DataSourceGroup(new[] { new FileDataSource("sample") }, new DefaultProcessingMode());

            var sut = new EngineCreateInfo(DataSourceSet.Create().AsReadOnly()).WithProcessorOptions(processorOptionsMap);

            Assert.IsNotNull(sut.OptionsResolver, "Options Resolver is null when a default is expected");

            var guidDataSourcePairsDefault = new[]
            {
                new Tuple<Guid, IDataSourceGroup>(Guid.NewGuid(), dsg),
                new Tuple<Guid, IDataSourceGroup>(Guid.NewGuid(), dsg),
                new Tuple<Guid, IDataSourceGroup>(Guid.NewGuid(), Any.DataSourceGroup()),
                new Tuple<Guid, IDataSourceGroup>(Guid.NewGuid(), Any.DataSourceGroup()),
            };
            
            var guidDataSourcePairs = new[]
            {
                new Tuple<Guid, IDataSourceGroup>(processingGuid, dsg),
                new Tuple<Guid, IDataSourceGroup>(processingGuid, dsg),
                new Tuple<Guid, IDataSourceGroup>(processingGuid, Any.DataSourceGroup()),
                new Tuple<Guid, IDataSourceGroup>(processingGuid, Any.DataSourceGroup()),
            };

            ProcessingOptionsResolverTests.AssertExpectedProcessorOptions(sut.OptionsResolver, guidDataSourcePairs, processorOptions);
            ProcessingOptionsResolverTests.AssertExpectedProcessorOptions(sut.OptionsResolver, guidDataSourcePairsDefault, ProcessorOptions.Default);
        }

        [TestMethod]
        [UnitTest]
        public void NullOptionsResolver_Failure()
        {
            var sut = new EngineCreateInfo(DataSourceSet.Create().AsReadOnly());
            IProcessingOptionsResolver resolver = null;
            Assert.ThrowsException<ArgumentNullException>(() => sut.WithProcessorOptions(resolver));
        }
    }
}
