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
        private IEnumerable<Option> options = new List<Option>()
        {
            new Option('r', "test"),
            new Option('s', "test1"),
            new Option('t', "test2")
        };

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
                processorEnvironment);
        }

        protected override bool IsDataSourceSupportedCore(IDataSource dataSource)
        {
            return StringComparer.OrdinalIgnoreCase.Equals(
                   Extension,
                   Path.GetExtension(dataSource.Uri.LocalPath));
        }

        public override IEnumerable<Option> CommandLineOptions => options;
        
        private sealed class Discovery
            : IProcessingSourceTableProvider
        {
            public IEnumerable<TableDescriptor> Discover(ITableConfigurationsSerializer tableConfigSerializer)
            {
                return new HashSet<TableDescriptor>
                {
                    Source123Table.TableDescriptor,
                };
            }
        }
    }
}
