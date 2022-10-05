// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.

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

namespace Microsoft.Performance.SDK.Tests
{
    [TestClass]
    public class ProcessingOptionsResolverTests
    {
        [TestMethod]
        [IntegrationTest]
        public void DefaultGlobalProcessingOptionsResolverTest()
        {
            var sut = GlobalProcessingOptionsResolver.Default;

            Assert.IsNotNull(sut, "Options Resolver is null");

            var dsg1 = new DataSourceGroup(
                new[] { new FileDataSource("test1" + Source123DataSource.Extension) },
                new DefaultProcessingMode());

            var dsg2 = new DataSourceGroup(
                new[] { new FileDataSource("test2" + Source4DataSource.Extension) },
                new DefaultProcessingMode());

            ProcessingSourceReference.TryCreateReference(typeof(Source123DataSource), out var psr123);
            ProcessingSourceReference.TryCreateReference(typeof(Source4DataSource), out var psr4);

            // Ensure these are default
            Assert.AreEqual(ProcessorOptions.Default, sut.GetProcessorOptions(dsg1, psr123.Instance), "ProcessorOptions differ from expected Default");
            Assert.AreEqual(ProcessorOptions.Default, sut.GetProcessorOptions(dsg2, psr4.Instance), "ProcessorOptions differ from expected Default");

            // Default ProcessorOptions should always work no matter the PSR
            Assert.AreEqual(ProcessorOptions.Default, sut.GetProcessorOptions(dsg1, psr4.Instance), "ProcessorOptions differ from expected Default");
            Assert.AreEqual(ProcessorOptions.Default, sut.GetProcessorOptions(dsg2, psr123.Instance), "ProcessorOptions differ from expected Default");
        }

        [TestMethod]
        [IntegrationTest]
        public void GlobalProcessingOptionsResolverTest()
        {
            var processorOptions = new ProcessorOptions(
                new[]
                {
                    new OptionInstance(
                        new Option('s', "test1"),
                        "arg1"),
                });

            var sut = new GlobalProcessingOptionsResolver(processorOptions);

            Assert.IsNotNull(sut, "Options Resolver is null");

            var dsg123 = new DataSourceGroup(
                new[]
                {
                    new FileDataSource("test1" + Source123DataSource.Extension),
                    new FileDataSource("test2" + Source123DataSource.Extension),
                    new FileDataSource("test3" + Source123DataSource.Extension)
                }, new DefaultProcessingMode());

            var dsg4 = new DataSourceGroup(
                new[]
                {
                    new FileDataSource("test4" + Source4DataSource.Extension),
                }, new DefaultProcessingMode());

            var dsg5 = new DataSourceGroup(
                new[]
                {
                    new FileDataSource("test5" + Source5DataSource.Extension),
                }, new DefaultProcessingMode());

            ProcessingSourceReference.TryCreateReference(typeof(Source123DataSource), out var psr123);
            ProcessingSourceReference.TryCreateReference(typeof(Source4DataSource), out var psr4);
            ProcessingSourceReference.TryCreateReference(typeof(Source5DataSource), out var psr5);

            Assert.AreEqual(processorOptions, sut.GetProcessorOptions(dsg123, psr123.Instance), "ProcessorOptions differ from expected");
            Assert.AreEqual(processorOptions, sut.GetProcessorOptions(dsg4, psr4.Instance), "ProcessorOptions differ from expected");
            Assert.AreEqual(processorOptions, sut.GetProcessorOptions(dsg5, psr5.Instance), "ProcessorOptions differ from expected");
        }

