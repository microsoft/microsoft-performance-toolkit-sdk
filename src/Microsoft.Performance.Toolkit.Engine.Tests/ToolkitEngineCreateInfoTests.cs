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

            Assert.AreEqual(ProcessorOptions.Default, sut.OptionsResolver.GetProcessorOptions(Guid.NewGuid(), Any.DataSourceGroup()), "Options did not match default options.");
            Assert.AreEqual(ProcessorOptions.Default, sut.OptionsResolver.GetProcessorOptions(Guid.NewGuid(), Any.DataSourceGroup()), "Options did not match default options.");
            Assert.AreEqual(ProcessorOptions.Default, sut.OptionsResolver.GetProcessorOptions(Guid.NewGuid(), dsg), "Options did not match default options.");
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

            var sut = new EngineCreateInfo(DataSourceSet.Create().AsReadOnly());
            sut.WithProcessorOptions(processorOptions);

            Assert.IsNotNull(sut.OptionsResolver, "Options Resolver is null when a default is expected");
            
            Assert.AreEqual(processorOptions, sut.OptionsResolver.GetProcessorOptions(Guid.NewGuid(), dsg), "Expected processor options were not returned" );
            Assert.AreEqual(processorOptions, sut.OptionsResolver.GetProcessorOptions(Guid.NewGuid(), dsg), "Expected processor options were not returned" );
            Assert.AreEqual(processorOptions, sut.OptionsResolver.GetProcessorOptions(Guid.NewGuid(), Any.DataSourceGroup()), "Expected processor options were not returned" );
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

            var processingGuid = Guid.NewGuid();
            var processorOptionsMap = new Dictionary<Guid, ProcessorOptions>()
            {
                { processingGuid, processorOptions }
            };

            var dsg = new DataSourceGroup(new[] { new FileDataSource("sample") }, new DefaultProcessingMode());

            var sut = new EngineCreateInfo(DataSourceSet.Create().AsReadOnly());
            sut.WithProcessorOptions(processorOptionsMap);

            Assert.IsNotNull(sut.OptionsResolver, "Options Resolver is null when a default is expected");
            Assert.AreEqual(processorOptions, sut.OptionsResolver.GetProcessorOptions(processingGuid, dsg), "Expected processor options were not returned");
            Assert.AreEqual(ProcessorOptions.Default, sut.OptionsResolver.GetProcessorOptions(Guid.NewGuid(), dsg), "Expected default processor options for unspecified dsg");
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
