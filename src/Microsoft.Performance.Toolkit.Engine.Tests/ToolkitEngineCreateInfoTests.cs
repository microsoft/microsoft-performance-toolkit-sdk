// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.

using Microsoft.Performance.SDK.Processing;
using Microsoft.Performance.SDK.Processing.DataSourceGrouping;
using Microsoft.Performance.SDK.Runtime;
using Microsoft.Performance.Testing;
using Microsoft.Performance.Toolkit.Engine.Tests.TestCookers.Source123;
using Microsoft.VisualStudio.TestTools.UnitTesting;

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
        }

        [TestMethod]
        [IntegrationTest]
        public void GlobalProcessorOptionsResolver_Set()
        {
            var processorOptions = new ProcessorOptions(
                new[]
                {
                    new OptionInstance(
                        new Option('s', "test1"),
                        "arg1"),
                });

            var dsg = new DataSourceGroup(new[] { new FileDataSource("") }, new DefaultProcessingMode());
            ProcessingSourceReference.TryCreateReference(typeof(Source123DataSource), out var ps);

            var sut = new EngineCreateInfo(DataSourceSet.Create().AsReadOnly());
            sut.WithProcessorOptions(processorOptions);

            Assert.IsNotNull(sut.OptionsResolver, "Options Resolver is null when a default is expected");
            Assert.AreEqual(processorOptions, sut.OptionsResolver.GetProcessorOptions(dsg, ps.Instance), "Expected processor options were not returned" );
        }
    }
}
