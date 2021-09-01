// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.

using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using Microsoft.Performance.SDK.Extensibility;
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
        private readonly Dictionary<TableDescriptor, ITableExtensionReference> tableReferences;

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
            this.tableReferences = new Dictionary<TableDescriptor, ITableExtensionReference>();
        }

        /// <inheritdoc />
        public bool AddTable(TableDescriptor tableDescriptor)
        {
            lock (this.dataProcessor)
            {
                if (this.finalizedData)
                {
                    return false;
                }
            }

            if (tableDescriptor.RequiredDataCookers.Any() || tableDescriptor.RequiredDataProcessors.Any())
            {
                if (TableExtensionReference.TryCreateReference(tableDescriptor.Type, true, out var tableReference))
                {
                    if (!tableReference.IsInternalTable)
                    {
                        return EnableRequiredSourceDataCookers(tableReference);
                    }

                    tableReference.ProcessDependencies(this.dataExtensionRepository);

                    if (tableReference.Availability != DataExtensionAvailability.Available)
                    {
                        return false;
                    }

                    // check that there are no external sources required.
                    foreach (var cookerPath in tableReference.DependencyReferences.RequiredSourceDataCookerPaths)
                    {
                        if (!StringComparer.Ordinal.Equals(cookerPath.SourceParserId, this.dataProcessor.SourceParserId))
                        {
                            return false;
                        }
                    }

                    this.tableReferences[tableDescriptor] = tableReference;
                    return true;
                }
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

            var tablesToRemove = new List<TableDescriptor>();

            foreach (var kvp in this.tableReferences)
            {
                var tableReference = kvp.Value;

                if (tableReference.Availability == DataExtensionAvailability.Undetermined)
                {
                    tableReference.ProcessDependencies(this.dataExtensionRepository);
                }

                if (tableReference.Availability != DataExtensionAvailability.Available)
                {
                    tablesToRemove.Add(kvp.Key);
                    continue;
                }

                // just double check that we didn't allow any tables with external references in
                foreach (var cookerPath in tableReference.DependencyReferences.RequiredSourceDataCookerPaths)
                {
                    Debug.Assert(StringComparer.Ordinal.Equals(cookerPath.SourceParserId, this.dataProcessor.SourceParserId));
                }
            }

            foreach (var tableDescriptor in tablesToRemove)
            {
                this.tableReferences.Remove(tableDescriptor);
            }

            lock (this.dataProcessor)
            {
                if (this.dataExtensionRetrievalFactory == null)
                {
                    // Data processors can use composite cookers for internal extensions, and those cookers should come
                    // from the pool of cookers in the system of processors to which this processor belongs. This is why
                    // we've passed down the for the given processing system. Otherwise there might be mutiple instances
                    // of composite cookers, each having processed the same data - a waste of memory and processing time.
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
        public ISet<DataCookerPath> GetAllRequiredSourceDataCookers()
        {
            var sourceCookerPaths = new HashSet<DataCookerPath>();

            lock (this.dataProcessor)
            {
                if (!this.finalizedData)
                {
                    throw new InvalidOperationException(
                        $"{nameof(FinalizeTables)} must be called before calling " +
                        $"{nameof(GetAllRequiredSourceDataCookers)}.");
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
        public IEnumerable<TableDescriptor> GetAllRequiredTables()
        {
            return this.tableReferences.Keys;
        }

        /// <inheritdoc />
        private bool EnableRequiredSourceDataCookers(ITableExtensionReference tableReference)
        {
            //Guard.NotNull(tableDescriptor, nameof(tableDescriptor));

            //lock (this.dataProcessor)
            //{
            //    if (this.finalizedData)
            //    {
            //        throw new InvalidOperationException(
            //            "Unable to enable data extensions after a data processor has been finalized.");
            //    }
            //}

            //bool result = TableExtensionReference.CreateReference(tableDescriptor.Type, true, out var tableReference);

            // because we already have the table descriptor available, we expect that this will either succeed or throw
            //Debug.Assert(result);
            //Debug.Assert(tableReference != null);

            if (tableReference.Availability == DataExtensionAvailability.Undetermined)
            {
                tableReference.ProcessDependencies(this.dataExtensionRepository);
            }

            if (tableReference.Availability != DataExtensionAvailability.Available)
            {
                return false;
                //throw new ExtendedTableException($"The table is not available: {tableReference.Availability}");
            }

            var cookerFactories = tableReference.DependencyReferences.RequiredSourceDataCookerPaths
                .Where(path => StringComparer.Ordinal.Equals(path.SourceParserId, this.dataProcessor.SourceParserId))
                .Select(path => this.dataExtensionRepository.GetSourceDataCookerFactory(path));

            foreach (var cookerFactory in cookerFactories)
            {
                this.dataProcessor.EnableCooker(cookerFactory);
            }

            return true;
        }
    }
}
