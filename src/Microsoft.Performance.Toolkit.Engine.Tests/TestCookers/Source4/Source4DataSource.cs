// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.

using System;
using System.Collections.Generic;
using System.IO;
using Microsoft.Performance.SDK.Processing;

namespace Microsoft.Performance.Toolkit.Engine.Tests.TestCookers.Source4
{
    [CustomDataSource(
       "{BE673F97-D329-4074-8A0D-FBA74AED130A}",
       nameof(Source4DataSource),
       "Source4 for Runtime Tests")]
    [FileDataSource(Extension)]
    public sealed class Source4DataSource
        : CustomDataSourceBase
    {
        public const string Extension = ".s4d";

        protected override ICustomDataProcessor CreateProcessorCore(
            IEnumerable<IDataSource> dataSources, 
            IProcessorEnvironment processorEnvironment, 
            ProcessorOptions options)
        {
            var parser = new Source4Parser(dataSources);

            return new Source4Processor(
                parser,
                options,
                this.ApplicationEnvironment,
                processorEnvironment,
                this.AllTables,
                this.MetadataTables);
        }

        protected override bool IsFileSupportedCore(
            string path)
        {
            return StringComparer.OrdinalIgnoreCase.Equals(
                Extension,
                Path.GetExtension(path));
        }

        protected override void SetApplicationEnvironmentCore(
            IApplicationEnvironment applicationEnvironment)
        {
        }
    }
}
