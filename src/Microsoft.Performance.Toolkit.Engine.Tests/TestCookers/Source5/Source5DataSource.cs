// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.

using System;
using System.Collections.Generic;
using System.IO;
using Microsoft.Performance.SDK.Extensibility;
using Microsoft.Performance.SDK.Processing;
using Microsoft.Performance.Toolkit.Engine.Tests.TestTables;

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
        internal static readonly int BuildActionInt = 2600;

        public const string Extension = ".s5d";

        public Source5DataSource()
            : base(new Discovery())
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

        /// <inheritdoc/>
        /// This is used to establish the build action for internal table <see cref="Source5InternalTableNoBuildAction"/>.
        protected override Action<ITableBuilder, IDataExtensionRetrieval> GetTableBuildAction(
            Type type)
        {
            if (type == typeof(Source5InternalTableNoBuildAction))
            {
                return (builder, data) => Source5InternalTableNoBuildAction.BuildTableAction(builder, data, Source5DataSource.BuildActionInt);
            }

            return null;
        }

        private sealed class Discovery
            : ITableProvider
        {
            private static readonly ITableProvider DefaultProvider
                = TableDiscovery.CreateForAssembly(typeof(Source5DataSource).Assembly);

            public IEnumerable<DiscoveredTable> Discover(ISerializer tableConfigSerializer)
            {
                var tables = new HashSet<DiscoveredTable>(DefaultProvider.Discover(tableConfigSerializer));
                tables.Add(
                    new DiscoveredTable(Source5InternalTable.TableDescriptor, Source5InternalTable.BuildTableAction));
                return tables;
            }
        }
    }
}
