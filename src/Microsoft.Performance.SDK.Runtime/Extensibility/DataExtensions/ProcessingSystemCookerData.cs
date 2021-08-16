// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.

using System;
using Microsoft.Performance.SDK.Extensibility;
using Microsoft.Performance.SDK.Extensibility.DataCooking;

namespace Microsoft.Performance.SDK.Runtime.Extensibility.DataExtensions
{
    internal sealed class ProcessingSystemCookerData
        : IProcessingSystemCookerData
    {
        private readonly ICompositeCookerRepository compositeCookers;
        private readonly bool ownsCompositeCookers;

        // This isn't readonly so that it can be set to null while disposing.
        private ICookedDataRetrieval sourceCookerData;

        private bool disposedValue;

        internal ProcessingSystemCookerData(
            ICookedDataRetrieval sourceCookerData,
            ICompositeCookerRepository compositeCookers,
            bool ownsCompositeCookers)
        {
            Guard.NotNull(sourceCookerData, nameof(sourceCookerData));
            Guard.NotNull(compositeCookers, nameof(compositeCookers));

            this.sourceCookerData = sourceCookerData;
            this.compositeCookers = compositeCookers;
            this.ownsCompositeCookers = ownsCompositeCookers;
        }

        public ICookedDataRetrieval SourceCookerData
        {
            get
            {
                this.ThrowIfDisposed();
                return this.sourceCookerData;
            }
        }

        public ICookedDataRetrieval GetOrCreateCompositeCooker(
            DataCookerPath cookerPath,
            Func<DataCookerPath, IDataExtensionRetrieval> createDataRetrieval)
        {
            this.ThrowIfDisposed();
            return this.compositeCookers.GetOrCreateCompositeCooker(cookerPath, createDataRetrieval);
        }

        /// <inheritdoc/>
        public void Dispose()
        {
            // Do not change this code. Put cleanup code in 'Dispose(bool disposing)' method
            Dispose(disposing: true);
            GC.SuppressFinalize(this);
        }

        private void Dispose(bool disposing)
        {
            if (!this.disposedValue)
            {
                if (disposing)
                {
                    if (this.ownsCompositeCookers)
                    {
                        this.compositeCookers.SafeDispose();
                    }
                }

                this.sourceCookerData = null;
                this.disposedValue = true;
            }
        }

        private void ThrowIfDisposed()
        {
            if (this.disposedValue)
            {
                throw new ObjectDisposedException(nameof(ProcessingSystemCookerData));
            }
        }
    }
}
