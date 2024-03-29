﻿// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.

using Microsoft.Performance.SDK.Processing;
using System.Collections.Generic;

namespace Microsoft.Performance.SDK.Runtime.NetCoreApp.Tests.Plugins.MockProcessingSources
{
    [ProcessingSource(
    "{DE9D525C-3D4F-4498-B0BA-63CED1E4E14A}", "Mock PS - Invalid A", "A mock invalid data source A.")]
    [FileDataSource(".sdk")]
    public class InvalidSchemaA
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
