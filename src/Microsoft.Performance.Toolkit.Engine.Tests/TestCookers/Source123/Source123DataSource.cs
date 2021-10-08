// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.

using System;
using System.Collections.Generic;
using System.IO;
using Microsoft.Performance.SDK.Processing;

namespace Microsoft.Performance.Toolkit.Engine.Tests.TestCookers.Source123
{
    [ProcessingSource(
       "{FA37C400-D2C4-48F7-B485-48F119BEE02E}",
       nameof(Source123DataSource),
       "Source123 for Runtime Tests")]
    [FileDataSource(Extension)]
    public sealed class Source123DataSource
        : ProcessingSource
    {
        public const string Extension = ".s123d";

        public Source123DataSource()
            : base(new Discovery())
        {
        }

        protected override ICustomDataProcessor CreateProcessorCore(
            IEnumerable<IDataSource> dataSources, 
            IProcessorEnvironment processorEnvironment, 
            ProcessorOptions options)
        {
            var parser = new Source123Parser(dataSources);

            return new Source123Processor(
                parser,
                options,
                this.ApplicationEnvironment,
                processorEnvironment,
                this.AllTables,
                this.MetadataTables);
        }

        protected override bool IsDataSourceSupportedCore(IDataSource dataSource)
        {
            return StringComparer.OrdinalIgnoreCase.Equals(
                   Extension,
                   Path.GetExtension(dataSource.Uri.LocalPath));
        }

        private sealed class Discovery
            : ITableProvider
        {
            public IEnumerable<DiscoveredTable> Discover(ISerializer tableConfigSerializer)
            {
                return new HashSet<DiscoveredTable>
                {
                    new DiscoveredTable(Source123Table.TableDescriptor),
                };
            }
        }
    }
}
