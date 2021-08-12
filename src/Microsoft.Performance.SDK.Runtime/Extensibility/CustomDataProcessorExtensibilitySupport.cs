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
            // todo: this doesn't prevent somone from calling FinalizeTables on another thread while AddTable is running.
            lock (this.dataProcessor)
            {
                if (this.dataExtensionRetrievalFactory != null)
                {
                    throw new InvalidOperationException($"Cannot add tables after calling {nameof(FinalizeTables)}.");
                }
            }

            if (tableDescriptor.RequiredDataCookers.Any() || tableDescriptor.RequiredDataProcessors.Any())
            {
                if (TableExtensionReference.TryCreateReference(tableDescriptor.Type, true, out var tableReference))
                {
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

            // When a plugin has multiple source parsers, then it's possible that the list of table references has some
            // tables associated with a source parser other than the one associated with the data source that was
            // opened. so just remove these tables and proceed - this is not an error case
            //
            var tablesToRemove = new List<TableDescriptor>();

            foreach (var kvp in this.tableReferences)
            {
                var tableReference = kvp.Value;

                tableReference.ProcessDependencies(this.dataExtensionRepository);

                if (tableReference.Availability != DataExtensionAvailability.Available)
                {
                    tablesToRemove.Add(kvp.Key);
                    continue;
                }

                // check that there are no external sources required
                foreach (var cookerPath in tableReference.DependencyReferences.RequiredSourceDataCookerPaths)
                {
                    if (!StringComparer.Ordinal.Equals(cookerPath.SourceParserId, this.dataProcessor.SourceParserId))
                    {
                        tablesToRemove.Add(kvp.Key);
                        break;
                    }

                    Debug.Assert(!string.IsNullOrWhiteSpace(cookerPath.SourceParserId));
                }
            }

            foreach (var tableDescriptor in tablesToRemove)
            {
                this.tableReferences.Remove(tableDescriptor);
            }

            //var tablesToProcessor = new Dictionary<TableDescriptor, ICustomDataProcessor>();
            //foreach (var tableDescriptor in this.tableReferences.Keys)
            //{
            //    tablesToProcessor.Add(tableDescriptor, this.dataProcessor);
            //}

            lock (this.dataProcessor)
            {
                if (this.dataExtensionRetrievalFactory == null)
                {
                    var tablesToProcessor = new Dictionary<TableDescriptor, ICustomDataProcessor>();

                    // Data processors can use composite cookers for internal extensions, and those cookers should come
                    // from the pool of cookers in the system of processors to which this processor belongs. this is why
                    // we've passed down the ProcessingSystemCompositeCookers.
                    //
                    var cookerData = new ProcessingSystemCookerData(this.dataProcessor, this.compositeCookers, false);
                    this.dataExtensionRetrievalFactory = new DataExtensionRetrievalFactory(cookerData, dataExtensionRepository);
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
    }
}
