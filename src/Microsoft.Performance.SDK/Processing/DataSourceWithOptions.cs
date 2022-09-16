using System;
using System.Collections.Generic;
using System.Text;

namespace Microsoft.Performance.SDK.Processing
{
    public sealed class DataSourceWithOptions
    {
        private readonly IDataSource dataSource;
        private readonly ProcessorOptions processorOptions;

        public DataSourceWithOptions(IDataSource dataSource, ProcessorOptions processorOptions)
        {
            this.dataSource = dataSource;
            this.processorOptions = processorOptions;
        }

        public IDataSource DataSource => this.dataSource;
        public ProcessorOptions ProcessorOptions => this.processorOptions;
    }
}
