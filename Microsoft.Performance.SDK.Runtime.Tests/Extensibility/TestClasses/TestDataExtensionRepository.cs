// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.

using System;
using System.Collections.Generic;
using Microsoft.Performance.SDK.Extensibility;
using Microsoft.Performance.SDK.Extensibility.DataCooking;
using Microsoft.Performance.SDK.Runtime.Extensibility.DataExtensions.DataCookers;
using Microsoft.Performance.SDK.Runtime.Extensibility.DataExtensions.DataProcessors;
using Microsoft.Performance.SDK.Runtime.Extensibility.DataExtensions.Repository;
using Microsoft.Performance.SDK.Runtime.Extensibility.DataExtensions.Tables;

namespace Microsoft.Performance.SDK.Runtime.Tests.Extensibility.TestClasses
{
    public class TestDataExtensionRepository
        : IDataExtensionRepository
    {
        public HashSet<ISourceDataCookerFactory> sourceDataCookerReferences = new HashSet<ISourceDataCookerFactory>();
        public IReadOnlyCollection<ISourceDataCookerFactory> GetSourceDataCookers(string sourceParserId)
        {
            return this.sourceDataCookerReferences;
        }

        public ISourceDataCookerFactory GetSourceDataCookerFactory(DataCookerPath dataCookerPath)
        {
            return this.GetSourceDataCookerReference(dataCookerPath);
        }

        public Dictionary<DataCookerPath, ISourceDataCookerReference> sourceCookersByPath = new Dictionary<DataCookerPath, ISourceDataCookerReference>();
        public Func<DataCookerPath, ISourceDataCookerReference> getSourceDataCooker;
        public ISourceDataCookerReference GetSourceDataCookerReference(DataCookerPath dataCookerPath)
        {
            if (this.sourceCookersByPath.TryGetValue(dataCookerPath, out var sourceCookerReference))
            {
                return sourceCookerReference;
            }

            return this.getSourceDataCooker?.Invoke(dataCookerPath);
        }

        public Dictionary<DataCookerPath, ICompositeDataCookerReference> compositeCookersByPath 
            = new Dictionary<DataCookerPath, ICompositeDataCookerReference>();
        public Func<DataCookerPath, ICompositeDataCookerReference> getCompositeDataCooker;
        public ICompositeDataCookerReference GetCompositeDataCookerReference(DataCookerPath dataCookerPath)
        {
            if (this.compositeCookersByPath.TryGetValue(dataCookerPath, out var cookerReference))
            {
                return cookerReference;
            }

            if (this.getCompositeDataCooker != null)
            {
                return this.getCompositeDataCooker.Invoke(dataCookerPath);
            }

            return null;
        }

        public Dictionary<Guid, ITableExtensionReference> tablesById = new Dictionary<Guid, ITableExtensionReference>();
        public IReadOnlyDictionary<Guid, ITableExtensionReference> TablesById => this.tablesById;

        public IEnumerable<DataCookerPath> SourceDataCookers => throw new NotImplementedException();

        public IEnumerable<DataCookerPath> CompositeDataCookers => throw new NotImplementedException();

        public IEnumerable<DataProcessorId> DataProcessors => throw new NotImplementedException();

        public Dictionary<DataProcessorId, IDataProcessorReference> dataProcessorsById = new Dictionary<DataProcessorId, IDataProcessorReference>();
        public Func<DataProcessorId, IDataProcessorReference> getDataProcessor;
        public IDataProcessorReference GetDataProcessorReference(DataProcessorId dataProcessorId)
        {
            if (this.dataProcessorsById.TryGetValue(dataProcessorId, out var processorReference))
            {
                return processorReference;
            }

            return this.getDataProcessor?.Invoke(dataProcessorId);
        }

        public void FinalizeDataExtensions()
        {
            foreach (var dataCookerReference in this.sourceCookersByPath)
            {
                dataCookerReference.Value.ProcessDependencies(this);
            }

            foreach (var dataCookerReference in this.compositeCookersByPath)
            {
                dataCookerReference.Value.ProcessDependencies(this);
            }

            foreach (var dataProcessorReference in this.dataProcessorsById)
            {
                dataProcessorReference.Value.ProcessDependencies(this);
            }

            foreach (var tableReferenceBuilder in this.TablesById)
            {
                tableReferenceBuilder.Value.ProcessDependencies(this);
            }
        }
    }
}
