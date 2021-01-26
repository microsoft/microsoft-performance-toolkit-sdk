// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.

using Microsoft.Performance.SDK.Processing;
using System.Collections.Generic;

namespace Microsoft.Performance.SDK.Runtime.NetCoreApp.Tests.Plugins.MockCustomDataSources
{
    [CustomDataSource(
    "{0A3709CE-BA67-498B-9AB4-FC14CC517DAC}", "Mock CDS - Legacy A", "A mock legacy data source A.")]
    [FileDataSource(".sdk")]
    public class LegacySchemaA
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
