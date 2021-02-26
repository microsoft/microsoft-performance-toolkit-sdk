// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.

using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using Microsoft.Performance.SDK.Extensibility;
using Microsoft.Performance.SDK.Extensibility.DataCooking;
using Microsoft.Performance.SDK.Runtime.Extensibility.DataExtensions.DataCookers;
using Microsoft.Performance.SDK.Runtime.Extensibility.DataExtensions.DataProcessors;
using Microsoft.Performance.SDK.Runtime.Extensibility.DataExtensions.Tables;

namespace Microsoft.Performance.SDK.Runtime.Extensibility.DataExtensions.Repository
{
    /// <summary>
    ///     This class holds references to data extensions. It offers basic functionality to add/retrieve extensions.
    /// </summary>
    internal class DataExtensionRepository
        : IDataExtensionRepositoryBuilder
    {
        private readonly Dictionary<string, HashSet<ISourceDataCookerFactory>> dataCookerReferencesBySource 
            = new Dictionary<string, HashSet<ISourceDataCookerFactory>>(StringComparer.Ordinal);

        private readonly HashSet<ICompositeDataCookerReference> compositeDataCookerReferences 
            = new HashSet<ICompositeDataCookerReference>();

        private readonly Dictionary<DataCookerPath, ISourceDataCookerReference> sourceDataCookerReferencesByPath 
            = new Dictionary<DataCookerPath, ISourceDataCookerReference>();

        private readonly Dictionary<DataCookerPath, ICompositeDataCookerReference> compositeDataCookerReferencesByPath
            = new Dictionary<DataCookerPath, ICompositeDataCookerReference>();

        private readonly ConcurrentSet<IDataProcessorReference> dataProcessors
            = new ConcurrentSet<IDataProcessorReference>();

        private readonly ConcurrentDictionary<Guid, ITableExtensionReference> tablesById 
            =  new ConcurrentDictionary<Guid, ITableExtensionReference>();

        private bool isDisposed = false;

        ~DataExtensionRepository()
        {
            this.Dispose(false);
        }

        public IReadOnlyDictionary<Guid, ITableExtensionReference> TablesById
        {
            get
            {
                this.ThrowIfDisposed();
                return this.tablesById;
            }
        }

        public IEnumerable<DataCookerPath> SourceDataCookers
        {
            get
            {
                this.ThrowIfDisposed();
                return this.sourceDataCookerReferencesByPath.Keys;
            }
        }

        public IEnumerable<DataCookerPath> CompositeDataCookers
        {
            get
            {
                this.ThrowIfDisposed();
                return this.compositeDataCookerReferencesByPath.Keys;
            }
        }

        public IEnumerable<DataProcessorId> DataProcessors
        {
            get
            {
                this.ThrowIfDisposed();
                return this.dataProcessors.Select(x => new DataProcessorId(x.Id));
            }
        }

        public bool AddSourceDataCookerReference(ISourceDataCookerReference dataCooker)
        {
            Guard.NotNull(dataCooker, nameof(dataCooker));
            this.ThrowIfDisposed();

            bool addedCooker;

            lock (this.dataCookerReferencesBySource)
            {
                if (!this.dataCookerReferencesBySource.ContainsKey(dataCooker.Path.SourceParserId))
                {
                    this.dataCookerReferencesBySource.Add(dataCooker.Path.SourceParserId, new HashSet<ISourceDataCookerFactory>());
                }

                addedCooker = this.dataCookerReferencesBySource[dataCooker.Path.SourceParserId].Add(dataCooker);
                if (addedCooker)
                {
                    this.sourceDataCookerReferencesByPath[dataCooker.Path] = dataCooker;
                }
            }

            return addedCooker;
        }

        public bool AddCompositeDataCookerReference(ICompositeDataCookerReference dataCooker)
        {
            Guard.NotNull(dataCooker, nameof(dataCooker));
            this.ThrowIfDisposed();

            bool addedCooker = false;

            lock (this.compositeDataCookerReferences)
            {
                addedCooker = this.compositeDataCookerReferences.Add(dataCooker);
                if (addedCooker)
                {
                    this.compositeDataCookerReferencesByPath[dataCooker.Path] = dataCooker;
                }
            }

            return addedCooker;
        }

        public ISourceDataCookerFactory GetSourceDataCookerFactory(DataCookerPath dataCookerPath)
        {
            this.ThrowIfDisposed();

            return this.GetSourceDataCookerReference(dataCookerPath);
        }

        public ISourceDataCookerReference GetSourceDataCookerReference(DataCookerPath dataCookerPath)
        {
            this.ThrowIfDisposed();

            if (!this.sourceDataCookerReferencesByPath.TryGetValue(dataCookerPath, out var reference))
            {
                return null;
            }
            if (!this.sourceDataCookerReferencesByPath.ContainsKey(dataCookerPath))
            {
                return null;
            }

            return this.sourceDataCookerReferencesByPath[dataCookerPath];
        }

        public ICompositeDataCookerReference GetCompositeDataCookerReference(DataCookerPath dataCookerPath)
        {
            this.ThrowIfDisposed();

            if (!this.compositeDataCookerReferencesByPath.TryGetValue(dataCookerPath, out var reference))
            {
                return null;
            }
            if (!this.compositeDataCookerReferencesByPath.ContainsKey(dataCookerPath))
            {
                return null;
            }

            return this.compositeDataCookerReferencesByPath[dataCookerPath];
        }

        public bool AddTableExtensionReference(
            ITableExtensionReference tableExtensionReference)
        {
            Guard.NotNull(tableExtensionReference, nameof(tableExtensionReference));
            this.ThrowIfDisposed();

            return this.tablesById.TryAdd(tableExtensionReference.TableDescriptor.Guid, tableExtensionReference);
        }

        public bool AddDataProcessorReference(IDataProcessorReference dataProcessorReference)
        {
            Guard.NotNull(dataProcessorReference, nameof(dataProcessorReference));
            this.ThrowIfDisposed();

            return this.dataProcessors.Add(dataProcessorReference);
        }

        public IDataProcessorReference GetDataProcessorReference(DataProcessorId dataProcessorId)
        {
            this.ThrowIfDisposed();

            return this.dataProcessors.FirstOrDefault(reference =>
                StringComparer.Ordinal.Equals(reference.Id, dataProcessorId));
        }

        /// <summary>
        /// After all data extensions have been added, this is called to process dependencies on each of the
        /// data extensions.
        /// </summary>
        public void FinalizeDataExtensions()
        {
            this.ThrowIfDisposed();

            foreach (var kvp in this.sourceDataCookerReferencesByPath)
            {
                kvp.Value.ProcessDependencies(this);
            }

            foreach (var dataCookerReference in this.compositeDataCookerReferences)
            {
                dataCookerReference.ProcessDependencies(this);
            }

            foreach (var dataProcessorReference in this.dataProcessors)
            {
                dataProcessorReference.ProcessDependencies(this);
            }

            foreach (var kvp in this.TablesById)
            {
                kvp.Value.ProcessDependencies(this);
            }
        }

        public void Dispose()
        {
            this.Dispose(true);
            GC.SuppressFinalize(this);
        }

        protected virtual void Dispose(bool disposing)
        {
            if (this.isDisposed)
            {
                return;
            }

            if (disposing)
            {
                foreach (var c in this.compositeDataCookerReferences)
                {
                    c.TryDispose();
                }

                foreach (var s in this.sourceDataCookerReferencesByPath.Values)
                {
                    s.TryDispose();
                }

                foreach (var d in this.dataProcessors)
                {
                    d.TryDispose();
                }

                this.compositeDataCookerReferences.Clear();
                this.compositeDataCookerReferencesByPath.Clear();
                this.sourceDataCookerReferencesByPath.Clear();
                this.dataCookerReferencesBySource.Clear();
                this.dataProcessors.Clear();
                this.tablesById.Clear();
            }

            this.isDisposed = true;
        }

        private void ThrowIfDisposed()
        {
            if (this.isDisposed)
            {
                throw new ObjectDisposedException(nameof(DataExtensionRepository));
            }
        }
    }
}
