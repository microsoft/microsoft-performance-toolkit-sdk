using System.Collections.Generic;
using System.Linq;
using Microsoft.Performance.SDK.Processing;

namespace DataExtensionsSample
{
    /// <summary>
    /// This custom data source defines the type of data it will process. In this case, a file with a "des" filename
    /// extension. 
    /// </summary>
    [CustomDataSource(
        "{96630F86-3FEA-4D21-A72E-8449D8784272}",
        "Data Extensions Sample",
        "Data Extensions Sample")]
    [FileDataSource("des", "Data Extension Sample")]
    public class CustomDataSource
        : CustomDataSourceBase
    {
        IApplicationEnvironment applicationEnvironment;

        protected override ICustomDataProcessor CreateProcessorCore(
            IEnumerable<IDataSource> dataSources,
            IProcessorEnvironment processorEnvironment,
            ProcessorOptions options)
        {
            var parser = new CustomSourceParser(dataSources.First().GetUri().LocalPath);

            var dataProcessor = new CustomDataProcessor(
                parser,
                options,
                this.applicationEnvironment,
                processorEnvironment,
                this.AllTables,
                this.MetadataTables);

            return dataProcessor;
        }

        /// <summary>
        /// This method is called to perform additional checks on the data source, to confirm that the data is contains
        /// can be processed. This is helpful for common file extensions, such as ".xml" or ".log". This method could
        /// peek inside at the contents confirm whether it is associated with this custom data source.
        ///
        /// For this sample, we just assume that if the file name is a match, it is handled by this add-in.
        /// </summary>
        /// <param name="path">Path to the source file</param>
        /// <returns>true when <param name="path"> is handled by this add-in</param></returns>
        protected override bool IsFileSupportedCore(string path)
        {
            return true;
        }

        /// <summary>
        /// This method just saves the application environment so that it can be used later.
        /// </summary>
        /// <param name="applicationEnvironment">Contains information helpful to future processing</param>
        protected override void SetApplicationEnvironmentCore(IApplicationEnvironment applicationEnvironment)
        {
            this.applicationEnvironment = applicationEnvironment;
        }
    }
}
