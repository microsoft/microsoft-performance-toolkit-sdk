// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.

using Microsoft.Performance.SDK.Processing;
using Microsoft.Performance.Testing;
using Microsoft.Performance.Testing.SDK;
using Microsoft.Performance.Toolkit.Engine.Tests.TestCookers.Source123;
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

            var dataSourceOptionsMap = new Dictionary<IEnumerable<IDataSource>, ProcessorOptions>() 
            {
                {null , null},
                {null , null}
            };

            var processingResolver = new ProcessingOptionsResolver();


            var sut = new EngineCreateInfo(dataSources.AsReadOnly(), processingResolver);


        }
    }
}
