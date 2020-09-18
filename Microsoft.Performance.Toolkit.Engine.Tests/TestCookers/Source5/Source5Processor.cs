// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.

using System;
using System.Collections.Generic;
using Microsoft.Performance.SDK.Extensibility;
using Microsoft.Performance.SDK.Extensibility.SourceParsing;
using Microsoft.Performance.SDK.Processing;

namespace Microsoft.Performance.Toolkit.Engine.Tests.TestCookers.Source5
{
    public sealed class Source5Processor
        : CustomDataProcessorBaseWithSourceParser<Source5DataObject, EngineTestContext, int>
    {
        public Source5Processor(
            ISourceParser<Source5DataObject, EngineTestContext, int> sourceParser, 
            ProcessorOptions options,
            IApplicationEnvironment applicationEnvironment, 
            IProcessorEnvironment processorEnvironment, 
            IReadOnlyDictionary<TableDescriptor, Action<ITableBuilder, IDataExtensionRetrieval>> allTablesMapping,
            IEnumerable<TableDescriptor> metadataTables) 
            : base(sourceParser, options, applicationEnvironment, processorEnvironment, allTablesMapping, metadataTables)
        {
        }
    }
}
