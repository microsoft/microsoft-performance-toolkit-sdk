// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.

using Microsoft.Performance.SDK.Processing;
using Microsoft.Performance.SDK.Processing.DataSourceGrouping;
using Microsoft.Performance.SDK.Runtime;
using Microsoft.Performance.Testing;
using Microsoft.Performance.Toolkit.Engine.Tests.TestCookers.Source123;
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
            Assert.AreEqual(typeof(GlobalProcessingOptionsResolver), sut.OptionsResolver.GetType(), "Options Resolver is not of type GlobalProcessingOptionsResolver");
        }

        [TestMethod]
        [IntegrationTest]
        public void GlobalProcessorOptionsResolver_Test()
        {
            var processorOptions = new ProcessorOptions(
                new[]
                {
                    new OptionInstance(
                        new Option('s', "test1"),
                        "arg1"),
                });

            var dsg = new DataSourceGroup(new[] { new FileDataSource("sample") }, new DefaultProcessingMode());
            ProcessingSourceReference.TryCreateReference(typeof(Source123DataSource), out var ps);

            var sut = new EngineCreateInfo(DataSourceSet.Create().AsReadOnly());
            sut.WithProcessorOptions(processorOptions);

            Assert.IsNotNull(sut.OptionsResolver, "Options Resolver is null when a default is expected");
            Assert.AreEqual(typeof(GlobalProcessingOptionsResolver), sut.OptionsResolver.GetType(), "Options Resolver is not of type GlobalProcessingOptionsResolver");
            
            Assert.AreEqual(processorOptions, sut.OptionsResolver.GetProcessorOptions(ps.Guid, dsg), "Expected processor options were not returned" );
        }

        [TestMethod]
        [IntegrationTest]
        public void ProcessingSourceOptionsResolver_Test()
        {
            var processorOptions = new ProcessorOptions(
                new[]
                {
                    new OptionInstance(
                        new Option('s', "test1"),
                        "arg1"),
                });

            ProcessingSourceReference.TryCreateReference(typeof(Source123DataSource), out var psr);
            var processorOptionsMap = new Dictionary<Guid, ProcessorOptions>()
            {
                { psr.Guid, processorOptions }
            };

            var dsg = new DataSourceGroup(new[] { new FileDataSource("sample") }, new DefaultProcessingMode());

            var sut = new EngineCreateInfo(DataSourceSet.Create().AsReadOnly());
            sut.WithProcessorOptions(processorOptionsMap);

            Assert.IsNotNull(sut.OptionsResolver, "Options Resolver is null when a default is expected");
            Assert.AreEqual(typeof(ProcessingSourceOptionsResolver), sut.OptionsResolver.GetType(), "Options Resolver is not of type ProcessingSourceOptionsResolver");
            Assert.AreEqual(processorOptions, sut.OptionsResolver.GetProcessorOptions(psr.Guid, dsg), "Expected processor options were not returned");
        }

        [TestMethod]
        [IntegrationTest]
        public void NullOptionsResolver_Failure()
        {
            var sut = new EngineCreateInfo(DataSourceSet.Create().AsReadOnly());
            IProcessingOptionsResolver resolver = null;
            Assert.ThrowsException<ArgumentNullException>(() => sut.WithProcessorOptions(resolver));
        }
    }
}
