// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.

using System;
using System.Collections.Generic;
using System.IO;
using Microsoft.Performance.SDK.Processing;
using Microsoft.Performance.Testing.SDK;

namespace Microsoft.Performance.Toolkit.Engine.Tests.TestCookers.Source5
{
    [ProcessingSource(
       "{6DBA249D-F113-4F8D-A758-CD030D6C16F8}",
       nameof(Source5DataSource),
       "Source5 for Runtime Tests")]
    [FileDataSource(Extension)]
    public sealed class Source5DataSource
        : ProcessingSource
    {
        private IEnumerable<Option> supportedOptions = new List<Option>()
        {
            FakeProcessingSourceOptions.FakeOptionTwo,
            FakeProcessingSourceOptions.FakeOptionThree,
        };

        public const string Extension = ".s5d";

        public Source5DataSource()
            : base()
        {
        }

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
                processorEnvironment);
        }

        protected override bool IsDataSourceSupportedCore(IDataSource dataSource)
        {
            return StringComparer.OrdinalIgnoreCase.Equals(
                Extension,
                Path.GetExtension(dataSource.Uri.LocalPath));
        }

        public override IEnumerable<Option> CommandLineOptions => supportedOptions;
    }
}
