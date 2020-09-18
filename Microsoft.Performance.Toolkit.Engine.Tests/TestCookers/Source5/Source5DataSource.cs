// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.

using System;
using System.Collections.Generic;
using System.IO;
using Microsoft.Performance.SDK.Processing;

namespace Microsoft.Performance.Toolkit.Engine.Tests.TestCookers.Source5
{
    [CustomDataSource(
       "{6DBA249D-F113-4F8D-A758-CD030D6C16F8}",
       nameof(Source5DataSource),
       "Source5 for Runtime Tests")]
    [FileDataSource(Extension)]
    public sealed class Source5DataSource
        : CustomDataSourceBase
    {
        public const string Extension = ".s5d";

        protected override ICustomDataProcessor CreateProcessorCore(
            IEnumerable<IDataSource> dataSources, 
            IProcessorEnvironment processorEnvironment, 
            ProcessorOptions options)
        {
            var parser = new Source5Parser(dataSources);

            return new Source5Processor(
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
