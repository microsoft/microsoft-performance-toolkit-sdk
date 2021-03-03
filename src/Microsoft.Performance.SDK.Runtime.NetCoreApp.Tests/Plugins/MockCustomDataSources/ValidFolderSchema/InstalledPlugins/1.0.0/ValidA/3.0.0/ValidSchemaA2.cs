// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.

using Microsoft.Performance.SDK.Processing;
using System.Collections.Generic;

// NOTE: this file contains a duplicate class definition for "ValidSchemaA2" in this namespace.
// The test project will not include this file in its build, so the conflict does not result
// in compilation errors. When this file is compiled programatically in unit tests,
// the resulting assembly will be an exact copy of the 2.0.0 version of this file.
//
// This setup mimicks a plugin containing an older, unmodified custom data source in
// a new release. In the event that both versions of the plugin are loaded, we want
// to ensure that only one instance of the CDS is advertised by the plugin loader.
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
