// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.

using Microsoft.Performance.SDK.Processing;
using System.Collections.Generic;

namespace Microsoft.Performance.SDK.Runtime.NetCoreApp.Tests.Plugins.MockProcessingSources
{
    [ProcessingSource(
    "{571640BE-E380-483A-81D8-A9A79EDCFF1E}", "Mock PS - Valid A v2.0.0", "A mock valid data source A2.")]
    [FileDataSource(".sdk")]
    public class ValidSchemaA2
    : ProcessingSource
    {
        protected override ICustomDataProcessor CreateProcessorCore(IEnumerable<IDataSource> dataSources, IProcessorEnvironment processorEnvironment, ProcessorOptions options)
        {
            throw new System.NotImplementedException();
        }

        protected override bool IsDataSourceSupportedCore(IDataSource dataSource)
        {
            return true;
        }
    }
}
