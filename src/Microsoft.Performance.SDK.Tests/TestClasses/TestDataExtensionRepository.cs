// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.

using System;
using System.Collections.Generic;
using Microsoft.Performance.SDK.Extensibility;
using Microsoft.Performance.SDK.Extensibility.DataCooking;
using Microsoft.Performance.SDK.Runtime.Extensibility.DataExtensions.DataCookers;
using Microsoft.Performance.SDK.Runtime.Extensibility.DataExtensions.Repository;
using Microsoft.Performance.SDK.Runtime.Extensibility.DataExtensions.Tables;

namespace Microsoft.Performance.SDK.Tests.TestClasses
{
    public class TestDataExtensionRepository
        : IDataExtensionRepository
    {
        public HashSet<ISourceDataCookerFactory> sourceDataCookerReferences = new HashSet<ISourceDataCookerFactory>();
        public IReadOnlyCollection<ISourceDataCookerFactory> GetSourceDataCookers(string sourceParserId)
        {
            return sourceDataCookerReferences;
        }

        public ISourceDataCookerFactory GetSourceDataCookerFactory(DataCookerPath dataCookerPath)
        {
            return GetSourceDataCookerReference(dataCookerPath);
        }

        public Dictionary<DataCookerPath, ISourceDataCookerReference> sourceCookersByPath = new Dictionary<DataCookerPath, ISourceDataCookerReference>();
        public Func<DataCookerPath, ISourceDataCookerReference> getSourceDataCooker;
        public ISourceDataCookerReference GetSourceDataCookerReference(DataCookerPath dataCookerPath)
        {
            if (sourceCookersByPath.TryGetValue(dataCookerPath, out var sourceCookerReference))
            {
                return sourceCookerReference;
            }

            return getSourceDataCooker?.Invoke(dataCookerPath);
        }

        public Dictionary<DataCookerPath, ICompositeDataCookerReference> compositeCookersByPath
            = new Dictionary<DataCookerPath, ICompositeDataCookerReference>();
        public Func<DataCookerPath, ICompositeDataCookerReference> getCompositeDataCooker;
        public ICompositeDataCookerReference GetCompositeDataCookerReference(DataCookerPath dataCookerPath)
        {
            if (compositeCookersByPath.TryGetValue(dataCookerPath, out var cookerReference))
            {
                return cookerReference;
            }

            if (getCompositeDataCooker != null)
            {
                return getCompositeDataCooker.Invoke(dataCookerPath);
            }

            return null;
        }

        public Dictionary<Guid, ITableExtensionReference> tablesById = new Dictionary<Guid, ITableExtensionReference>();
        public IReadOnlyDictionary<Guid, ITableExtensionReference> TablesById => tablesById;

        public IEnumerable<DataCookerPath> SourceDataCookers => throw new NotImplementedException();

        public IEnumerable<DataCookerPath> CompositeDataCookers => throw new NotImplementedException();

        public IEnumerable<DataProcessorId> DataProcessors => throw new NotImplementedException();

        // TODO: __SDK_DP__
        // Redesign Data Processor API
        //// public Dictionary<DataProcessorId, IDataProcessorReference> dataProcessorsById = new Dictionary<DataProcessorId, IDataProcessorReference>();
        //// public Func<DataProcessorId, IDataProcessorReference> getDataProcessor;
        //// public IDataProcessorReference GetDataProcessorReference(DataProcessorId dataProcessorId)
        //// {
        ////     if (dataProcessorsById.TryGetValue(dataProcessorId, out var processorReference))
        ////     {
        ////         return processorReference;
        ////     }

        ////     return getDataProcessor?.Invoke(dataProcessorId);
        //// }

        public void FinalizeDataExtensions()
        {
            foreach (var dataCookerReference in sourceCookersByPath)
            {
                dataCookerReference.Value.ProcessDependencies(this);
            }

            foreach (var dataCookerReference in compositeCookersByPath)
            {
                dataCookerReference.Value.ProcessDependencies(this);
            }

            // TODO: __SDK_DP__
            // Redesign Data Processor API
            //// foreach (var dataProcessorReference in dataProcessorsById)
            //// {
            ////     dataProcessorReference.Value.ProcessDependencies(this);
            //// }

            foreach (var tableReferenceBuilder in TablesById)
            {
                tableReferenceBuilder.Value.ProcessDependencies(this);
            }
        }

        public void Dispose()
        {
        }
    }
}
