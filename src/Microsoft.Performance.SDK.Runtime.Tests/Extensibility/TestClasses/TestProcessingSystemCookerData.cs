// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.

using System;
using Microsoft.Performance.SDK.Extensibility;
using Microsoft.Performance.SDK.Extensibility.DataCooking;
using Microsoft.Performance.SDK.Runtime.Extensibility.DataExtensions;

namespace Microsoft.Performance.SDK.Runtime.Tests.Extensibility.TestClasses
{
    public class TestProcessingSystemCookerData
        : IProcessingSystemCookerData
    {
        private readonly ICookedDataRetrieval sourceCookerData;
        private readonly ICompositeCookerRepository compositeCookers;

        public bool isDisposed;

        public TestProcessingSystemCookerData(ICookedDataRetrieval sourceCookerData, ICompositeCookerRepository compositeCookers)
        {
            this.sourceCookerData = sourceCookerData;
            this.compositeCookers = compositeCookers;
        }

        public ICookedDataRetrieval SourceCookerData
        {
            get
            {
                this.ThrowIfDisposed();
                return this.sourceCookerData;
            }
        }

        public ICookedDataRetrieval GetOrCreateCompositeCooker(DataCookerPath cookerPath, Func<DataCookerPath, IDataExtensionRetrieval> createDataRetrieval)
        {
            this.ThrowIfDisposed();
            return this.compositeCookers.GetOrCreateCompositeCooker(cookerPath, createDataRetrieval);
        }

        public void Dispose()
        {
            this.isDisposed = true;
        }

        public void ThrowIfDisposed()
        {
            if (this.isDisposed)
            {
                throw new ObjectDisposedException(this.GetType().Name);
            }
        }
    }
}
