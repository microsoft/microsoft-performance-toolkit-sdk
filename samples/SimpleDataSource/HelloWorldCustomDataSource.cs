using Microsoft.Performance.SDK.Processing;
using System.Collections.Generic;
using System.Linq;

namespace SampleCustomDataSource
{
    //
    // This is a sample Custom Data Source (CDS) that understands files with the .txt extension
    //

    // In order for a CDS to be recognized, it MUST satisfy the following:
    //  a) Be a public type
    //  b) Have a public parameterless constructor
    //  c) Implement the ICustomDataSource interface
    //  d) Be decorated with the CustomDataSourceAttribute attribute
    //  e) Be decorated with at least one of the derivatives of the DataSourceAttribute attribute
    //

    [CustomDataSource(
        "{F73EACD4-1AE9-4844-80B9-EB77396781D1}",  // The GUID must be unique for your Custom Data Source. You can use Visual Studio's Tools -> Create Guid… tool to create a new GUID
        "Hello World",                             // The Custom Data Source MUST have a name
        "A data source to say hello!")]            // The Custom Data Source MUST have a description
    [FileDataSource(
        ".txt",                                    // A file extension is REQUIRED
        "Text files")]                             // A description is OPTIONAL. The description is what appears in the file open menu to help users understand what the file type actually is. 

    //
    // There are two methods to creating a Custom Data Source that is recognized by the SDK:
    //    1. Using the helper abstract base classes
    //    2. Implementing the raw interfaces
    // This sample demonstrates method 1 where the CustomDataSourceBase abstract class
    // helps provide a public parameterless constructor and implement the ICustomDataSource interface
    //

    public class HelloWorldCustomDataSource
        : CustomDataSourceBase
    {
        private IApplicationEnvironment applicationEnvironment;

        protected override void SetApplicationEnvironmentCore(IApplicationEnvironment applicationEnvironment)
        {
            //
            // Saves the given application environment into this instance
            //

            this.applicationEnvironment = applicationEnvironment;
        }

        protected override ICustomDataProcessor CreateProcessorCore(
            IEnumerable<IDataSource> dataSources,
            IProcessorEnvironment processorEnvironment,
            ProcessorOptions options)
        {
            //
            // Create a new instance implementing ICustomDataProcessor here to process the specified data sources.
            // Note that you can have more advanced logic here to create different processors if you would like based on the file, or any other criteria.
            // You are not restricted to always returning the same type from this method.
            //

            return new HelloWorldCustomDataProcessor(
                dataSources.Select(x => x.GetUri().LocalPath).ToArray(),
                options,
                this.applicationEnvironment,
                processorEnvironment,
                this.AllTables,
                this.MetadataTables);
        }

        protected override bool IsFileSupportedCore(string path)
        {
            return true;
        }
    }
}
