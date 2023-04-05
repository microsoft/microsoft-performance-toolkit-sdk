// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.

using Microsoft.Performance.SDK.Processing;

namespace TestPluginAssembly2
{
    [ProcessingSource(
        "{D30C3E14-E55A-427F-8C9F-F3F58D5C398A}",
        nameof(FakePlugin2),
        "Source for Tests")]
    [FileDataSource(Extension)]
    public sealed class FakePlugin2
        : ProcessingSource
    {
        public const string Extension = ".fp2";

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