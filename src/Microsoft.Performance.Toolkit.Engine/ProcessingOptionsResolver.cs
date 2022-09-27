using Microsoft.Performance.SDK.Processing;
using System.Collections.Generic;

namespace Microsoft.Performance.Toolkit.Engine
{
    public class ProcessingOptionsResolver : IProcessingOptionsResolver
    {
        private readonly IDictionary<IEnumerable<IDataSource>, ProcessorOptions> dataSourceOptions;

        public ProcessingOptionsResolver()
        {
            dataSourceOptions = new Dictionary<IEnumerable<IDataSource>, ProcessorOptions>();
        }

        public ProcessingOptionsResolver(IDictionary<IEnumerable<IDataSource>, ProcessorOptions> dataSourceOptions)
        {
            this.dataSourceOptions = dataSourceOptions;
        }

        public IDictionary<IEnumerable<IDataSource>, ProcessorOptions> OptionsForDataSources => dataSourceOptions;

        public ProcessorOptions GetProcessorOptions(IEnumerable<IDataSource> dataSourceGroup, IProcessingSource processingSource)
        {
            bool success = dataSourceOptions.TryGetValue(dataSourceGroup, out var options);
            return success ? options : ProcessorOptions.Default;
        }
    }
}
