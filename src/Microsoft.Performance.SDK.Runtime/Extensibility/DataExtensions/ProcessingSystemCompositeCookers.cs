// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.

using System;
using System.Collections.Concurrent;
using System.Diagnostics;
using System.Threading;
using Microsoft.Performance.SDK.Extensibility;
using Microsoft.Performance.SDK.Extensibility.DataCooking;
using Microsoft.Performance.SDK.Runtime.Extensibility.DataExtensions.Repository;

namespace Microsoft.Performance.SDK.Runtime.Extensibility.DataExtensions
{
    /// <summary>
    ///     This owns all instances of composite cookers for a given system.
    /// </summary>
    public sealed class ProcessingSystemCompositeCookers
          : ICompositeCookerRepository
    {
        private readonly ReaderWriterLockSlim compositeCookerLock = new ReaderWriterLockSlim();

        private readonly ConcurrentDictionary<DataCookerPath, IDataCooker> compositeCookersByPath
            = new ConcurrentDictionary<DataCookerPath, IDataCooker>();

        private readonly IDataExtensionRepository dataExtensionRepository;
        private bool disposedValue;

        /// <summary>
        ///     Initializes an instance of this class.
        /// </summary>
        /// <param name="dataExtensionRepository">
        ///     A data extension repository.
        /// </param>
        public ProcessingSystemCompositeCookers(
            IDataExtensionRepository dataExtensionRepository)
        {
            this.dataExtensionRepository = dataExtensionRepository;
        }

        /// <inheritdoc/>
        public ICookedDataRetrieval GetOrCreateCompositeCooker(
            DataCookerPath cookerPath,
            Func<DataCookerPath, IDataExtensionRetrieval> createDataRetrieval)
        {
            this.ThrowIfDisposed();

            if (this.compositeCookersByPath.TryGetValue(cookerPath, out var dataCooker))
            {
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

            if (createDataRetrieval == null)
            {
                throw new ArgumentNullException(nameof(createDataRetrieval));
            }

            var compositeCooker = compositeCookerReference.CreateInstance(createDataRetrieval(cookerPath));
            if (compositeCooker == null)
            {
                Debug.Assert(false, "If the data cooker reference is available, why did this fail?");
                throw new InvalidOperationException(
                    $"The composite cooker reference returned null cooker: {cookerPath}. Was it properly initialized?");
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
            if (!this.disposedValue)
            {
                if (disposing)
                {
                    foreach (var cooker in this.compositeCookersByPath.Values)
                    {
                        cooker.TryDispose();
                    }

                    this.compositeCookersByPath.Clear();

                    this.compositeCookerLock.Dispose();
                }

                this.disposedValue = true;
            }
        }
    }
}
