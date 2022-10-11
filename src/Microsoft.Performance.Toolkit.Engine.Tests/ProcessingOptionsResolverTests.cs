// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.

using Microsoft.Performance.SDK.Processing;
using Microsoft.Performance.SDK.Processing.DataSourceGrouping;
using Microsoft.Performance.Testing;
using Microsoft.Performance.Toolkit.Engine.Tests.TestCookers.Source123;
using Microsoft.Performance.Toolkit.Engine.Tests.TestCookers.Source4;
using Microsoft.Performance.Toolkit.Engine.Tests.TestCookers.Source5;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using System;
using System.Collections.Generic;

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


            var guidDataSourcePairs = new[]
            {
                new Tuple<Guid, IDataSourceGroup>(Guid.NewGuid(), dsg1),
                new Tuple<Guid, IDataSourceGroup>(Guid.NewGuid(), dsg1),
                new Tuple<Guid, IDataSourceGroup>(Guid.NewGuid(), dsg2),
                new Tuple<Guid, IDataSourceGroup>(Guid.NewGuid(), dsg2),
            };

            AssertExpectedProcessorOptions(sut, guidDataSourcePairs, ProcessorOptions.Default);
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
            
            var guidDataSourcePairs = new[]
            {
                new Tuple<Guid, IDataSourceGroup>(Guid.NewGuid(), dsg123),
                new Tuple<Guid, IDataSourceGroup>(Guid.NewGuid(), dsg4),
                new Tuple<Guid, IDataSourceGroup>(Guid.NewGuid(), dsg5),
            };

            AssertExpectedProcessorOptions(sut, guidDataSourcePairs, processorOptions);
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

            IDictionary<Guid, ProcessorOptions> optionsMap = new Dictionary<Guid, ProcessorOptions>()
            {
                { guids[0],  processorOptions[0]},
                { guids[1],    processorOptions[1]},
                { guids[2],    processorOptions[0]},
            };

            ProcessingSourceOptionsResolver sut = new ProcessingSourceOptionsResolver(optionsMap);

            var guidDataSourcePairs1 = new[]
            {
                new Tuple<Guid, IDataSourceGroup>(guids[0], dsg123_1),
                new Tuple<Guid, IDataSourceGroup>(guids[0], dsg123_2),
                new Tuple<Guid, IDataSourceGroup>(guids[0], dsg4),
                new Tuple<Guid, IDataSourceGroup>(guids[0], dsg5),
                new Tuple<Guid, IDataSourceGroup>(guids[2], dsg123_1),
                new Tuple<Guid, IDataSourceGroup>(guids[2], dsg123_2),
                new Tuple<Guid, IDataSourceGroup>(guids[2], dsg4),
                new Tuple<Guid, IDataSourceGroup>(guids[2], dsg5),
            };

            var guidDataSourcePairs2 = new[]
            {
                new Tuple<Guid, IDataSourceGroup>(guids[1], dsg123_1),
                new Tuple<Guid, IDataSourceGroup>(guids[1], dsg123_2),
                new Tuple<Guid, IDataSourceGroup>(guids[1], dsg4),
                new Tuple<Guid, IDataSourceGroup>(guids[1], dsg5),
            };

            AssertExpectedProcessorOptions(sut, guidDataSourcePairs1, processorOptions[0]);
            AssertExpectedProcessorOptions(sut, guidDataSourcePairs2, processorOptions[1]);
        }

        public static void AssertExpectedProcessorOptions(
            IProcessingOptionsResolver sut,
            IEnumerable<Tuple<Guid, IDataSourceGroup>> guidDataSourceGroups,
            ProcessorOptions expectedOptions)
        {
            Assert.IsNotNull(sut, "Options Resolver is null");
            foreach (var g in guidDataSourceGroups)
            {
                Assert.AreEqual(expectedOptions, sut.GetProcessorOptions(g.Item1, g.Item2), "ProcessorOptions differ from expected");
            }
        }
    }

}
