// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.

using Microsoft.Performance.SDK.Processing;
using System.Collections.Generic;

namespace Microsoft.Performance.SDK.Runtime.NetCoreApp.Tests.Plugins.MockCustomDataSources
{
    [CustomDataSource(
    "{82DD1CC4-9594-4F53-AA9C-64E188877F19}", "Mock CDS - Invalid B", "A mock invalid data source B.")]
    [FileDataSource(".sdk")]
    public class InvalidSchemaB
    : CustomDataSourceBase
    {
        protected override ICustomDataProcessor CreateProcessorCore(IEnumerable<IDataSource> dataSources, IProcessorEnvironment processorEnvironment, ProcessorOptions options)
        {
            throw new System.NotImplementedException();
        }

        protected override bool IsFileSupportedCore(string path)
        {
            return true;
        }

        protected override void SetApplicationEnvironmentCore(IApplicationEnvironment applicationEnvironment)
        {
            return;
        }
    }
}
