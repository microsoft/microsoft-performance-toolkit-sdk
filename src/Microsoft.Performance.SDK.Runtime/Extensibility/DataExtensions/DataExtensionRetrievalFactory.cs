// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.

using System;
using System.Diagnostics;
using Microsoft.Performance.SDK.Extensibility;
using Microsoft.Performance.SDK.Extensibility.DataCooking;
using Microsoft.Performance.SDK.Runtime.Extensibility.DataExtensions.Repository;
using Microsoft.Performance.SDK.Runtime.Extensibility.DataExtensions.Tables;

namespace Microsoft.Performance.SDK.Runtime.Extensibility.DataExtensions
{
    /// <summary>
    ///     All data extension objects, other than a source data cooker, require a set of data to pull
    ///     from. We need a way to restrict the data provided to the given data extension to just the data
    ///     coming from the data extensions it marked as required.
    ///
    ///     The purpose of this class is to generate that unique data set for a given data extension.
    ///
    ///     This restriction provides us the ability to quickly determine when a data extension is missing
    ///     a requirement, as well as to programatically enable a set of data extension from a list of tables,
    ///     so it is extremely important and full of general happiness.
    /// </summary>
    public class DataExtensionRetrievalFactory
        : IDataExtensionRetrievalFactory
    {
        private readonly DataRetrievalCache dataRetrievalCache = new DataRetrievalCache();

        /// <summary>
        ///     Creates an instance.
        /// </summary>
        /// <param name="sourceCookerData">
        ///     Provides access to source data cooker output.
        /// </param>
        /// <param name="dataExtensionRepository">
        ///     Provides a way to generate data extensions other than source data cookers.
        /// </param>
        public DataExtensionRetrievalFactory(
            ICookedDataRetrieval sourceCookerData,
            IDataExtensionRepository dataExtensionRepository)
        {
            Guard.NotNull(sourceCookerData, nameof(sourceCookerData));
            Guard.NotNull(dataExtensionRepository, nameof(dataExtensionRepository));

            this.CookedSourceData = sourceCookerData;
            this.DataExtensionRepository = dataExtensionRepository;
        }

        internal ICookedDataRetrieval CookedSourceData { get; }

        internal IDataExtensionRepository DataExtensionRepository { get; }

        /// <summary>
        ///     A composite cooker has access to source data cookers, as well as other composite data cookers.
        /// </summary>
        /// <param name="dataCookerPath">
        ///     Identifies the composite data cooker.
        /// </param>
        /// <returns>
        ///     A set of data uniquely tailored to this composite data cooker.
        /// </returns>
        public IDataExtensionRetrieval CreateDataRetrievalForCompositeDataCooker(
            DataCookerPath dataCookerPath)
        {
            var filteredData = this.dataRetrievalCache.GetCompositeDataCookerFilteredData(dataCookerPath);
            if (filteredData != null)
            {
                return filteredData;
            }

            var compositeDataCookerReference = this.DataExtensionRepository.GetCompositeDataCookerReference(dataCookerPath);

            if (compositeDataCookerReference == null)
            {
                throw new ArgumentException("Data retrieval requested for data cooker not found in repository.");
            }

            if (compositeDataCookerReference.Availability != DataExtensionAvailability.Available)
            {
                throw new ArgumentException("Data retrieval requested for data cooker that is not available.");
            }

            filteredData = new FilteredDataRetrieval(this, compositeDataCookerReference.DependencyReferences);

            this.dataRetrievalCache.AddCompositeDataCookerFilteredData(dataCookerPath, filteredData);

            return filteredData;
        }

        /// <summary>
        ///     A data processor has access to source data cookers, composite data cookers, as well as other
        ///     data processors.
        /// </summary>
        /// <param name="dataProcessorId">
        ///     Identifies the data processor.
        /// </param>
        /// <returns>
        ///     A set of data uniquely tailored to this data processor.
        /// </returns>
        public IDataExtensionRetrieval CreateDataRetrievalForDataProcessor(
            DataProcessorId dataProcessorId)
        {
            var filteredData = this.dataRetrievalCache.GetDataProcessorFilteredData(dataProcessorId);
            if (filteredData != null)
            {
                return filteredData;
            }

            var dataProcessorReference = this.DataExtensionRepository.GetDataProcessorReference(dataProcessorId);

            if (dataProcessorReference == null)
            {
                throw new ArgumentException("Data retrieval requested for data processor not found in repository.");
            }

            if (dataProcessorReference.Availability != DataExtensionAvailability.Available)
            {
                throw new ArgumentException("Data retrieval requested for data processor that is not available.");
            }

            filteredData = new FilteredDataRetrieval(this, dataProcessorReference.DependencyReferences);

            this.dataRetrievalCache.AddDataProcessorFilteredData(dataProcessorId, filteredData);

            return filteredData;
        }

        /// <summary>
        ///     A table has access to source data cookers, composite data cookers, and data processors.
        /// </summary>
        /// <param name="tableId">
        ///     Identifies the table.
        /// </param>
        /// <returns>
        ///     A set of data uniquely tailored to this table.
        /// </returns>
        public IDataExtensionRetrieval CreateDataRetrievalForTable(
            Guid tableId)
        {
            if (tableId == Guid.Empty)
            {
                throw new ArgumentException($"The table Id may not be {nameof(Guid.Empty)}.", nameof(tableId));
            }

            var filteredData = this.dataRetrievalCache.GetTableFilteredData(tableId);
            if (filteredData != null)
            {
                return filteredData;
            }

            if (!this.DataExtensionRepository.TablesById.TryGetValue(tableId, out var tableExtensionReference))
            {
                throw new ArgumentException(
                    $"The table Id reference was not found in the data extension repository.",
                    nameof(tableId));
            }

            Debug.Assert(tableExtensionReference != null);

            filteredData = this.CreateDataRetrievalForTable(tableExtensionReference);

            this.dataRetrievalCache.AddTableFilteredData(tableId, filteredData);

            return filteredData;
        }

        /// <summary>
        ///     A table has access to source data cookers, composite data cookers, and data processors.
        /// </summary>
        /// <param name="tableExtensionReference">
        ///     Reference to a table data extension.
        /// </param>
        /// <returns>
        ///     A set of data uniquely tailored to this table.
        /// </returns>
        public IDataExtensionRetrieval CreateDataRetrievalForTable(
            ITableExtensionReference tableExtensionReference)
        {
            Guard.NotNull(tableExtensionReference, nameof(tableExtensionReference));

            if (tableExtensionReference.Availability != DataExtensionAvailability.Available)
            {
                throw new ArgumentException("Data retrieval requested for table that is not available.");
            }

            var filteredData = new FilteredDataRetrieval(this, tableExtensionReference.DependencyReferences);

            return filteredData;
        }
    }
}
