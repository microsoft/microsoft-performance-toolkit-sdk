// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.

using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using Microsoft.Performance.SDK.Processing;
using Microsoft.Performance.SDK.Runtime;

namespace Microsoft.Performance.Toolkit.Engine
{
    /// <summary>
    ///     Presents a readonly view of a <see cref="DataSourceSet"/>.
    /// </summary>
    /// <remarks>
    ///     Instances of this class are only valid for as long as the creating
    ///     <see cref="DataSourceSet"/> is valid. Instances of this class are
    ///     invalidated when the creating <see cref="DataSourceSet"/> is disposed.
    ///     The behavior of instances of this class after the creating 
    ///     <see cref="DataSourceSet"/> is disposed is undefined.
    /// </remarks>
    public sealed class ReadOnlyDataSourceSet
    {
        private readonly Dictionary<ProcessingSourceReference, List<List<IDataSource>>> dataSourcesToProcess;
        private readonly Dictionary<ProcessingSourceReference, List<ProcessorOptions>> processorOptionsToProcess;
        private readonly List<DataSourceWithOptions> freeDataSourcesToProcess;
        private readonly ReadOnlyCollection<DataSourceWithOptions> freeDataSourcesRO;
        private readonly PluginSet plugins;

        internal ReadOnlyDataSourceSet(
            Dictionary<ProcessingSourceReference, List<List<IDataSource>>> dataSourcesToProcess,
            Dictionary<ProcessingSourceReference, List<ProcessorOptions>> processorOptionsToProcess,
            List<DataSourceWithOptions> freeDataSourcesToProcess, // todo : create another constructor? or change this to list<DataSourceWIthOptions>
            PluginSet plugins)
        {
            this.dataSourcesToProcess = dataSourcesToProcess;
            this.processorOptionsToProcess = processorOptionsToProcess;
            this.freeDataSourcesToProcess = freeDataSourcesToProcess;
            this.freeDataSourcesRO = this.freeDataSourcesToProcess.AsReadOnly();
            this.plugins = plugins;
        }

        /// <summary>
        ///     Gets the collection of Data Sources that are to be processed by a specific <see cref="IProcessingSource"/>.
        /// </summary>
        /// <exception cref="ObjectDisposedException">
        ///     This instance is disposed.
        /// </exception>
        public IReadOnlyDictionary<ProcessingSourceReference, IReadOnlyList<IReadOnlyList<IDataSource>>> DataSourcesToProcess
        {
            get
            {
                return this.dataSourcesToProcess.ToDictionary(
                    x => x.Key,
                    x => (IReadOnlyList<IReadOnlyList<IDataSource>>)x.Value
                        .Select(v => (IReadOnlyList<IDataSource>)v.AsReadOnly())
                        .ToList()
                        .AsReadOnly());
            }
        }

        /// <summary>
        ///     Gets the collection of Data Sources to process. These Data Sources will be processed in whatever
        ///     <see cref="ICustomDataProcessorWithSourceParser"/> are able to handle the Data Source.
        /// </summary>
        /// <exception cref="ObjectDisposedException">
        ///     This instance is disposed.
        /// </exception>
        public IEnumerable<IDataSource> FreeDataSourcesToProcess
        {
            get
            {
                return this.freeDataSourcesRO.Select(ds => ds.DataSource);
            }
        }

        /// <summary>
        ///     Gets the plugins backing this set of data sources.
        /// </summary>
        public PluginSet Plugins
        {
            get
            {
                return this.plugins;
            }
        }
    }
}
