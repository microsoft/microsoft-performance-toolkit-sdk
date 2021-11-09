// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.

using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using Microsoft.Performance.SDK.Extensibility;
using Microsoft.Performance.SDK.Extensibility.Exceptions;
using Microsoft.Performance.SDK.Processing;
using Microsoft.Performance.SDK.Runtime.Extensibility.DataExtensions;
using Microsoft.Performance.SDK.Runtime.Extensibility.DataExtensions.Repository;
using Microsoft.Performance.SDK.Runtime.Extensibility.DataExtensions.Tables;

namespace Microsoft.Performance.SDK.Runtime.Extensibility
{
    /// <summary>
    ///     This class implements IDataProcessorExtensibilitySupport.
    /// </summary>
    public class CustomDataProcessorExtensibilitySupport
        : IDataProcessorExtensibilitySupport
    {
        private readonly ICustomDataProcessorWithSourceParser dataProcessor;
        private readonly IDataExtensionRepository dataExtensionRepository;
        private readonly ProcessingSystemCompositeCookers compositeCookers;
        private readonly ConcurrentDictionary<TableDescriptor, ITableExtensionReference> tableReferences;

        private DataExtensionRetrievalFactory dataExtensionRetrievalFactory;
        private bool finalizedData;

        /// <summary>
        ///     This constructor is associated with a given <see cref="ICustomDataProcessorWithSourceParser"/>, provided
        ///     as a parameter. <paramref name="dataExtensionRepository"/> is required to retrieve all available data
        ///     extensions.
        /// </summary>
        /// <param name="dataProcessor">
        ///     The data processor with which this object is associated.
        /// </param>
        /// <param name="dataExtensionRepository">
        ///     Provides access to a set of data extensions.
        /// </param>
        /// <param name="compositeCookers">
        ///     The composite cooker repository for a system of data processors.
        /// </param>
        public CustomDataProcessorExtensibilitySupport(
            ICustomDataProcessorWithSourceParser dataProcessor,
            IDataExtensionRepository dataExtensionRepository,
            ProcessingSystemCompositeCookers compositeCookers)
        {
            this.dataProcessor = dataProcessor;
            this.dataExtensionRepository = dataExtensionRepository;
            this.compositeCookers = compositeCookers;
            this.tableReferences = new ConcurrentDictionary<TableDescriptor, ITableExtensionReference>();
        }

        /// <inheritdoc />
        public void EnableTable(TableDescriptor tableDescriptor)
        {
            lock (this.dataProcessor)
            {
                if (this.finalizedData)
                {
                    throw new InvalidOperationException(
                        $"Tables cannot be enabled after finalizing the {nameof(IDataProcessorExtensibilitySupport)}.");
                }
            }

            if (!tableDescriptor.RequiresDataExtensions())
            {
                return;
            }

            ITableExtensionReference tableReference = TableExtensionReference.CreateReference(
                tableDescriptor,
                tableBuildAction: null,
                tableIsDataAvailableFunc: null);

            if (tableReference.Availability == DataExtensionAvailability.Undetermined)
            {
                tableReference.ProcessDependencies(this.dataExtensionRepository);
            }

            if (tableReference.Availability != DataExtensionAvailability.Available)
            {
                throw new ExtensionTableException($"The table is not available: {tableReference.Availability}");
            }

            // todo: remove this class entirely?
            //if (!tableReference.IsInternalTable)
            //{
            //    EnableRequiredSourceDataCookers(tableReference);
            //    return;
            //}

            // Get all unique source parsers that are required by this table.
            var uniqueParserIds = tableReference.DependencyReferences.RequiredSourceDataCookerPaths
                .Select(p => p.SourceParserId)
                .Distinct(DataCookerPath.EqualityComparer).ToList();

            //// Internal table should only reference data from the source parser to which it belongs, so there should
            //// only be one unique source parser.
            ////
            //if (uniqueParserIds.Count > 1)
            //{
            //    throw new InternalTableReferencesMultipleSourceParsersException(tableDescriptor);
            //}

            if (uniqueParserIds.Any())
            {
                // Make sure that the table's required source parser matches that of the data processor associated with
                // this class.
                var requiredParserId = uniqueParserIds.First();
                if (!DataCookerPath.EqualityComparer.Equals(requiredParserId, this.dataProcessor.SourceParserId))
                {
                    throw new WrongProcessorTableException(
                        $"The internal table {tableReference.TableDescriptor.Type} references source parser " +
                        $"{requiredParserId} and cannot be enabled on {this.dataProcessor.SourceParserId}.");
                }
            }

            this.tableReferences[tableDescriptor] = tableReference;
        }

        /// <inheritdoc />
        public bool TryEnableTable(TableDescriptor tableDescriptor)
        {
            try
            {
                EnableTable(tableDescriptor);
                return true;
            }
            catch (Exception)
            {
            }

            return false;
        }

        /// <inheritdoc />
        public void FinalizeTables()
        {
            lock (this.dataProcessor)
            {
                if (this.finalizedData)
                {
                    throw new InvalidOperationException($"{nameof(FinalizeTables)} may only be called once.");
                }

                this.finalizedData = true;
            }

            foreach (var kvp in this.tableReferences)
            {
                var tableReference = kvp.Value;

                // this should have been checked when enabling the table
                Debug.Assert(tableReference.Availability == DataExtensionAvailability.Available);

                // just double check that we didn't allow any tables with external references in
                foreach (var cookerPath in tableReference.DependencyReferences.RequiredSourceDataCookerPaths)
                {
                    Debug.Assert(StringComparer.Ordinal.Equals(cookerPath.SourceParserId, this.dataProcessor.SourceParserId));
                }
            }

            lock (this.dataProcessor)
            {
                if (this.dataExtensionRetrievalFactory == null)
                {
                    // Data processors can use composite cookers for internal extensions, and those cookers should be
                    // maintained by the compositoe cooker respository associated with this data processor, avoiding
                    // mutiple instances of those composite cookers.
                    //

                    var filteredRetrievalFactory = this.compositeCookers.CreateFilteredRepository();

                    this.dataExtensionRetrievalFactory = new DataExtensionRetrievalFactory(
                        this.dataProcessor,
                        filteredRetrievalFactory,
                        dataExtensionRepository);

                    filteredRetrievalFactory.Initialize(this.dataExtensionRetrievalFactory);
                }
            }
        }

        /// <inheritdoc />
        public IDataExtensionRetrieval GetDataExtensionRetrieval(TableDescriptor tableDescriptor)
        {
            lock (this.dataProcessor)
            {
                if (this.dataExtensionRetrievalFactory == null)
                {
                    throw new InvalidOperationException(
                        $"{nameof(FinalizeTables)} must be called before calling " +
                        $"{nameof(GetDataExtensionRetrieval)}.");
                }
            }

            if (!this.tableReferences.ContainsKey(tableDescriptor))
            {
                return null;
            }

            return this.dataExtensionRetrievalFactory.CreateDataRetrievalForTable(this.tableReferences[tableDescriptor]);
        }

        /// <inheritdoc />
        public ISet<DataCookerPath> GetRequiredSourceDataCookers()
        {
            var sourceCookerPaths = new HashSet<DataCookerPath>();

            lock (this.dataProcessor)
            {
                if (!this.finalizedData)
                {
                    throw new InvalidOperationException(
                        $"{nameof(FinalizeTables)} must be called before calling " +
                        $"{nameof(GetRequiredSourceDataCookers)}.");
                }
            }

            foreach (var kvp in this.tableReferences)
            {
                var tableReference = kvp.Value;

                Debug.Assert(tableReference.DependencyReferences != null);
                foreach (var cookerPath in tableReference.DependencyReferences.RequiredSourceDataCookerPaths)
                {
                    sourceCookerPaths.Add(cookerPath);
                }
            }

            return sourceCookerPaths;
        }

        /// <inheritdoc />
        public IEnumerable<TableDescriptor> GetEnabledInternalTables()
        {
            return this.tableReferences.Keys;
        }

        private void EnableRequiredSourceDataCookers(ITableExtensionReference tableReference)
        {
            Debug.Assert(tableReference != null);

            var cookerFactories = tableReference.DependencyReferences.RequiredSourceDataCookerPaths
                .Where(path => StringComparer.Ordinal.Equals(path.SourceParserId, this.dataProcessor.SourceParserId))
                .Select(path => this.dataExtensionRepository.GetSourceDataCookerFactory(path));

            foreach (var cookerFactory in cookerFactories)
            {
                this.dataProcessor.EnableCooker(cookerFactory);
            }
        }
    }
}
