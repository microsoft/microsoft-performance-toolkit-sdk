// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.

using System;
using System.Collections.Generic;
using Microsoft.Performance.SDK.Extensibility;
using Microsoft.Performance.SDK.Extensibility.DataCooking;
using Microsoft.Performance.SDK.Runtime.Discovery;
using Microsoft.Performance.SDK.Runtime.Extensibility.DataExtensions.DataCookers;
using Microsoft.Performance.SDK.Runtime.Extensibility.DataExtensions.DataProcessors;
using Microsoft.Performance.SDK.Runtime.Extensibility.DataExtensions.Dependency;
using Microsoft.Performance.SDK.Runtime.Extensibility.DataExtensions.Repository;
using Microsoft.Performance.SDK.Runtime.Extensibility.DataExtensions.Tables;

namespace Microsoft.Performance.SDK.Runtime
{
    /// <summary>
    ///     Provides the root of all loaded extensions.
    /// </summary>
    public sealed class ExtensionRoot
        : IPlugInCatalog,
          IDataExtensionRepositoryBuilder
    {
        private DependencyDag dependencyGraph;

        private bool isDisposed;

        public ExtensionRoot(
            IPlugInCatalog catalog,
            IDataExtensionRepositoryBuilder repository)
        {
            this.dependencyGraph = null;

            this.Catalog = catalog;
            this.Repository = repository;

            this.isDisposed = false;
        }

        ~ExtensionRoot()
        {
            this.Dispose(false);
        }

        public IPlugInCatalog Catalog { get; }

        public IDataExtensionRepositoryBuilder Repository { get; }

        public DependencyDag Graph
        {
            get
            {
                this.ThrowIfDisposed();
                if (this.dependencyGraph is null)
                {
                    throw new InvalidOperationException(nameof(FinalizeDataExtensions) +
                        " must have been called before the DAG can be inspected.");
                }

                return this.dependencyGraph;
            }
        }

        public bool IsLoaded => this.Catalog.IsLoaded;

        public IEnumerable<CustomDataSourceReference> PlugIns => this.Catalog.PlugIns;

        public IEnumerable<DataCookerPath> SourceDataCookers => this.Repository.SourceDataCookers;

        public IEnumerable<DataCookerPath> CompositeDataCookers => this.Repository.CompositeDataCookers;

        public IReadOnlyDictionary<Guid, ITableExtensionReference> TablesById => this.Repository.TablesById;

        public IEnumerable<DataProcessorId> DataProcessors => this.Repository.DataProcessors;

        public bool AddCompositeDataCookerReference(ICompositeDataCookerReference dataCooker)
        {
            return this.Repository.AddCompositeDataCookerReference(dataCooker);
        }

        public bool AddDataProcessorReference(IDataProcessorReference dataProcessorReference)
        {
            return this.Repository.AddDataProcessorReference(dataProcessorReference);
        }

        public bool AddSourceDataCookerReference(ISourceDataCookerReference dataCooker)
        {
            return this.Repository.AddSourceDataCookerReference(dataCooker);
        }

        public bool AddTableExtensionReference(ITableExtensionReference tableExtensionReference)
        {
            return this.Repository.AddTableExtensionReference(tableExtensionReference);
        }

        public void Dispose()
        {
            this.Dispose(true);
            GC.SuppressFinalize(this);
        }

        public void FinalizeDataExtensions()
        {
            this.Repository.FinalizeDataExtensions();
            this.dependencyGraph = DependencyDag.Create(this.Catalog, this.Repository);
        }

        public ICompositeDataCookerReference GetCompositeDataCookerReference(DataCookerPath dataCookerPath)
        {
            return this.Repository.GetCompositeDataCookerReference(dataCookerPath);
        }

        public IDataProcessorReference GetDataProcessorReference(DataProcessorId dataProcessorId)
        {
            return this.Repository.GetDataProcessorReference(dataProcessorId);
        }

        public ISourceDataCookerFactory GetSourceDataCookerFactory(DataCookerPath dataCookerPath)
        {
            return this.Repository.GetSourceDataCookerFactory(dataCookerPath);
        }

        public ISourceDataCookerReference GetSourceDataCookerReference(DataCookerPath dataCookerPath)
        {
            return this.Repository.GetSourceDataCookerReference(dataCookerPath);
        }

        private void Dispose(bool disposing)
        {
            if (this.isDisposed)
            {
                return;
            }

            if (disposing)
            {
                var dag = DependencyDag.Create(this.Catalog, this.Repository);
                dag.DependentWalk(x => x.Target.Match(r => r.SafeDispose(), r => r.SafeDispose()));

                this.Catalog.SafeDispose();
                this.Repository.SafeDispose();
            }

            this.isDisposed = true;
        }

        private void ThrowIfDisposed()
        {
            if (this.isDisposed)
            {
                throw new ObjectDisposedException(nameof(ExtensionRoot));
            }
        }
    }
}
