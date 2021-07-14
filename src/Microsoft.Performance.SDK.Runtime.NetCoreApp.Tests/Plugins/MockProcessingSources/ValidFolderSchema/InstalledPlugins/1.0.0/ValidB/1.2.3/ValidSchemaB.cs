// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.

using Microsoft.Performance.SDK.Processing;
using System.Collections.Generic;

namespace Microsoft.Performance.SDK.Runtime.NetCoreApp.Tests.Plugins.MockProcessingSources
{
    [ProcessingSource(
    "{BD25B1C8-E608-49A7-8521-3711BA125DBE}", "Mock PS - Valid B v1.2.3", "A mock valid data source B.")]
    [FileDataSource(".sdk")]
    public class ValidSchemaB
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
