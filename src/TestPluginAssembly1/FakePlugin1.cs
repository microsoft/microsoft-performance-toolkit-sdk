// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.

using System;
using System.Collections.Generic;
using System.IO;
using Microsoft.Performance.SDK.Processing;

namespace TestPluginAssembly1
{
    [ProcessingSource(
        "{B7785024-EC4B-459E-B3A0-D0ED32548603}",
        nameof(FakePlugin1),
        "Source for Tests")]
    [FileDataSource(Extension)]
    public sealed class FakePlugin1
        : ProcessingSource
    {
        public const string Extension = ".fp1";

        protected override ICustomDataProcessor CreateProcessorCore(
            IEnumerable<IDataSource> dataSources,
            IProcessorEnvironment processorEnvironment,
            ProcessorOptions options)
        {
            throw new NotImplementedException();
        }

        protected override bool IsDataSourceSupportedCore(IDataSource dataSource)
        {
            return StringComparer.OrdinalIgnoreCase.Equals(
                Extension,
                Path.GetExtension(dataSource.Uri.LocalPath));
        }
    }
}