using Microsoft.Performance.SDK.Processing;
using Microsoft.Performance.Testing;
using Microsoft.Performance.Toolkit.Engine;
using Microsoft.Performance.Toolkit.Engine.Tests;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using System.Collections.Generic;

namespace Microsoft.Performance.SDK.Tests
{
    // todo : do we need this?
    [TestClass]
    public class ProcessingOptionsResolverTests 
        : EngineFixture
    {
        [TestMethod]
        [UnitTest]
        public void ProcessingOptionsReferenceTest()
        {
            ProcessingOptionsResolver sut = new ProcessingOptionsResolver();

            Assert.IsNotNull(sut);
            Assert.IsNotNull(sut.OptionsForDataSourceGroups);
            Assert.AreEqual(ProcessorOptions.Default, sut.GetProcessorOptions(null, null));
        }

        [TestMethod]
        [UnitTest]
        public void ProcessingOptionsResolverTests2()
        {

            IEnumerable<IDataSource> dataSources = new List<IDataSource>()
            {
                new FileDataSource("test.txt"),
                new FileDataSource("test2.txt")
            };

            var expectedProcessorOptions = new ProcessorOptions( new List<OptionInstance> {  } , new List<string>{ "arg1", "arg2" });

            var dataSourceOptions = new Dictionary<IEnumerable<IDataSource>, ProcessorOptions>()
            {
                {  dataSources, ProcessorOptions.Default }
            };

            ProcessingOptionsResolver sut = new ProcessingOptionsResolver(dataSourceOptions);

            Assert.IsNotNull(sut);
            Assert.IsNotNull(sut.OptionsForDataSourceGroups);


            foreach (IEnumerable<IDataSource> dataSourceGroup in dataSourceOptions.Keys)
            {

                // Assert.AreEqual(dataSourceGroup, sut.OptionsForDataSources[dataSourceGroup]);
            }


        }

    }
}
