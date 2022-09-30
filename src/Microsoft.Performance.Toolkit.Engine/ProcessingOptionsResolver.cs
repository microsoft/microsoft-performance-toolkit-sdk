using Microsoft.Performance.SDK;
using Microsoft.Performance.SDK.Processing;
using Microsoft.Performance.SDK.Processing.DataSourceGrouping;
using System.Collections.Generic;

namespace Microsoft.Performance.Toolkit.Engine
{
    // todo : refactor with IDataSourceGroup
    public sealed class ProcessingOptionsResolver : IProcessingOptionsResolver
    {
        private readonly IDictionary<IEnumerable<IDataSource>, ProcessorOptions> dataSourceGroupOptionsMap;

        public ProcessingOptionsResolver()
        {
            dataSourceGroupOptionsMap = new Dictionary<IEnumerable<IDataSource>, ProcessorOptions>();
        }


        public ProcessingOptionsResolver(IDictionary<IEnumerable<IDataSource>, ProcessorOptions> dataSourceGroupOptions)
        {
            dataSourceGroupOptionsMap = dataSourceGroupOptions;
        }

        public IDictionary<IEnumerable<IDataSource>, ProcessorOptions> OptionsForDataSourceGroups => dataSourceGroupOptionsMap;

        public ProcessorOptions GetProcessorOptions(IEnumerable<IDataSource> dataSourceGroup, IProcessingSource processingSource)
        {
            Guard.NotNull(dataSourceGroup, nameof(dataSourceGroup));
            Guard.NotNull(processingSource, nameof(processingSource));

            bool success = dataSourceGroupOptionsMap.TryGetValue(dataSourceGroup, out var options);
            return success ? options : ProcessorOptions.Default;
        }
    }
}
