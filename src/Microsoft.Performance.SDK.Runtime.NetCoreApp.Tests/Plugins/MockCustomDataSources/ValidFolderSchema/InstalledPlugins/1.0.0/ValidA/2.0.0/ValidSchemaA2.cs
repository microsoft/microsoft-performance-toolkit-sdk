﻿// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.

using Microsoft.Performance.SDK.Processing;
using System.Collections.Generic;

namespace Microsoft.Performance.SDK.Runtime.NetCoreApp.Tests.Plugins.MockCustomDataSources
{
    [CustomDataSource(
    "{571640BE-E380-483A-81D8-A9A79EDCFF1E}", "Mock CDS - Valid A v2.0.0", "A mock valid data source A2.")]
    [FileDataSource(".sdk")]
    public class ValidSchemaA2
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

        protected override void SetApplicationEnvironmentCore(IApplicationEnvironment applicationEnvironment)
        {
            return;
        }
    }
}
