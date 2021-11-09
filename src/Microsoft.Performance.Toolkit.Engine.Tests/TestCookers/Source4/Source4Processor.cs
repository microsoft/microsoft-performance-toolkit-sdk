// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.

using System.Collections.Generic;
using Microsoft.Performance.SDK.Extensibility.SourceParsing;
using Microsoft.Performance.SDK.Processing;

namespace Microsoft.Performance.Toolkit.Engine.Tests.TestCookers.Source4
{
    public sealed class Source4Processor
        : CustomDataProcessorWithSourceParser<Source4DataObject, EngineTestContext, int>
    {
        public Source4Processor(
            ISourceParser<Source4DataObject, EngineTestContext, int> sourceParser,
            ProcessorOptions options,
            IApplicationEnvironment applicationEnvironment,
            IProcessorEnvironment processorEnvironment,
            IEnumerable<TableDescriptor> allTables)
            : base(sourceParser, options, applicationEnvironment, processorEnvironment, allTables)
        {
        }
    }
}
