// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.

using System;
using System.Collections.Concurrent;
using System.Diagnostics;
using Microsoft.Performance.SDK.Extensibility;
using Microsoft.Performance.SDK.Extensibility.DataCooking;
using Microsoft.Performance.SDK.Runtime.Extensibility.DataExtensions.Repository;

namespace Microsoft.Performance.SDK.Runtime.Extensibility.DataExtensions
{
    /// <summary>
    ///     This owns all instances of composite cookers for a given processing system.
    /// </summary>
    public sealed class ProcessingSystemCompositeCookers
          : ICompositeCookerRepository
    {
        private readonly ConcurrentDictionary<DataCookerPath, IDataCooker> compositeCookersByPath
            = new ConcurrentDictionary<DataCookerPath, IDataCooker>();

        private IDataExtensionRepository dataExtensionRepository;
        private IDataExtensionRetrievalFactory retrievalFactory;
        private bool disposedValue;

        /// <summary>
        ///     Initializes an instance of this class.
        /// </summary>
        /// <param name="dataExtensionRepository">
        ///     A data extension repository.
        /// </param>
        /// <exception cref="ArgumentNullException">
        ///     <paramref name="dataExtensionRepository"/> is <c>null</c>.
        /// </exception>
        public ProcessingSystemCompositeCookers(
            IDataExtensionRepository dataExtensionRepository)
        {
            Guard.NotNull(dataExtensionRepository, nameof(dataExtensionRepository));

            this.dataExtensionRepository = dataExtensionRepository;
        }

        /// <summary>
        ///     Initializes this class with a <see cref="IDataExtensionRetrievalFactory"/>.
        /// </summary>
        /// <param name="retrievalFactory">
        ///     Provides access to data needed to instantiate a composite cooker.
        /// </param>
        /// <exception cref="ObjectDisposedException">
        ///     This instance is disposed.
        /// </exception>
        /// <exception cref="System.ArgumentNullException">
        ///     <paramref name="retrievalFactory"/> is <c>null</c>.
        /// </exception>
        public void Initialize(IDataExtensionRetrievalFactory retrievalFactory)
        {
            this.ThrowIfDisposed();
            Guard.NotNull(retrievalFactory, nameof(retrievalFactory));

            this.retrievalFactory = retrievalFactory;
        }

        /// <inheritdoc/>
        public ICookedDataRetrieval GetCookerOutput(DataCookerPath dataCookerPath)
        {
            this.ThrowIfDisposed();

            Debug.Assert(
                this.retrievalFactory != null,
                $"{nameof(Initialize)} needs to be called before accessing composite cookers.");

            return this.GetOrCreateCompositeCooker(
                dataCookerPath,
                this.retrievalFactory.CreateDataRetrievalForCompositeDataCooker);
        }

        internal FilteredCompositeCookers CreateFilteredRepository()
        {
            return new FilteredCompositeCookers(this);
        }

        private ICookedDataRetrieval GetOrCreateCompositeCooker(
            DataCookerPath cookerPath,
            Func<DataCookerPath, IDataExtensionRetrieval> createDataRetrieval)
        {
            Debug.Assert(createDataRetrieval != null);

            this.ThrowIfDisposed();

            Debug.Assert(
                this.retrievalFactory != null,
                $"{nameof(Initialize)} needs to be called before accessing composite cookers.");

            if (this.compositeCookersByPath.TryGetValue(cookerPath, out var dataCooker))
            {
                // The cooker already exists and is ready to use.
                return dataCooker;
            }

            var compositeCookerReference = this.dataExtensionRepository.GetCompositeDataCookerReference(cookerPath);
            if (compositeCookerReference == null)
            {
                throw new ArgumentException(
                    $"The data extension repository is missing expected composite data cooker: {cookerPath}");
            }

            if (compositeCookerReference.Availability != DataExtensionAvailability.Available)
            {
                throw new ArgumentException(
                    $"The data cooker is not available: {cookerPath}");
            }

            var compositeCookerRequiredData = createDataRetrieval(cookerPath);
            if (compositeCookerRequiredData == null)
            {
                // is this a bug in the runtime?
                Debug.Assert(false);
                throw new InvalidOperationException(
                    $"Failed to create {nameof(IDataExtensionRetrieval)} needed for the composite cooker.");
            }

            var compositeCooker = compositeCookerReference.CreateInstance(compositeCookerRequiredData);
            if (compositeCooker == null)
            {
                Debug.Assert(false, "If the data cooker reference is available, this shouldn't fail.");
            }

            return this.compositeCookersByPath.GetOrAdd(cookerPath, compositeCooker);
        }

        /// <inheritdoc/>
        public void Dispose()
        {
            // Do not change this code. Put cleanup code in 'Dispose(bool disposing)' method
            Dispose(disposing: true);
            GC.SuppressFinalize(this);
        }

        private void ThrowIfDisposed()
        {
            if (this.disposedValue)
            {
                throw new ObjectDisposedException(this.GetType().Name);
            }
        }

        private void Dispose(bool disposing)
        {
            if (!this.disposedValue)
            {
                if (disposing)
                {
                    foreach (var cooker in this.compositeCookersByPath.Values)
                    {
                        cooker.TryDispose();
                    }

                    this.compositeCookersByPath.Clear();
                }

                this.dataExtensionRepository = null;
                this.retrievalFactory = null;
                this.disposedValue = true;
            }
        }

        /// <summary>
        ///     This class uses <see cref="ProcessingSystemCompositeCookers"/> to create and store composite
        ///     cookers, but uses a its own <see cref="IDataExtensionRetrievalFactory"/> which has access
        ///     to a subset of source data cookers.
        /// </summary>
        internal sealed class FilteredCompositeCookers
            : ICompositeCookerRepository
        {
            private bool disposedValue;

            private ProcessingSystemCompositeCookers compositeCookers;
            private IDataExtensionRetrievalFactory retrievalFactory;

            internal FilteredCompositeCookers(
                ProcessingSystemCompositeCookers compositeCookers)
            {
                this.compositeCookers = compositeCookers;
            }

            public ICookedDataRetrieval GetCookerOutput(DataCookerPath cookerPath)
            {
                this.ThrowIfDisposed();

                if (this.retrievalFactory == null)
                {
                    Debug.Assert(false, $"{nameof(Initialize)} needs to be called before accessing composite cookers.");
                    throw new InvalidOperationException(
                        $"{nameof(ProcessingSystemCompositeCookers.Initialize)} hasn't been called.");
                }

                return this.compositeCookers.GetOrCreateCompositeCooker(
                    cookerPath,
                    this.retrievalFactory.CreateDataRetrievalForCompositeDataCooker);
            }

            public void Dispose()
            {
                // Do not change this code. Put cleanup code in 'Dispose(bool disposing)' method
                Dispose(disposing: true);
                GC.SuppressFinalize(this);
            }

            internal void Initialize(IDataExtensionRetrievalFactory retrievalFactory)
            {
                this.retrievalFactory = retrievalFactory;
            }

            private void Dispose(bool disposing)
            {
                if (!disposedValue)
                {
                    if (disposing)
                    {
                        // nothing to do, this class doesn't own anything
                    }

                    this.compositeCookers = null;
                    this.retrievalFactory = null;
                    disposedValue = true;
                }
            }

            private void ThrowIfDisposed()
            {
                if (this.disposedValue)
                {
                    throw new ObjectDisposedException(this.GetType().Name);
                }
            }
        }
    }
}
