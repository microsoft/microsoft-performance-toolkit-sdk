// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.

using Microsoft.Performance.SDK.Processing;
using System.Collections.Generic;

namespace Microsoft.Performance.SDK.Runtime.NetCoreApp.Tests.Plugins.MockCustomDataSources
{
    [CustomDataSource(
    "{AAC2FD6A-6AA3-4BF8-BD74-D4D2AF7EA6D8}", "Mock CDS - Valid A v1.0.0", "A mock valid data source A1.")]
    [FileDataSource(".sdk")]
    public class ValidSchemaA1
    : CustomDataSourceBase
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
