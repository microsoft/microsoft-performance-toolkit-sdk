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

        /// <summary>
        ///     Initializes a new instance of the <see cref="ExtensionException"/>
        ///     class, encapsulating the given catalog and repository.
        /// </summary>
        /// <param name="catalog">
        ///     The catalog to use to manage plugins.
        /// </param>
        /// <param name="repository">
        ///     The repository to use to manage data extensions.
        /// </param>
        /// <exception cref="System.ArgumentNullException">
        ///     <paramref name="catalog"/> is <c>null</c>.
        ///     - or -
        ///     <paramref name="repository"/> is <c>null</c>.
        /// </exception>
        public ExtensionRoot(
            IPlugInCatalog catalog,
            IDataExtensionRepositoryBuilder repository)
        {
            Guard.NotNull(catalog, nameof(catalog));
            Guard.NotNull(repository, nameof(repository));

            this.dependencyGraph = null;

            this.Catalog = catalog;
            this.Repository = repository;

            this.isDisposed = false;
        }

        /// <summary>
        ///     Finalizes an instance of the <see cref="ExtensionRoot"/>
        ///     class.
        /// </summary>
        ~ExtensionRoot()
        {
            this.Dispose(false);
        }

        /// <summary>
        ///     Gets the catalog exposing plugins.
        /// </summary>
        /// <exception cref="System.ObjectDisposedException">
        ///     This instance is disposed.
        /// </exception>
        public IPlugInCatalog Catalog { get; }

        /// <summary>
        ///     Gets the repository exposing data extensions.
        /// </summary>
        /// <exception cref="System.ObjectDisposedException">
        ///     This instance is disposed.
        /// </exception>
        public IDataExtensionRepositoryBuilder Repository { get; }

        /// <inheritdoc />
        public bool IsLoaded => this.Catalog.IsLoaded;

        /// <inheritdoc />
        public IEnumerable<CustomDataSourceReference> PlugIns => this.Catalog.PlugIns;

        /// <inheritdoc />
        public IEnumerable<DataCookerPath> SourceDataCookers => this.Repository.SourceDataCookers;

        /// <inheritdoc />
        public IEnumerable<DataCookerPath> CompositeDataCookers => this.Repository.CompositeDataCookers;

        /// <inheritdoc />
        public IReadOnlyDictionary<Guid, ITableExtensionReference> TablesById => this.Repository.TablesById;

        /// <inheritdoc />
        public IEnumerable<DataProcessorId> DataProcessors => this.Repository.DataProcessors;

        /// <inheritdoc />
        public bool AddCompositeDataCookerReference(ICompositeDataCookerReference dataCooker)
        {
            return this.Repository.AddCompositeDataCookerReference(dataCooker);
        }

        /// <inheritdoc />
        public bool AddDataProcessorReference(IDataProcessorReference dataProcessorReference)
        {
            return this.Repository.AddDataProcessorReference(dataProcessorReference);
        }

        /// <inheritdoc />
        public bool AddSourceDataCookerReference(ISourceDataCookerReference dataCooker)
        {
            return this.Repository.AddSourceDataCookerReference(dataCooker);
        }

        /// <inheritdoc />
        public bool AddTableExtensionReference(ITableExtensionReference tableExtensionReference)
        {
            return this.Repository.AddTableExtensionReference(tableExtensionReference);
        }

        /// <inheritdoc />
        public void Dispose()
        {
            this.Dispose(true);
            GC.SuppressFinalize(this);
        }

        /// <inheritdoc />
        public void FinalizeDataExtensions()
        {
            this.Repository.FinalizeDataExtensions();
            this.dependencyGraph = DependencyDag.Create(this.Catalog, this.Repository);
        }

        /// <inheritdoc />
        public ICompositeDataCookerReference GetCompositeDataCookerReference(DataCookerPath dataCookerPath)
        {
            return this.Repository.GetCompositeDataCookerReference(dataCookerPath);
        }

        /// <inheritdoc />
        public IDataProcessorReference GetDataProcessorReference(DataProcessorId dataProcessorId)
        {
            return this.Repository.GetDataProcessorReference(dataProcessorId);
        }

        /// <inheritdoc />
        public ISourceDataCookerFactory GetSourceDataCookerFactory(DataCookerPath dataCookerPath)
        {
            return this.Repository.GetSourceDataCookerFactory(dataCookerPath);
        }

        /// <inheritdoc />
        public ISourceDataCookerReference GetSourceDataCookerReference(DataCookerPath dataCookerPath)
        {
            return this.Repository.GetSourceDataCookerReference(dataCookerPath);
        }

        /// <summary>
        ///     Releases all instances of Custom Data Processors and Data
        ///     Extensions, without tearing down the repositories.
        /// </summary>
        public void Release()
        {
            this.ThrowIfDisposed();

            var dag = DependencyDag.Create(this.Catalog, this.Repository);
            dag.DependentWalk(
                x => x.Target.Match(
                    r => r.Release(),
                    r => r.Release()));
        }

        private void Dispose(bool disposing)
        {
            if (this.isDisposed)
            {
                return;
            }

            if (disposing)
            {
                try
                {
                    this.Release();
                }
                finally
                {
                    this.Catalog.SafeDispose();
                    this.Repository.SafeDispose();
                }
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
