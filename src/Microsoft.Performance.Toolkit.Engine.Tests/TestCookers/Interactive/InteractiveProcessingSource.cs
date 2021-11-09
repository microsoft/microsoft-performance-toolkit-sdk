// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.

using System.Collections.Generic;
using Microsoft.Performance.SDK.Processing;

namespace Microsoft.Performance.Toolkit.Engine.Tests.TestCookers.Interactive
{
    [ProcessingSource(
      "{83BC3100-C96A-4C07-92AA-0A9B0CB83232}",
      nameof(InteractiveProcessingSource),
      "Interactive Processing Source for Engine Tests")]
    [FileDataSource(Extension)]
    public class InteractiveProcessingSource
        : ProcessingSource
    {
        public const string Extension = ".ips";

        protected override ICustomDataProcessor CreateProcessorCore(
            IEnumerable<IDataSource> dataSources,
            IProcessorEnvironment processorEnvironment,
            ProcessorOptions options)
        {
            return new InteractiveProcessor(
                options,
                this.ApplicationEnvironment,
                processorEnvironment,
                this.AllTables);
        }

        protected override bool IsDataSourceSupportedCore(
            IDataSource dataSource)
        {
            return true;
        }
    }
}
