// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.

using System;
using System.Collections.Generic;
using System.Linq;
using Microsoft.Performance.SDK;
using Microsoft.Performance.SDK.Extensibility;
using Microsoft.Performance.SDK.Extensibility.DataCooking;
using Microsoft.Performance.SDK.Runtime.Extensibility.DataExtensions.Repository;

namespace Microsoft.Performance.Toolkit.Engine
{
    /// <summary>
    ///     Represents the results of processing data using the <see cref="Engine"/>.
    /// </summary>
    public sealed class RuntimeExecutionResults
    {
        private readonly ICookedDataRetrieval cookedDataRetrieval;
        private readonly IDataExtensionRetrievalFactory retrievalFactory;
        private readonly IDataExtensionRepository repository;
        private readonly IEnumerable<DataCookerPath> sourceCookers;

        /// <summary>
        ///     Initializes a new instance of the <see cref="RuntimeExecutionResults"/>
        ///     class.
        /// </summary>
        /// <param name="cookedDataRetrieval">
        ///     The retrieval interface for getting to cooked data.
        /// </param>
        /// <param name="retrievalFactory">
        ///     The factory for creating retrievals for composite cookers.
        /// </param>
        /// <param name="repository">
        ///     The repository that was used to process the data.
        /// </param>
        /// <exception cref="ArgumentNullException">
        ///     <paramref name="cookedDataRetrieval"/> is <c>null</c>.
        ///     - or -
        ///     <paramref name="retrievalFactory"/> is <c>null</c>.
        ///     - or -
        ///     <paramref name="repository"/> is <c>null</c>.
        /// </exception>
        public RuntimeExecutionResults(
            ICookedDataRetrieval cookedDataRetrieval,
            IDataExtensionRetrievalFactory retrievalFactory,
            IDataExtensionRepository repository)
        {
            Guard.NotNull(cookedDataRetrieval, nameof(cookedDataRetrieval));
            Guard.NotNull(retrievalFactory, nameof(retrievalFactory));
            Guard.NotNull(repository, nameof(repository));

            this.cookedDataRetrieval = cookedDataRetrieval;
            this.retrievalFactory = retrievalFactory;
            this.repository = repository;
            this.sourceCookers = new HashSet<DataCookerPath>(this.repository.SourceDataCookers);
        }

        /// <summary>
        ///     Gets the direct cooker retrieval for the specified
        ///     cooker.
        /// </summary>
        /// <param name="cookerPath">
        ///     The path to the cooker to retrieve.
        /// </param>
        /// <returns>
        ///     The interface to query for cooked data from said cooker.
        /// </returns>
        /// <exception cref="CookerNotFoundException">
        ///     <paramref name="cookerPath"/> does not represent a known cooker.
        /// </exception>
        public ICookedDataRetrieval GetCookedData(DataCookerPath cookerPath)
        {
            if (this.sourceCookers.Contains(cookerPath))
            {
                return this.cookedDataRetrieval;
            }

            try
            {
                var cooker = this.repository.GetCompositeDataCookerReference(cookerPath);
                var retrieval = this.retrievalFactory.CreateDataRetrievalForCompositeDataCooker(cookerPath);
                return cooker.GetOrCreateInstance(retrieval);
            }
            catch (Exception e)
            {
                throw new CookerNotFoundException(cookerPath, e);
            }
        }

        /// <summary>
        ///     Attempts to get the direct cooker retrieval for the specified
        ///     cooker.
        /// </summary>
        /// <param name="cookerPath">
        ///     The path to the cooker to retrieve.
        /// </param>
        /// <param name="retrieval">
        ///     The found retrieval, if any.
        /// </param>
        /// <returns>
        ///     <c>true</c> if the cooker can be queried;
        ///     <c>false</c> otherwise.
        /// </returns>
        public bool TryGetCookedData(DataCookerPath cookerPath, out ICookedDataRetrieval retrieval)
        {
            try
            {
                retrieval = this.GetCookedData(cookerPath);
                return true;
            }
            catch
            {
                retrieval = null;
                return false;
            }
        }

        /// <summary>
        ///     Queries for processed data from the given path.
        /// </summary>
        /// <typeparam name="T">
        ///     The <see cref="Type"/> of data being queried.
        /// </typeparam>
        /// <param name="dataOutputPath">
        ///     The fully qualified output path to the data to retrieve.
        /// </param>
        /// <returns>
        ///     The cooked data.
        /// </returns>
        /// <exception cref="DataOutputNotFoundException">
        ///     No processed data could be found for the given <paramref name="dataOutputPath"/>.
        /// </exception>
        public T QueryOutput<T>(DataOutputPath dataOutputPath)
        {
            return (T)this.QueryOutput(dataOutputPath);
        }

        /// <summary>
        ///     Queries for processed data from the given path.
        /// </summary>
        /// <param name="dataOutputPath">
        ///     The fully qualified output path to the data to retrieve.
        /// </param>
        /// <returns>
        ///     The cooked data.
        /// </returns>
        /// <exception cref="DataOutputNotFoundException">
        ///     No processed data could be found for the given <paramref name="dataOutputPath"/>.
        /// </exception>
        public object QueryOutput(DataOutputPath dataOutputPath)
        {
            try
            {
                var cooker = this.GetCookedData(dataOutputPath.CookerPath);
                return cooker.QueryOutput(dataOutputPath);
            }
            catch (Exception e)
            {
                throw new DataOutputNotFoundException(dataOutputPath, e);
            }
        }

        /// <summary>
        ///     Attempts to query for processed data from the given path.
        /// </summary>
        /// <param name="dataOutputPath">
        ///     The fully qualified output path to the data to retrieve.
        /// </param>
        /// <param name="data">
        ///     The retrieved data, if any.
        /// </param>
        /// <returns>
        ///     <c>true</c> if data at the given path was found;
        ///     <c>false</c> otherwise.
        /// </returns>
        public bool TryQueryOutput(DataOutputPath dataOutputPath, out object data)
        {
            try
            {
                data = this.QueryOutput(dataOutputPath);
                return true;
            }
            catch
            {
                data = null;
                return false;
            }
        }

        /// <summary>
        ///     Attempts to query for processed data from the given path.
        /// </summary>
        /// <typeparam name="T">
        ///     The <see cref="Type"/> of data being queried.
        /// </typeparam>
        /// <param name="dataOutputPath">
        ///     The fully qualified output path to the data to retrieve.
        /// </param>
        /// <param name="data">
        ///     The retrieved data, if any.
        /// </param>
        /// <returns>
        ///     <c>true</c> if data at the given path was found;
        ///     <c>false</c> otherwise.
        /// </returns>
        public bool TryQueryOutput<T>(DataOutputPath dataOutputPath, out T data)
        {
            if (this.TryQueryOutput(dataOutputPath, out object dataRaw))
            {
                data = (T)dataRaw;
                return true;
            }

            data = default(T);
            return false;
        }
    }
}
