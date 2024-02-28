// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.

using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using Microsoft.Performance.SDK.Processing;
using Microsoft.Performance.Testing.SDK;

namespace Microsoft.Performance.Toolkit.Engine.Tests.TestCookers.Source123
{
    [ProcessingSource(
       GuidAsString,
       nameof(Source123DataSource),
       "Source123 for Runtime Tests")]
    [FileDataSource(Extension)]
    public sealed class Source123DataSource
        : ProcessingSource,
          IDisposable
    {
        private IEnumerable<Option> supportedOptions = new List<Option>()
        {
            FakeProcessingSourceOptions.FakeOptionOne,
            FakeProcessingSourceOptions.FakeOptionTwo,
            FakeProcessingSourceOptions.FakeOptionThree,
        };

        public const string GuidAsString = "{FA37C400-D2C4-48F7-B485-48F119BEE02E}";
        public static Guid Guid = Guid.Parse(GuidAsString);
        public const string Extension = ".s123d";

        private bool disposedValue;

        // saves the constructor stack. useful for debugging cases where Dispose wasn't called.
        private readonly StackTrace constructionStack = null;

        public Source123DataSource()
            : base(new Discovery())
        {
            this.constructionStack = new StackTrace(true);
        }

        public bool IsDisposed => this.disposedValue;

        protected override ICustomDataProcessor CreateProcessorCore(
            IEnumerable<IDataSource> dataSources,
            IProcessorEnvironment processorEnvironment,
            ProcessorOptions options)
        {
            var parser = new Source123Parser(dataSources);
            this.UserSpecifiedOptions = options;

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

        public override IEnumerable<Option> CommandLineOptions => supportedOptions;

        public ProcessorOptions UserSpecifiedOptions { get; private set; }

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

        private void Dispose(bool disposing)
        {
            if (!disposedValue)
            {
                if (disposing)
                {
                }

                disposedValue = true;
            }
        }

        ~Source123DataSource()
        {
            throw new InvalidOperationException($"{nameof(Source123DataSource)} was not disposed of correctly.");
        }

        public void Dispose()
        {
            // Do not change this code. Put cleanup code in 'Dispose(bool disposing)' method
            Dispose(disposing: true);
            GC.SuppressFinalize(this);
        }
    }
}
