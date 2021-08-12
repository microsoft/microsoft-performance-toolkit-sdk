// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.

using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using Microsoft.Performance.SDK.Extensibility;
using Microsoft.Performance.SDK.Extensibility.DataCooking;
using Microsoft.Performance.SDK.Processing;
using Microsoft.Performance.SDK.Runtime.Extensibility.DataExtensions.Repository;
using Microsoft.Performance.SDK.Runtime.Extensibility.DataExtensions.Tables;

namespace Microsoft.Performance.SDK.Runtime.Extensibility.DataExtensions
{
    /// <summary>
    ///     Provides access to extension data associated with a single <see cref="ICustomDataProcessor"/> or a grouped
    ///     set of <see cref="ICustomDataProcessor"/>s.
    /// </summary>
    public sealed class ProcessingSystemData
        : IProcessingSystemData,
          IDisposable
    {
        private readonly ProcessingSystemCookerData cookerDataRetrieval;

        // These two fields aren't marked readonly so that they can be set to null during while disposing.
        private DataExtensionRetrievalFactory dataExtensionRetrievalFactory;
        private ReadOnlyDictionary<TableDescriptor, ICustomDataProcessor> tableToProcessor;

        private bool disposedValue;

        /// <summary>
        ///     Initializes an instance of <see cref="ProcessingSystemData"/>.
        /// </summary>
        /// <param name="cookerDataRetrieval">
        ///     Provides access to all available cooker data.
        /// </param>
        /// <param name="dataExtensionRepository">
        ///     Provides access to data extensions.
        /// </param>
        /// <param name="tableToProcessorMap">
        ///     Maps a <see cref="TableDescriptor"/> to its <see cref="ICustomDataProcessor"/>.
        /// </param>
        /// <exception cref="ArgumentNullException">
        ///     <paramref name="cookerDataRetrieval"/> is null.
        ///     - or -
        ///     <paramref name="dataExtensionRepository"/> is null.
        ///     - or -
        ///     <paramref name="tableToProcessorMap"/> is null.
        /// </exception>
        public ProcessingSystemData(
            ICookedDataRetrieval sourceCookerData,
            ICompositeCookerRepository compositeCookers,
            IDictionary<TableDescriptor, ICustomDataProcessor> tableToProcessorMap,
            IDataExtensionRepository dataExtensionRepository)
            : this(
                  new ProcessingSystemCookerData(sourceCookerData, compositeCookers, true),
                  tableToProcessorMap,
                  dataExtensionRepository)
        {
        }

        /// <summary>
        ///     Initializes an instance of <see cref="ProcessingSystemData"/>.
        /// </summary>
        /// <param name="cookerDataRetrieval">
        ///     Provides access to all available cooker data.
        /// </param>
        /// <param name="dataExtensionRepository">
        ///     Provides access to data extensions.
        /// </param>
        /// <param name="tableToProcessorMap">
        ///     Maps a <see cref="TableDescriptor"/> to its <see cref="ICustomDataProcessor"/>.
        /// </param>
        /// <exception cref="ArgumentNullException">
        ///     <paramref name="cookerDataRetrieval"/> is null.
        ///     - or -
        ///     <paramref name="dataExtensionRepository"/> is null.
        ///     - or -
        ///     <paramref name="tableToProcessorMap"/> is null.
        /// </exception>
        internal ProcessingSystemData(
            ProcessingSystemCookerData cookerDataRetrieval,
            IDictionary<TableDescriptor, ICustomDataProcessor> tableToProcessorMap,
            IDataExtensionRepository dataExtensionRepository)
        {
            Guard.NotNull(cookerDataRetrieval, nameof(cookerDataRetrieval));
            Guard.NotNull(dataExtensionRepository, nameof(dataExtensionRepository));
            Guard.NotNull(tableToProcessorMap, nameof(tableToProcessorMap));

            this.cookerDataRetrieval = cookerDataRetrieval;
            this.dataExtensionRetrievalFactory = new DataExtensionRetrievalFactory(this.cookerDataRetrieval, dataExtensionRepository);
            this.tableToProcessor = new ReadOnlyDictionary<TableDescriptor, ICustomDataProcessor>(tableToProcessorMap);
        }

        /// <inheritdoc/>
        /// <exception cref="ObjectDisposedException">
        ///     This instance is disposed.
        /// </exception>
        public IReadOnlyDictionary<TableDescriptor, ICustomDataProcessor> TableToProcessor
        {
            get
            {
                this.ThrowIfDisposed();
                return this.tableToProcessor;
            }
        }

        /// <summary>
        ///     Gets the cooker data available to the processing system.
        /// </summary>
        /// <exception cref="ObjectDisposedException">
        ///     This instance is disposed.
        /// </exception>
        internal ProcessingSystemCookerData CookerData
        {
            get
            {
                this.ThrowIfDisposed();
                return this.CookerData;
            }
        }

        private ICookedDataRetrieval SourceCookerDataRetrieval
        {
            get
            {
                this.ThrowIfDisposed();
                return this.cookerDataRetrieval.SourceCookerData;
            }
        }

        private DataExtensionRetrievalFactory DataExtensionRetrievalFactory
        {
            get
            {
                this.ThrowIfDisposed();
                return this.dataExtensionRetrievalFactory;
            }
        }

        /// <inheritdoc/>
        /// <exception cref="ObjectDisposedException">
        ///     This instance is disposed.
        /// </exception>
        public T QueryOutput<T>(DataOutputPath identifier)
        {
            return this.SourceCookerDataRetrieval.QueryOutput<T>(identifier);
        }

        /// <inheritdoc/>
        /// <exception cref="ObjectDisposedException">
        ///     This instance is disposed.
        /// </exception>
        public object QueryOutput(DataOutputPath identifier)
        {
            return this.SourceCookerDataRetrieval.QueryOutput(identifier);
        }

        /// <inheritdoc/>
        /// <exception cref="ObjectDisposedException">
        ///     This instance is disposed.
        /// </exception>
        public bool TryQueryOutput<T>(DataOutputPath identifier, out T result)
        {
            return this.SourceCookerDataRetrieval.TryQueryOutput(identifier, out result);
        }

        /// <inheritdoc/>
        /// <exception cref="ObjectDisposedException">
        ///     This instance is disposed.
        /// </exception>
        public bool TryQueryOutput(DataOutputPath identifier, out object result)
        {
            return this.SourceCookerDataRetrieval.TryQueryOutput(identifier, out result);
        }

        /// <inheritdoc/>
        /// <exception cref="ObjectDisposedException">
        ///     This instance is disposed.
        /// </exception>
        public ICookedDataRetrieval GetCompositeCookerDataRetrieval(DataCookerPath cookerPath)
        {
            return this.CookerData.GetOrCreateCompositeCooker(
                cookerPath,
                this.dataExtensionRetrievalFactory.CreateDataRetrievalForCompositeDataCooker);
        }

        /// <inheritdoc/>
        /// <exception cref="ObjectDisposedException">
        ///     This instance is disposed.
        /// </exception>
        public IDataExtensionRetrieval CreateDataRetrievalForCompositeDataCooker(
            DataCookerPath dataCookerPath)
        {
            return this.DataExtensionRetrievalFactory.CreateDataRetrievalForCompositeDataCooker(dataCookerPath);
        }

        /// <inheritdoc/>
        /// <exception cref="ObjectDisposedException">
        ///     This instance is disposed.
        /// </exception>
        public IDataExtensionRetrieval CreateDataRetrievalForDataProcessor(DataProcessorId dataProcessorId)
        {
            return this.DataExtensionRetrievalFactory.CreateDataRetrievalForDataProcessor(dataProcessorId);
        }

        /// <inheritdoc/>
        /// <exception cref="ObjectDisposedException">
        ///     This instance is disposed.
        /// </exception>
        public IDataExtensionRetrieval CreateDataRetrievalForTable(Guid tableId)
        {
            return this.DataExtensionRetrievalFactory.CreateDataRetrievalForTable(tableId);
        }

        /// <inheritdoc/>
        /// <exception cref="ObjectDisposedException">
        ///     This instance is disposed.
        /// </exception>
        public IDataExtensionRetrieval CreateDataRetrievalForTable(ITableExtensionReference tableExtensionReference)
        {
            return this.DataExtensionRetrievalFactory.CreateDataRetrievalForTable(tableExtensionReference);
        }

        /// <inheritdoc/>
        public void Dispose()
        {
            // Do not change this code. Put cleanup code in 'Dispose(bool disposing)' method
            Dispose(disposing: true);
            GC.SuppressFinalize(this);
        }

        /// <summary>
        ///     Throws an exception if this instance has been disposed.
        /// </summary>
        /// <exception cref="ObjectDisposedException">
        ///     This instance is disposed.
        /// </exception>
        private void ThrowIfDisposed()
        {
            if (this.disposedValue)
            {
                throw new ObjectDisposedException(this.GetType().Name);
            }
        }

        private void Dispose(bool disposing)
        {
            if (!disposedValue)
            {
                if (disposing)
                {
                    this.cookerDataRetrieval.SafeDispose();
                }

                this.dataExtensionRetrievalFactory = null;
                this.tableToProcessor = null;

                disposedValue = true;
            }
        }
    }
}
