// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.

using Microsoft.Performance.SDK.Extensibility;
using System;
using System.Collections.Generic;
using System.Diagnostics;

namespace Microsoft.Performance.SDK.Runtime.Extensibility.DataExtensions
{
    /// <summary>
    ///     This class provides a cache for data extension dependencies. Once a required data extension has been
    ///     found and validated, this prevents additional validation on that data extension in future
    ///     lookups.
    /// </summary>
    internal class DataRetrievalCache
    {
        private readonly object compositeCacheLock = new object();
        private readonly object processorCacheLock = new object();
        private readonly object tableCacheLock = new object();

        private readonly Dictionary<DataCookerPath, System.WeakReference<IDataExtensionRetrieval>> compositeDataCookerCache
            = new Dictionary<DataCookerPath, System.WeakReference<IDataExtensionRetrieval>>();

        private readonly Dictionary<DataProcessorId, System.WeakReference<IDataExtensionRetrieval>> dataProcessorCache
            = new Dictionary<DataProcessorId, System.WeakReference<IDataExtensionRetrieval>>();

        private readonly Dictionary<Guid, System.WeakReference<IDataExtensionRetrieval>> tableCache
            = new Dictionary<Guid, System.WeakReference<IDataExtensionRetrieval>>();

        internal IDataExtensionRetrieval GetCompositeDataCookerFilteredData(DataCookerPath dataCookerPath)
        {
            Debug.Assert(dataCookerPath != null, nameof(dataCookerPath));

            lock (this.compositeCacheLock)
            {
                if (this.compositeDataCookerCache.TryGetValue(dataCookerPath, out var filteredDataReference))
                {
                    if (filteredDataReference.TryGetTarget(out var filteredData))
                    {
                        return filteredData;
                    }
                }
            }

            return null;
        }

        internal void AddCompositeDataCookerFilteredData(DataCookerPath dataCookerPath, IDataExtensionRetrieval data)
        {
            Debug.Assert(data != null, nameof(data));

            lock (this.compositeCacheLock)
            {
                this.compositeDataCookerCache[dataCookerPath] = new System.WeakReference<IDataExtensionRetrieval>(data);
            }
        }

        internal IDataExtensionRetrieval GetDataProcessorFilteredData(DataProcessorId dataProcessorId)
        {
            lock (this.processorCacheLock)
            {
                if (this.dataProcessorCache.TryGetValue(dataProcessorId, out var filteredDataReference))
                {
                    if (filteredDataReference.TryGetTarget(out var filteredData))
                    {
                        return filteredData;
                    }
                }
            }

            return null;
        }

        internal void AddDataProcessorFilteredData(DataProcessorId dataProcessorId, IDataExtensionRetrieval data)
        {
            Debug.Assert(data != null, nameof(data));

            lock (this.processorCacheLock)
            {
                this.dataProcessorCache[dataProcessorId] = new System.WeakReference<IDataExtensionRetrieval>(data);
            }
        }

        internal IDataExtensionRetrieval GetTableFilteredData(Guid tableId)
        {
            Debug.Assert(tableId != Guid.Empty);

            lock (this.tableCacheLock)
            {
                if (this.tableCache.TryGetValue(tableId, out var filteredDataReference))
                {
                    if (filteredDataReference.TryGetTarget(out var filteredData))
                    {
                        return filteredData;
                    }
                }
            }

            return null;
        }

        internal void AddTableFilteredData(Guid tableId, IDataExtensionRetrieval data)
        {
            Debug.Assert(tableId != Guid.Empty);
            Debug.Assert(data != null, nameof(data));

            lock (this.tableCacheLock)
            {
                this.tableCache[tableId] = new System.WeakReference<IDataExtensionRetrieval>(data);
            }
        }
    }
}
