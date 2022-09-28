using Microsoft.Performance.SDK.Processing;
using Microsoft.Performance.Testing;
using Microsoft.Performance.Toolkit.Engine;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using System.Collections.Generic;

namespace Microsoft.Performance.SDK.Tests
{
    [TestClass]
    public class ProcessingOptionsResolverTests
    {
        [TestMethod]
        [UnitTest]
        public void ProcessingOptionsReferenceTest()
        {
            ProcessingOptionsResolver sut = new ProcessingOptionsResolver();

            Assert.IsNotNull(sut);
            Assert.IsNotNull(sut.OptionsForDataSources);
            Assert.AreEqual(ProcessorOptions.Default, sut.GetProcessorOptions(null, null));
        }

        [TestMethod]
        [UnitTest]
        public void ProcessingOptionsResolverTests2()
        {

            //IDataSource dataSource = new FileDataSource();
            //IDataSource dataSource2 = new FileDataSource();


            var dataSourceOptions = new Dictionary<IEnumerable<IDataSource>, ProcessorOptions>()
            {

            };

            ProcessingOptionsResolver sut = new ProcessingOptionsResolver(dataSourceOptions);

            Assert.IsNotNull(sut);
            Assert.IsNotNull(sut.OptionsForDataSources);


            foreach (IEnumerable<IDataSource> dataSourceGroup in dataSourceOptions.Keys)
            {


                // Assert.AreEqual(dataSourceGroup, sut.OptionsForDataSources[dataSourceGroup]);
            }


        }

    }
}
