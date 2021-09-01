// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.

using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Diagnostics;
using System.Linq;
using Microsoft.Performance.SDK;
using Microsoft.Performance.SDK.Processing;
using Microsoft.Performance.SDK.Runtime;

namespace Microsoft.Performance.Toolkit.Engine
{
    /// <summary>
    ///     Represents a collection of <see cref="IDataSource"/>s to process.
    /// </summary>
    public sealed class DataSourceSet
         : IDisposable
    {
        private readonly Dictionary<ProcessingSourceReference, List<List<IDataSource>>> dataSourcesToProcess;
        private readonly List<IDataSource> freeDataSources;
        private readonly ReadOnlyCollection<IDataSource> freeDataSourcesRO;
        private readonly List<ProcessingSourceReference> processingSourceReferencesList;

        private readonly PluginSet plugins;
        private readonly bool ownsPlugins;

        private bool isDisposed;

        /// <summary>
        ///     Initializes a new instance of the <see cref="DataSourceSet"/>
        ///     class, referencing the given <see cref="PluginSet"/>.
        /// </summary>
        /// <param name="plugins">
        ///     The plugins that are available to process data sources.
        /// </param>
        /// <param name="ownsPlugins">
        ///     <c>true</c> to take ownership and dispose <paramref name="plugins"/>
        ///     when this instance is disposed; <c>false</c> to leave
        ///     <paramref name="plugins"/> undisposed.
        /// </param>
        /// <exception cref="System.ArgumentNullException">
        ///     <paramref name="plugins"/> is <c>null</c>.
        /// </exception>
        private DataSourceSet(PluginSet plugins, bool ownsPlugins)
        {
            Guard.NotNull(plugins, nameof(plugins));

            this.dataSourcesToProcess = new Dictionary<ProcessingSourceReference, List<List<IDataSource>>>();
            this.freeDataSources = new List<IDataSource>();
            this.freeDataSourcesRO = new ReadOnlyCollection<IDataSource>(this.freeDataSources);
            this.processingSourceReferencesList = plugins.ProcessingSourceReferences.ToList();

            this.plugins = plugins;
            this.ownsPlugins = ownsPlugins;
            this.isDisposed = false;
        }

        /// <summary>
        ///     Gets the collection of Data Sources grouped by their <see cref="IProcessingSource"/>.
        /// </summary>
        /// <exception cref="ObjectDisposedException">
        ///     This instance is disposed.
        /// </exception>
        public IReadOnlyDictionary<ProcessingSourceReference, IReadOnlyList<IReadOnlyList<IDataSource>>> AssignedDataSourcesToProcess
        {
            get
            {
                this.ThrowIfDisposed();
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
                this.ThrowIfDisposed();
                return this.freeDataSourcesRO;
            }
        }

        /// <summary>
        ///     Gets the plugins backing this set of data sources.
        /// </summary>
        public PluginSet Plugins
        {
            get
            {
                this.ThrowIfDisposed();
                return this.plugins;
            }
        }

        /// <summary>
        ///     Creates a new instance of the <see cref="DataSourceSet"/>
        ///     class, using plugins loaded from the current working
        ///     directory.
        /// </summary>
        public static DataSourceSet Create()
        {
            PluginSet plugins = null;
            try
            {
                plugins = PluginSet.Load();
                return Create(plugins);
            }
            catch
            {
                plugins.SafeDispose();
                throw;
            }
        }

        /// <summary>
        ///     Creates a new instance of the <see cref="DataSourceSet"/>
        ///     class, referencing the given <see cref="PluginSet"/>.
        /// </summary>
        /// <param name="plugins">
        ///     The plugins that are available to process data sources.
        /// </param>
        /// <exception cref="System.ArgumentNullException">
        ///     <paramref name="plugins"/> is <c>null</c>.
        /// </exception>
        public static DataSourceSet Create(PluginSet plugins)
        {
            return Create(plugins, true);
        }

        /// <summary>
        ///     Creates a new instance of the <see cref="DataSourceSet"/>
        ///     class, referencing the given <see cref="PluginSet"/>.
        /// </summary>
        /// <param name="plugins">
        ///     The plugins that are available to process data sources.
        /// </param>
        /// <param name="ownsPlugins">
        ///     <c>true</c> to take ownership and dispose <paramref name="plugins"/>
        ///     when this instance is disposed; <c>false</c> to leave
        ///     <paramref name="plugins"/> undisposed.
        /// </param>
        /// <exception cref="System.ArgumentNullException">
        ///     <paramref name="plugins"/> is <c>null</c>.
        /// </exception>
        /// <exception cref="NotSupportedException">
        ///     <paramref name="ownsPlugins"/> is <c>false</c>. This feature will
        ///     be enabled in a future update.
        /// </exception>
        public static DataSourceSet Create(PluginSet plugins, bool ownsPlugins)
        {
            return new DataSourceSet(plugins, ownsPlugins);
        }

        /// <inheritdoc />
        public void Dispose()
        {
            this.Dispose(true);
            GC.SuppressFinalize(this);
        }

        /// <summary>
        ///     Adds the given file to this instance for processing.
        /// </summary>
        /// <param name="dataSource">
        ///     The Data Source to process.
        /// </param>
        /// <returns>
        ///    A reference to this instance after the operation has completed.
        /// </returns>
        /// <exception cref="ArgumentNullException">
        ///     <paramref name="dataSource"/> is <c>null</c>.
        /// </exception>
        /// <exception cref="InvalidOperationException">
        ///     This instance is sealed.
        /// </exception>
        /// <exception cref="ObjectDisposedException">
        ///     This instance is disposed.
        /// </exception>
        /// <exception cref="UnsupportedDataSourceException">
        ///     <paramref name="dataSource"/> cannot be processed by any
        ///     discovered extensions.
        /// </exception>
        public DataSourceSet AddDataSource(IDataSource dataSource)
        {
            this.ThrowIfDisposed();

            Guard.NotNull(dataSource, nameof(dataSource));

            if (!this.TryAddDataSource(dataSource))
            {
                throw new UnsupportedDataSourceException(dataSource);
            }

            return this;
        }

        /// <summary>
        ///     Attempts to add the given Data Source to this instance for processing.
        /// </summary>
        /// <param name="filePath">
        ///     The Data Source to process.
        /// </param>
        /// <returns>
        ///     <c>true</c> if the Data Source has been added for processing;
        ///     <c>false</c> if the Data Source is not valid, cannot be processed,
        ///     or the instance has already been processed. Note that <c>false</c>
        ///     is always returned when <see cref="IsProcessed"/> is <c>true</c>.
        /// </returns>
        /// <exception cref="ObjectDisposedException">
        ///     This instance is disposed.
        /// </exception>
        public bool TryAddDataSource(IDataSource dataSource)
        {
            this.ThrowIfDisposed();

            if (dataSource is null)
            {
                return false;
            }

            if (!this.processingSourceReferencesList.Any(x => x.Supports(dataSource)))
            {
                return false;
            }

            try
            {
                this.freeDataSources.Add(dataSource);
            }
            catch
            {
                return false;
            }

            return true;
        }

        /// <summary>
        ///     Adds the given Data Source to this instance for processing by
        ///     the specific <see cref="IProcessingSource"/>.
        /// </summary>
        /// <param name="dataSource">
        ///     The Data Source to process.
        /// </param>
        /// <param name="processingSourceType">
        ///     The <see cref="IProcessingSource"/> to use to process the file.
        /// </param>
        /// <returns>
        ///    A reference to this instance after the operation has completed.
        /// </returns>
        /// <exception cref="ArgumentNullException">
        ///     <paramref name="dataSource"/> is <c>null</c>.
        ///     - or -
        ///     <paramref name="processingSourceType"/> is <c>null</c>.
        /// </exception>
        /// <exception cref="InstanceAlreadyProcessedException">
        ///     This instance has already been processed.
        /// </exception>
        /// <exception cref="ObjectDisposedException">
        ///     This instance is disposed.
        /// </exception>
        /// <exception cref="UnsupportedDataSourceException">
        ///     The specified <paramref name="processingSourceType"/> cannot handle
        ///     the given Data Source.
        /// </exception>
        /// <exception cref="UnsupportedProcessingSourceException">
        ///     The specified <paramref name="processingSourceType"/> is unknown.
        /// </exception>
        public DataSourceSet AddDataSource(IDataSource dataSource, Type processingSourceType)
        {
            return this.AddDataSources(new[] { dataSource, }, processingSourceType);
        }

        /// <summary>
        ///     Attempts to add the given Data Source to this instance for processing by
        ///     the specific <see cref="IProcessingSource"/>.
        /// </summary>
        /// <param name="dataSource">
        ///     The Data Source to process.
        /// </param>
        /// <param name="processingSourceType">
        ///     The <see cref="IProcessingSource"/> to use to process <paramref name="dataSource"/>.
        /// </param>
        /// <returns>
        ///     <c>true</c> if the <see cref="IDataSource"/> has been added for processing by the <see cref="IProcessingSource"/>;
        ///     <c>false</c> otherwise. Note that <c>false</c>
        ///     is always returned when <see cref="IsProcessed"/> is <c>true</c>.
        /// </returns>
        /// <exception cref="ObjectDisposedException">
        ///     This instance is disposed.
        /// </exception>
        public bool TryAddDataSource(IDataSource dataSource, Type processingSourceType)
        {
            return this.TryAddDataSources(new[] { dataSource, }, processingSourceType);
        }

        /// <summary>
        ///     Adds the given data sources to this instance for processing by
        ///     the specific <see cref="IProcessingSource"/>. All of the files will be processed
        ///     by the same instance of the Custom Data Processor. Use <see cref="AddDataSource(IDataSource, Type)"/>
        ///     to ensure each Data Source is processed by a different instance, or
        ///     use multiple calls to <see cref="AddDataSources(IEnumerable{IDataSource}, Type)"/>.
        /// </summary>
        /// <param name="dataSources">
        ///     The Data Sources to process.
        /// </param>
        /// <param name="processingSourceType">
        ///     The <see cref="IProcessingSource"/> to use to process the <paramref name="dataSources"/>.
        /// </param>
        /// <returns>
        ///    A reference to this instance after the operation has completed.
        /// </returns>
        /// <exception cref="ArgumentNullException">
        ///     <paramref name="dataSources"/> is <c>null</c>.
        ///     - or -
        ///     <paramref name="processingSourceType"/> is <c>null</c>.
        /// </exception>
        /// <exception cref="InvalidOperationException">
        ///     This instance is sealed.
        /// </exception>
        /// <exception cref="ObjectDisposedException">
        ///     This instance is disposed.
        /// </exception>
        /// <exception cref="UnsupportedDataSourceException">
        ///     The specified <paramref name="processingSourceType"/> cannot handle
        ///     the given Data Source.
        /// </exception>
        /// <exception cref="UnsupportedProcessingSourceException">
        ///     The specified <paramref name="processingSourceType"/> is unknown.
        /// </exception>
        public DataSourceSet AddDataSources(IEnumerable<IDataSource> dataSources, Type processingSourceType)
        {
            this.ThrowIfDisposed();

            Guard.NotNull(dataSources, nameof(dataSources));
            Guard.NotNull(processingSourceType, nameof(processingSourceType));
            if (dataSources.Any(x => x is null))
            {
                throw new ArgumentNullException(nameof(dataSources));
            }

            this.AddDataSourcesCore(dataSources, processingSourceType, this.processingSourceReferencesList, this.dataSourcesToProcess, this.TypeIs);

            return this;
        }

        /// <summary>
        ///     Attempts to add the given data sources to this instance for processing by
        ///     the specific <see cref="IProcessingSource"/>. All of the files will be processed
        ///     by the same instance of the Custom Data Processor. Use <see cref="AddDataSource(IDataSource, Type)"/>
        ///     to ensure each Data Source is processed by a different instance, or
        ///     use multiple calls to <see cref="AddDataSources(IEnumerable{IDataSource}, Type)"/>.
        /// </summary>
        /// <param name="dataSources">
        ///     The Data Sources to process.
        /// </param>
        /// <param name="processingSourceType">
        ///     The <see cref="IProcessingSource"/> to use to process the <paramref name="dataSources"/>.
        /// </param>
        /// <exception cref="ObjectDisposedException">
        ///     This instance is disposed.
        /// </exception>
        public bool TryAddDataSources(IEnumerable<IDataSource> dataSources, Type processingSourceType)
        {
            this.ThrowIfDisposed();

            if (dataSources is null ||
                dataSources.Any(x => x is null) ||
                processingSourceType is null)
            {
                return false;
            }

            try
            {
                this.AddDataSourcesCore(dataSources, processingSourceType, this.processingSourceReferencesList, this.dataSourcesToProcess, this.TypeIs);
                return true;
            }
            catch
            {
                return false;
            }
        }

        /// <summary>
        ///     Seals this instance, preventing any new data sources from being added.
        /// </summary>
        /// <returns>
        ///    A reference to this instance after the operation has completed.
        /// </returns>
        /// <exception cref="ObjectDisposedException">
        ///     This instance is disposed.
        /// </exception>
        public ReadOnlyDataSourceSet AsReadOnly()
        {
            this.ThrowIfDisposed();

            //
            // Create a deep copy of the given data source set. This
            // is used to make sure that the readonly set cannot be
            // changed by manipulating the original data source set reference.
            // Because a DataSourceSet may own the plugin set,
            // copies created by this method will *never* own the plugin
            // set from the original in order to avoid ambiguity in
            // ownership of the plugin set. This means that the result
            // of this method is not an exact deep copy, but for the 
            // purposes of the engine, it works. If we decide we want to
            // expose copy functionality to the public, then we need to
            // figure out how to deal with ownership of the underlying
            // plugin set in regards to copies so that we can communicate
            // them appropriately.
            //

            var dataSourcesToProcess = new Dictionary<ProcessingSourceReference, List<List<IDataSource>>>();
            var freeDataSources = new List<IDataSource>();
            var processingSourceReferencesList = new List<ProcessingSourceReference>();

            foreach (var kvp in this.dataSourcesToProcess)
            {
                dataSourcesToProcess[kvp.Key] = kvp.Value
                    .Select(x => new List<IDataSource>(x))
                    .ToList();
            }

            freeDataSources.AddRange(this.FreeDataSourcesToProcess);
            processingSourceReferencesList.AddRange(this.processingSourceReferencesList);

            return new ReadOnlyDataSourceSet(
                dataSourcesToProcess,
                freeDataSources,
                this.plugins);
        }

        private bool TypeIs(Type first, Type second)
        {
            Debug.Assert(!this.isDisposed);
            Debug.Assert(first != null);
            Debug.Assert(second != null);

            if (this.Plugins.ArePluginsIsolated)
            {
                return first.GUID == second.GUID &&
                       first.AssemblyQualifiedName == second.AssemblyQualifiedName;
            }
            else
            {
                return first.Is(second);
            }
        }

        private void AddDataSourcesCore(
            IEnumerable<IDataSource> dataSources,
            Type processingSourceType,
            List<ProcessingSourceReference> processingSourceReferences,
            Dictionary<ProcessingSourceReference, List<List<IDataSource>>> dataSourcesToProcess,
            Func<Type, Type, bool> typeIs)
        {
            Debug.Assert(!this.isDisposed);

            Debug.Assert(dataSources != null);
            Debug.Assert(processingSourceType != null);
            Debug.Assert(processingSourceReferences != null);
            Debug.Assert(dataSourcesToProcess != null);
            Debug.Assert(typeIs != null);

            var cdsr = processingSourceReferences.FirstOrDefault(x => typeIs(x.Instance.GetType(), processingSourceType));
            if (cdsr is null)
            {
                throw new UnsupportedProcessingSourceException(processingSourceType);
            }

            var atLeastOneDataSourceProvided = false;
            foreach (var dataSource in dataSources)
            {
                Debug.Assert(dataSource != null);

                atLeastOneDataSourceProvided = true;
                if (!cdsr.Supports(dataSource))
                {
                    throw new UnsupportedDataSourceException(dataSource, processingSourceType);
                }
            }

            if (!atLeastOneDataSourceProvided)
            {
                throw new ArgumentException("The Data Source collection cannot be empty.", nameof(dataSources));
            }

            if (!dataSourcesToProcess.TryGetValue(cdsr, out var list))
            {
                list = new List<List<IDataSource>>();
                dataSourcesToProcess[cdsr] = list;
            }

            list.Add(dataSources.ToList());
        }

        private void Dispose(bool disposing)
        {
            if (this.isDisposed)
            {
                return;
            }

            if (disposing)
            {
                if (this.ownsPlugins)
                {
                    this.plugins.SafeDispose();
                }
            }

            this.isDisposed = true;
        }

        private void ThrowIfDisposed()
        {
            if (this.isDisposed)
            {
                throw new ObjectDisposedException(nameof(Engine));
            }
        }
    }
}
