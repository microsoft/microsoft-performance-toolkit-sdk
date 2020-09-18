// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.

using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading;
using Microsoft.Performance.SDK.Extensibility.SourceParsing;
using Microsoft.Performance.SDK.Processing;
using Microsoft.Performance.SDK.Runtime;

namespace Microsoft.Performance.Toolkit.Engine.Tests.TestCookers.Source5
{
    public sealed class Source5Parser
        : SourceParserBase<Source5DataObject, EngineTestContext, int>
    {
        private readonly List<string> filePaths;

        public Source5Parser(
            IEnumerable<IDataSource> dataSources)
        {
            this.filePaths = dataSources
                .OfType<FileDataSource>()
                .Select(x => x.FullPath)
                .ToList();
        }

        public override string Id => nameof(Source5Parser);

        public override DataSourceInfo DataSourceInfo => DataSourceInfo.Default;

        public override void ProcessSource(
            ISourceDataProcessor<Source5DataObject, EngineTestContext, int> dataProcessor,
            ILogger logger,
            IProgress<int> progress, CancellationToken cancellationToken)
        {
            foreach (var line in this.filePaths
               .SelectMany(x => File.ReadAllLines(x))
               .Where(x => x.Length > 0 && x[0] != '#'))
            {
                var split = line.Split(',');
                dataProcessor.ProcessDataElement(
                     new Source5DataObject
                     {
                         Id = int.Parse(split[0]),
                         Data = split[1],
                     },
                     new EngineTestContext(),
                     cancellationToken);
            }
        }
    }
}
