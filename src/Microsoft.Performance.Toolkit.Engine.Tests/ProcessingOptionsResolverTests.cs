using Microsoft.Performance.SDK.Processing;
using Microsoft.Performance.SDK.Processing.DataSourceGrouping;
using Microsoft.Performance.Testing;
using Microsoft.Performance.Toolkit.Engine;
using Microsoft.Performance.Toolkit.Engine.Tests;
using Microsoft.Performance.Toolkit.Engine.Tests.TestCookers.Source123;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using System.Collections.Generic;

namespace Microsoft.Performance.SDK.Tests
{
    [TestClass]
    public class ProcessingOptionsResolverTests
        : EngineFixture
    {
        //[TestMethod]
        //[UnitTest]
        //public void ProcessingOptionsReferenceTest()
        //{
        //    ProcessingOptionsResolver sut = new ProcessingOptionsResolver();

        //    Assert.IsNotNull(sut);
        //    Assert.IsNotNull(sut.OptionsForDataSourceGroups);
        //    Assert.AreEqual(0, sut.OptionsForDataSourceGroups.Count, "Options for DSG is expected to be empty");
        //    Assert.AreEqual(ProcessorOptions.Default, sut.GetProcessorOptions(new List<IDataSource>(), new Source123DataSource()));
        //}

        //// This test needs a ProcessingSource
        //[TestMethod]
        //[UnitTest]
        //public void ProcessingOptionsResolverTests2()
        //{

        //    IEnumerable<IDataSource> dataSources1 = new List<IDataSource>()
        //    {
        //        new FileDataSource("test1.txt"),
        //        new FileDataSource("test2.txt")
        //    };

        //    IEnumerable<IDataSource> dataSources2 = new List<IDataSource>()
        //    {
        //        new FileDataSource("test2.txt"),
        //        new FileDataSource("test4.txt")
        //    };

        //    var expectedProcessorOptions1 = new ProcessorOptions(new List<OptionInstance> { }, new List<string> { "arg1", "arg2" });
        //    var expectedProcessorOptions2 = new ProcessorOptions(new List<OptionInstance> { }, new List<string> { "arg3", "arg4" });

        //    var dataSourceOptions = new Dictionary<IEnumerable<IDataSource>, ProcessorOptions>()
        //    {
        //        {  dataSources1, expectedProcessorOptions1 },
        //        {  dataSources2, expectedProcessorOptions2 }
        //    };

        //    ProcessingOptionsResolver sut = new ProcessingOptionsResolver(dataSourceOptions);

        //    Assert.IsNotNull(sut);
        //    Assert.IsNotNull(sut.OptionsForDataSourceGroups);

        //    Assert.AreEqual(dataSourceOptions.Count, sut.OptionsForDataSourceGroups.Count, "Map counts do not match");
        //    Assert.AreEqual(dataSourceOptions.Keys, sut.OptionsForDataSourceGroups.Keys, "Map Keys do not match");
        //    Assert.AreEqual(dataSourceOptions.Values, sut.OptionsForDataSourceGroups.Values, "Map Values do not match");

        //    Assert.AreEqual(expectedProcessorOptions1, sut.GetProcessorOptions(dataSources1, new Source123DataSource()));
        //    Assert.AreEqual(expectedProcessorOptions2, sut.GetProcessorOptions(dataSources2, new Source123DataSource()));
        //}

        private class MockDataSourceGroupeProcessingOptionsResolver : IProcessingOptionsResolver
        {

            public ProcessorOptions GetProcessorOptions(IDataSourceGroup dataSourceGroup, IProcessingSource processingSource)
            {
                return ProcessorOptions.Default;
            }
        }
    }

}
