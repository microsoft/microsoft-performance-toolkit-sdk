// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.

using Microsoft.Performance.SDK.Processing;
using Microsoft.Performance.SDK.Runtime;
using Microsoft.Performance.Testing;
using Microsoft.Performance.Testing.SDK;
using Microsoft.Performance.Toolkit.Engine.Tests.TestCookers.Source123;
using Microsoft.VisualBasic;
using Microsoft.VisualStudio.TestPlatform.ObjectModel.Utilities;
using Microsoft.VisualStudio.TestTools.UnitTesting;
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
        public void SomethingTest()
        {
            var file1 = new FileDataSource("test1" + Source123DataSource.Extension);
            var file2 = new FileDataSource("test2" + Source123DataSource.Extension);

            using var dataSources = DataSourceSet.Create();
            dataSources.AddDataSource(file1);
            dataSources.AddDataSource(file2);

            var processingResolver = new ProcessingOptionsResolver();
            
            var sut = new EngineCreateInfo(dataSources.AsReadOnly(), processingResolver);
            Assert.IsNotNull(sut.OptionsResolver, "Options Resolver is null when a default is expected");

            ProcessingSourceReference.TryCreateReference(typeof(Source123DataSource), out var psr);
            Assert.AreEqual(ProcessorOptions.Default, sut.OptionsResolver.GetProcessorOptions(dataSources.FreeDataSourcesToProcess, psr.Instance));

        }
        
        [TestMethod]
        [IntegrationTest]
        public void SomethingTest2()
        {
            var file1 = new FileDataSource("test1" + Source123DataSource.Extension);
            var file2 = new FileDataSource("test2" + Source123DataSource.Extension);

            using var dataSources = DataSourceSet.Create();
            dataSources.AddDataSource(file1);
            dataSources.AddDataSource(file2);

            var opt1 = new ProcessorOptions(
                new[]
                {
                    new OptionInstance(
                        new Option('r', "test"),
                        "arg1"),
                });
            
            var opt2 = new ProcessorOptions(
                new[]
                {
                    new OptionInstance(
                        new Option('r', "test"),
                        "arg2"),
                });

            var dsg1 = new List<IDataSource> { file1 };
            var dsg2 = new List<IDataSource> { file2 };

            var dataSourceOptionsMap = new Dictionary<IEnumerable<IDataSource>, ProcessorOptions>() 
            {
                { dsg1, opt1 },
                { dsg2, opt2 }
            };

            var processingResolver = new ProcessingOptionsResolver();

            var sut = new EngineCreateInfo(dataSources.AsReadOnly(), processingResolver);
            Assert.IsNotNull(sut.OptionsResolver, "Options Resolver is null when a default is expected");

            ProcessingSourceReference.TryCreateReference(typeof(Source123DataSource), out var psr);
            
            Assert.AreEqual(opt1, sut.OptionsResolver.GetProcessorOptions(dsg1, psr.Instance));
            Assert.AreEqual(opt2, sut.OptionsResolver.GetProcessorOptions(dsg2, psr.Instance));

            // This group doesn't exist in the map, so default
            Assert.AreEqual(ProcessorOptions.Default, sut.OptionsResolver.GetProcessorOptions(dataSources.FreeDataSourcesToProcess, psr.Instance));
        }
    }
}
