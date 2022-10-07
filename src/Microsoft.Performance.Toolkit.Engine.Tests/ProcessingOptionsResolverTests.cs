// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.

using Microsoft.Performance.SDK;
using Microsoft.Performance.SDK.Processing;
using Microsoft.Performance.SDK.Processing.DataSourceGrouping;
using Microsoft.Performance.SDK.Runtime;
using Microsoft.Performance.Testing;
using Microsoft.Performance.Toolkit.Engine.Tests.TestCookers.Source123;
using Microsoft.Performance.Toolkit.Engine.Tests.TestCookers.Source4;
using Microsoft.Performance.Toolkit.Engine.Tests.TestCookers.Source5;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using System;
using System.Collections.Generic;
using System.Linq;

namespace Microsoft.Performance.Toolkit.Engine.Tests
{
    [TestClass]
    public class ProcessingOptionsResolverTests
    {
        [TestMethod]
        [UnitTest]
        public void DefaultGlobalProcessingOptionsResolverTest()
        {
            GlobalProcessingOptionsResolver sut = new GlobalProcessingOptionsResolver(ProcessorOptions.Default);

            Assert.IsNotNull(sut, "Options Resolver is null");

            DataSourceGroup dsg1 = new DataSourceGroup(
                new[] { new FileDataSource("test1" + Source123DataSource.Extension) },
                new DefaultProcessingMode());

            DataSourceGroup dsg2 = new DataSourceGroup(
                new[] { new FileDataSource("test2" + Source4DataSource.Extension) },
                new DefaultProcessingMode());

            // Ensure these are default
            Assert.AreEqual(ProcessorOptions.Default, sut.GetProcessorOptions(Guid.NewGuid(), dsg1), "ProcessorOptions differ from expected Default");
            Assert.AreEqual(ProcessorOptions.Default, sut.GetProcessorOptions(Guid.NewGuid(), dsg2), "ProcessorOptions differ from expected Default");

            // Default ProcessorOptions should always work no matter the PSR
            Assert.AreEqual(ProcessorOptions.Default, sut.GetProcessorOptions(Guid.NewGuid(), dsg1), "ProcessorOptions differ from expected Default");
            Assert.AreEqual(ProcessorOptions.Default, sut.GetProcessorOptions(Guid.NewGuid(), dsg2), "ProcessorOptions differ from expected Default");
        }

        [TestMethod]
        [UnitTest]
        public void GlobalProcessingOptionsResolverTest()
        {
            ProcessorOptions processorOptions = new ProcessorOptions(
                new[]
                {
                    new OptionInstance(
                        new Option('s', "test1"),
                        "arg1"),
                });

            GlobalProcessingOptionsResolver sut = new GlobalProcessingOptionsResolver(processorOptions);

            Assert.IsNotNull(sut, "Options Resolver is null");

            DataSourceGroup dsg123 = new DataSourceGroup(
                new[]
                {
                    new FileDataSource("test1" + Source123DataSource.Extension),
                    new FileDataSource("test2" + Source123DataSource.Extension),
                    new FileDataSource("test3" + Source123DataSource.Extension)
                }, new DefaultProcessingMode());

            DataSourceGroup dsg4 = new DataSourceGroup(
                new[]
                {
                    new FileDataSource("test4" + Source4DataSource.Extension),
                }, new DefaultProcessingMode());

            DataSourceGroup dsg5 = new DataSourceGroup(
                new[]
                {
                    new FileDataSource("test5" + Source5DataSource.Extension),
                }, new DefaultProcessingMode());
            
            Assert.AreEqual(processorOptions, sut.GetProcessorOptions(Guid.NewGuid(), dsg123), "ProcessorOptions differ from expected");
            Assert.AreEqual(processorOptions, sut.GetProcessorOptions(Guid.NewGuid(), dsg4), "ProcessorOptions differ from expected");
            Assert.AreEqual(processorOptions, sut.GetProcessorOptions(Guid.NewGuid(), dsg5), "ProcessorOptions differ from expected");
        }

        [TestMethod]
        [UnitTest]
        public void ProcessingOptionsResolverMapTest()
        {
            DataSourceGroup dsg123_1 = new DataSourceGroup(
                new[]
                {
                    new FileDataSource("test1" + Source123DataSource.Extension),
                    new FileDataSource("test2" + Source123DataSource.Extension),
                    new FileDataSource("test3" + Source123DataSource.Extension)
                }, new DefaultProcessingMode());

            DataSourceGroup dsg123_2 = new DataSourceGroup(
                new[]
                {
                    new FileDataSource("test4" + Source123DataSource.Extension)
                }, new DefaultProcessingMode());

            DataSourceGroup dsg4 = new DataSourceGroup(
                new[]
                {
                    new FileDataSource("test5" + Source4DataSource.Extension),
                }, new DefaultProcessingMode());

            DataSourceGroup dsg5 = new DataSourceGroup(
                new[]
                {
                    new FileDataSource("test6" + Source5DataSource.Extension),
                }, new DefaultProcessingMode());

            var guids = new[]
            {
                Guid.NewGuid(),
                Guid.NewGuid(),
                Guid.NewGuid(),
            };

            ProcessorOptions[] processorOptions = new[]
            {
                new ProcessorOptions(
                    new[]
                    {
                        new OptionInstance(
                            new Option('s', "test1"),
                            "arg1"),
                    }),
                new ProcessorOptions(
                    new[]
                    {
                        new OptionInstance(
                            new Option('t', "test2"),
                            "arg2"),
                    }),
            };

            IDictionary<Guid, ProcessorOptions> map = new Dictionary<Guid, ProcessorOptions>()
            {
                { guids[0],  processorOptions[0]},
                { guids[1],    processorOptions[1]},
                { guids[2],    processorOptions[0]},
            };

            ProcessingSourceOptionsResolver sut = new ProcessingSourceOptionsResolver(map);

            Assert.IsNotNull(sut, "Options Resolver is null");

            Assert.AreEqual(processorOptions[0], sut.GetProcessorOptions(guids[0], dsg123_1), "ProcessorOptions differ from expected");
            Assert.AreEqual(processorOptions[0], sut.GetProcessorOptions(guids[0], dsg123_2), "ProcessorOptions differ from expected");
            Assert.AreEqual(processorOptions[1], sut.GetProcessorOptions(guids[1], dsg4), "ProcessorOptions differ from expected");
            Assert.AreEqual(processorOptions[0], sut.GetProcessorOptions(guids[2], dsg5), "ProcessorOptions differ from expected");
        }
    }

}
