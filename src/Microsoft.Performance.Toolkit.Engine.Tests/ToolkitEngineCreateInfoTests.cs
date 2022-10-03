// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.

using Microsoft.Performance.SDK.Processing;
using Microsoft.Performance.SDK.Processing.DataSourceGrouping;
using Microsoft.Performance.SDK.Runtime;
using Microsoft.Performance.Testing;
using Microsoft.Performance.Testing.SDK;
using Microsoft.Performance.Toolkit.Engine.Tests.TestCookers.Source123;
using Microsoft.Performance.Toolkit.Engine.Tests.TestCookers.Source4;
using Microsoft.Performance.Toolkit.Engine.Tests.TestCookers.Source5;
using Microsoft.VisualBasic;
using Microsoft.VisualStudio.TestPlatform.ObjectModel.Utilities;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using System;
using System.Collections.Generic;
using System.Linq;

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
            Assert.IsNotNull(sut.OptionsResolver, "Options Resolver is null when a default is expected");
        }

        [TestMethod]
        [IntegrationTest]
        public void DefaultGlobalProcessingOptionsResolverTest()
        {
            var sut = new EngineCreateInfo(DataSourceSet.Create().AsReadOnly());

            string expectedName = typeof(EngineCreateInfo).Assembly.GetName().Name;
            Assert.AreEqual(expectedName, sut.RuntimeName);
            Assert.IsNotNull(sut.OptionsResolver, "Options Resolver is null");

            var dsg1 = new DataSourceGroup(
                new List<IDataSource>() { new FileDataSource("test1" + Source123DataSource.Extension) },
                new DefaultProcessingMode());

            var dsg2 = new DataSourceGroup(
                new List<IDataSource>() { new FileDataSource("test2" + Source4DataSource.Extension) },
                new DefaultProcessingMode());

            ProcessingSourceReference.TryCreateReference(typeof(Source123DataSource), out var psr123);
            ProcessingSourceReference.TryCreateReference(typeof(Source4DataSource), out var psr4);

            // Ensure these are default
            Assert.AreEqual(ProcessorOptions.Default, sut.OptionsResolver.GetProcessorOptions(dsg1, psr123.Instance), "ProcessorOptions differ from expected Default");
            Assert.AreEqual(ProcessorOptions.Default, sut.OptionsResolver.GetProcessorOptions(dsg2, psr4.Instance), "ProcessorOptions differ from expected Default");

            // Default ProcessorOptions should always work no matter the PSR
            Assert.AreEqual(ProcessorOptions.Default, sut.OptionsResolver.GetProcessorOptions(dsg1, psr4.Instance), "ProcessorOptions differ from expected Default");
            Assert.AreEqual(ProcessorOptions.Default, sut.OptionsResolver.GetProcessorOptions(dsg2, psr123.Instance), "ProcessorOptions differ from expected Default");
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

            var sut = new EngineCreateInfo(DataSourceSet.Create().AsReadOnly(), processorOptions);

            string expectedName = typeof(EngineCreateInfo).Assembly.GetName().Name;
            Assert.AreEqual(expectedName, sut.RuntimeName);

            Assert.IsNotNull(sut.OptionsResolver, "Options Resolver is null");

            var dsg123 = new DataSourceGroup(new List<IDataSource>() 
                {
                    new FileDataSource("test1" + Source123DataSource.Extension),
                    new FileDataSource("test2" + Source123DataSource.Extension),
                    new FileDataSource("test3" + Source123DataSource.Extension)
                }, new DefaultProcessingMode());

            var dsg4 = new DataSourceGroup(new List<IDataSource>() 
                {
                    new FileDataSource("test4" + Source4DataSource.Extension),
                }, new DefaultProcessingMode());

            var dsg5 = new DataSourceGroup(new List<IDataSource>() 
                {
                    new FileDataSource("test5" + Source5DataSource.Extension),
                }, new DefaultProcessingMode());

            ProcessingSourceReference.TryCreateReference(typeof(Source123DataSource), out var psr123);
            ProcessingSourceReference.TryCreateReference(typeof(Source4DataSource), out var psr4);
            ProcessingSourceReference.TryCreateReference(typeof(Source5DataSource), out var psr5);

            Assert.AreEqual(processorOptions, sut.OptionsResolver.GetProcessorOptions(dsg123, psr123.Instance), "ProcessorOptions differ from expected");
            Assert.AreEqual(processorOptions, sut.OptionsResolver.GetProcessorOptions(dsg4, psr4.Instance), "ProcessorOptions differ from expected");
            Assert.AreEqual(processorOptions, sut.OptionsResolver.GetProcessorOptions(dsg5, psr5.Instance), "ProcessorOptions differ from expected");
        }

        [TestMethod]
        [IntegrationTest]
        public void GlobalProcessingOptionsResolverTestFailure()
        {

            var processorOptions = new ProcessorOptions(
                new[]
                {
                    new OptionInstance(
                        new Option('t', "test2"),
                        "arg2"),
                });

            var dsg123 = new DataSourceGroup(new List<IDataSource>()
                {
                    new FileDataSource("test1" + Source123DataSource.Extension),
                    new FileDataSource("test2" + Source123DataSource.Extension),
                    new FileDataSource("test3" + Source123DataSource.Extension)
                }, new DefaultProcessingMode());

            var dsg4 = new DataSourceGroup(new List<IDataSource>()
                {
                    new FileDataSource("test4" + Source4DataSource.Extension),
                }, new DefaultProcessingMode());

            var sut = new EngineCreateInfo(DataSourceSet.Create().AsReadOnly(), processorOptions);
            Assert.IsNotNull(sut.OptionsResolver, "Options Resolver is null");

            ProcessingSourceReference.TryCreateReference(typeof(Source123DataSource), out var psr123);
            ProcessingSourceReference.TryCreateReference(typeof(Source4DataSource), out var psr4);

            // success
            Assert.AreEqual(processorOptions, sut.OptionsResolver.GetProcessorOptions(dsg4, psr4.Instance), "ProcessorOptions differ from expected");

            // Failure Expected since 
            try
            {
                var actualOptions = sut.OptionsResolver.GetProcessorOptions(dsg123, psr123.Instance);
            } 
            catch (NotSupportedException e)
            {
                Assert.IsNotNull(e);
            }
            catch (Exception e)
            {
                Assert.Fail(e.Message);
            }

        }

        [TestMethod]
        [IntegrationTest]
        public void ProcessingOptionsResolverMapTest()
        {


            var dsg123_1 = new DataSourceGroup(new List<IDataSource>()
                {
                    new FileDataSource("test1" + Source123DataSource.Extension),
                    new FileDataSource("test2" + Source123DataSource.Extension),
                    new FileDataSource("test3" + Source123DataSource.Extension)
                }, new DefaultProcessingMode());
            
            var dsg123_2 = new DataSourceGroup(new List<IDataSource>()
                {
                    new FileDataSource("test4" + Source123DataSource.Extension)
                }, new DefaultProcessingMode());

            var dsg4 = new DataSourceGroup(new List<IDataSource>()
                {
                    new FileDataSource("test5" + Source4DataSource.Extension),
                }, new DefaultProcessingMode());

            var dsg5 = new DataSourceGroup(new List<IDataSource>()
                {
                    new FileDataSource("test6" + Source5DataSource.Extension),
                }, new DefaultProcessingMode());


            ProcessingSourceReference.TryCreateReference(typeof(Source123DataSource), out var psr123);
            ProcessingSourceReference.TryCreateReference(typeof(Source4DataSource), out var psr4);
            ProcessingSourceReference.TryCreateReference(typeof(Source5DataSource), out var psr5);

            var processorOptions = new List<ProcessorOptions>()
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
            

            var sut = new EngineCreateInfo(DataSourceSet.Create().AsReadOnly(), map);

            string expectedName = typeof(EngineCreateInfo).Assembly.GetName().Name;
            Assert.AreEqual(expectedName, sut.RuntimeName);

            Assert.IsNotNull(sut.OptionsResolver, "Options Resolver is null");

            Assert.AreEqual(processorOptions[0], sut.OptionsResolver.GetProcessorOptions(dsg123_1, psr123.Instance), "ProcessorOptions differ from expected");
            Assert.AreEqual(processorOptions[0], sut.OptionsResolver.GetProcessorOptions(dsg123_2, psr123.Instance), "ProcessorOptions differ from expected");
            Assert.AreEqual(processorOptions[1], sut.OptionsResolver.GetProcessorOptions(dsg4, psr4.Instance), "ProcessorOptions differ from expected");
            Assert.AreEqual(processorOptions[0], sut.OptionsResolver.GetProcessorOptions(dsg5, psr5.Instance), "ProcessorOptions differ from expected");
        }

        // todo : test the custom mock processing options resolver
        [TestMethod]
        [IntegrationTest]
        public void CustomProcessingOptionsResolver()
        {

        }
    }
}