        [TestMethod]
        [IntegrationTest]
        public void ProcessingOptionsResolverMapTest()
        {
            var dsg123_1 = new DataSourceGroup(
                new[]
                {
                    new FileDataSource("test1" + Source123DataSource.Extension),
                    new FileDataSource("test2" + Source123DataSource.Extension),
                    new FileDataSource("test3" + Source123DataSource.Extension)
                }, new DefaultProcessingMode());

            var dsg123_2 = new DataSourceGroup(
                new[]
                {
                    new FileDataSource("test4" + Source123DataSource.Extension)
                }, new DefaultProcessingMode());

            var dsg4 = new DataSourceGroup(
                new[]
                {
                    new FileDataSource("test5" + Source4DataSource.Extension),
                }, new DefaultProcessingMode());

            var dsg5 = new DataSourceGroup(
                new[]
                {
                    new FileDataSource("test6" + Source5DataSource.Extension),
                }, new DefaultProcessingMode());

            ProcessingSourceReference.TryCreateReference(typeof(Source123DataSource), out var psr123);
            ProcessingSourceReference.TryCreateReference(typeof(Source4DataSource), out var psr4);
            ProcessingSourceReference.TryCreateReference(typeof(Source5DataSource), out var psr5);

            var processorOptions = new[]
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

            IDictionary<IProcessingSource, ProcessorOptions> map = new Dictionary<IProcessingSource, ProcessorOptions>()
            {
                { psr123.Instance,  processorOptions[0]},
                { psr4.Instance,    processorOptions[1]},
                { psr5.Instance,    processorOptions[0]},
            };

            var sut = new ProcessingSourceOptionsResolver(map);

            Assert.IsNotNull(sut, "Options Resolver is null");

            Assert.AreEqual(processorOptions[0], sut.GetProcessorOptions(dsg123_1, psr123.Instance), "ProcessorOptions differ from expected");
            Assert.AreEqual(processorOptions[0], sut.GetProcessorOptions(dsg123_2, psr123.Instance), "ProcessorOptions differ from expected");
            Assert.AreEqual(processorOptions[1], sut.GetProcessorOptions(dsg4, psr4.Instance), "ProcessorOptions differ from expected");
            Assert.AreEqual(processorOptions[0], sut.GetProcessorOptions(dsg5, psr5.Instance), "ProcessorOptions differ from expected");
        }

        [TestMethod]
        [IntegrationTest]
        public void CustomProcessingOptionsResolver()
        {
            var singleDataSource = new FileDataSource("test" + Source123DataSource.Extension);
            var dsCollection = new[]
            {
                new FileDataSource("test1" + Source123DataSource.Extension),
                new FileDataSource("test2" + Source123DataSource.Extension),
                new FileDataSource("test3" + Source123DataSource.Extension)
            };

            var sut = new MockDataSourceGrouperProcessingOptionsResolver( dsCollection, singleDataSource);

            var expectedProcessorOptions = new[]
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
                                new Option('r', "test"),
                                "arg"),
                        })
            };

            ProcessingSourceReference.TryCreateReference(typeof(Source123DataSource), out var psr123);


            Assert.IsNotNull(sut, "Options resolver is null");

            Assert.AreEqual(expectedProcessorOptions.ElementAt(0), sut.GetProcessorOptions(new DataSourceGroup(dsCollection, new DefaultProcessingMode()), psr123.Instance), "ProcessorOptions differ from expected");
            Assert.AreEqual(expectedProcessorOptions.ElementAt(1), sut.GetProcessorOptions(new DataSourceGroup(new[] { singleDataSource }, new DefaultProcessingMode()), psr123.Instance), "ProcessorOptions differ from expected");
            Assert.AreEqual(ProcessorOptions.Default, sut.GetProcessorOptions(new DataSourceGroup(new[] { dsCollection.First() }, new DefaultProcessingMode()), psr123.Instance), "ProcessorOptions differ from expected");

        }

        private class MockDataSourceGrouperProcessingOptionsResolver : IProcessingOptionsResolver
        {

            private readonly IEnumerable<IDataSource> dataSourceCollection;
            private readonly IDataSource singleDataSource;

            public MockDataSourceGrouperProcessingOptionsResolver(IEnumerable<IDataSource> dataSourceCollection, IDataSource dataSource)
            {
                Guard.NotNull(dataSourceCollection, nameof(dataSourceCollection));
                Guard.NotNull(dataSource, nameof(dataSource));

                this.dataSourceCollection = dataSourceCollection;
                this.singleDataSource = dataSource;
            }

            public ProcessorOptions GetProcessorOptions(IDataSourceGroup dsg, IProcessingSource processingSource)
            {
                if (dsg.DataSources == null)
                {
                    return ProcessorOptions.Default;
                }
                else if (this.dataSourceCollection.All(ds1 => dsg.DataSources.Any(ds2 => ds2.Equals(ds1)))) {
                    return new ProcessorOptions(
                        new[]
                        {
                            new OptionInstance(
                                new Option('s', "test1"),
                                "arg1"),
                        });
                }
                else if (dsg.DataSources.Any(ds2 => ds2.Equals(this.singleDataSource))) {
                    return new ProcessorOptions(
                        new[]
                        {
                            new OptionInstance(
                                new Option('r', "test"),
                                "arg"),
                        });
                }

                return ProcessorOptions.Default;
            }
        }
    }

}
