// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.

using System;
using System.Collections.Generic;
using Microsoft.Performance.SDK.Extensibility;
using Microsoft.Performance.SDK.Extensibility.SourceParsing;
using Microsoft.Performance.SDK.Processing;

namespace Microsoft.Performance.Toolkit.Engine.Tests.TestCookers.Source123
{
    public sealed class Source123Processor
        : CustomDataProcessorWithSourceParser<Source123DataObject, EngineTestContext, int>
    {
        public Source123Processor(
            ISourceParser<Source123DataObject, EngineTestContext, int> sourceParser, 
            ProcessorOptions options,
            IApplicationEnvironment applicationEnvironment, 
            IProcessorEnvironment processorEnvironment, 
            IReadOnlyDictionary<TableDescriptor, Action<ITableBuilder, IDataExtensionRetrieval>> allTablesMapping,
            IEnumerable<TableDescriptor> metadataTables) 
            : base(sourceParser, options, applicationEnvironment, processorEnvironment, allTablesMapping, metadataTables)
        {
        }

        protected override void BuildTableCore(TableDescriptor tableDescriptor, Action<ITableBuilder, IDataExtensionRetrieval> createTable, ITableBuilder tableBuilder)
        {           
            if (tableDescriptor.Equals(Source123Table.TableDescriptor) && createTable == null)
            {
                var table = new Source123Table();
                table.Build(tableBuilder);
            }
            else
            {
                base.BuildTableCore(tableDescriptor, createTable, tableBuilder);
            }            
        }
    }
}
