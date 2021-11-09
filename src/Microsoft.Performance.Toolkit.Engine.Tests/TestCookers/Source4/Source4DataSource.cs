// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.

using System;
using System.Collections.Generic;
using System.IO;
using Microsoft.Performance.SDK.Processing;

namespace Microsoft.Performance.Toolkit.Engine.Tests.TestCookers.Source4
{
    [ProcessingSource(
       "{BE673F97-D329-4074-8A0D-FBA74AED130A}",
       nameof(Source4DataSource),
       "Source4 for Runtime Tests")]
    [FileDataSource(Extension)]
    public sealed class Source4DataSource
        : ProcessingSource
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
                processorEnvironment);
        }

        protected override bool IsDataSourceSupportedCore(IDataSource dataSource)
        {
            return StringComparer.OrdinalIgnoreCase.Equals(
                Extension,
                Path.GetExtension(dataSource.Uri.LocalPath));
        }
    }
}
