// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.

using Microsoft.Performance.SDK.Extensibility.DataCooking;
using Microsoft.Performance.SDK.Runtime.Extensibility.DataExtensions.Repository;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Diagnostics;

namespace Microsoft.Performance.SDK.Runtime.Extensibility.DataExtensions.Tables
{
    /// <summary>
    ///     This is a small helper class to aid in table selection and enabling the source data cookers
    ///     required by those tables.
    /// </summary>
    public class TableExtensionSelector
    {
        private readonly IDataExtensionRepository dataExtensions;

        /// <summary>
        ///  Constructor
        /// </summary>
        /// <param name="dataExtensions">
        ///     Repository of data extensions.
        /// </param>
        public TableExtensionSelector(IDataExtensionRepository dataExtensions)
        {
            Guard.NotNull(dataExtensions, nameof(dataExtensions));

            this.dataExtensions = dataExtensions;

            var availableTables = new Dictionary<Guid, ITableExtensionReference>();
            var availableCategories = new HashSet<string>(StringComparer.Ordinal);

            foreach (var kvp in dataExtensions.TablesById)
            {
                if (kvp.Value.Availability == DataExtensionAvailability.Available)
                {
                    availableTables.Add(kvp.Key, kvp.Value);
                    availableCategories.Add(kvp.Value.TableDescriptor.Category);
                }
            }

            this.Tables = new ReadOnlyDictionary<Guid, ITableExtensionReference>(availableTables);
            this.TableCategories = availableCategories;
        }

        /// <summary>
        ///     Gets all available table extensions indexed by the table Id.
        /// </summary>
        public IReadOnlyDictionary<Guid, ITableExtensionReference> Tables { get; }

        /// <summary>
        ///     Gets all categories represented by the available Tables.
        /// </summary>
        public IReadOnlyCollection<string> TableCategories { get; }

        /// <summary>
        ///     Given a set of table Ids, this will determine all of the required source data cookers required to
        ///     enable those tables.
        /// </summary>
        /// <param name="tableIds">
        ///     A set of table Ids.
        /// </param>
        /// <returns>
        ///     The required source data cookers necessary for the provided tables.
        /// </returns>
        public IDictionary<string, HashSet<ISourceDataCookerFactory>> GetSourceDataCookersForTables(
            IEnumerable<Guid> tableIds)
        {
            var referencesBySource = new Dictionary<string, HashSet<ISourceDataCookerFactory>>(StringComparer.Ordinal);

            if (tableIds == null)
            {
                return referencesBySource;
            }

            foreach (var tableId in tableIds)
            {
                if (!this.dataExtensions.TablesById.TryGetValue(tableId, out var tableReference))
                {
                    continue;
                }

                if (tableReference.Availability != DataExtensionAvailability.Available)
                {
                    throw new InvalidOperationException("Only available tables may be enabled.");
                }

                AddTableSourceCookers(tableReference, referencesBySource);
            }

            return referencesBySource;
        }

        /// <summary>
        ///     Given a set of table references, this will determine all of the required source data cookers required to
        ///     enable those tables.
        /// </summary>
        /// <param name="tables">
        ///     A set of table references.
        /// </param>
        /// <returns>
        ///     The required source data cookers necessary for the provided tables.
        /// </returns>
        public IDictionary<string, HashSet<ISourceDataCookerFactory>> GetSourceDataCookersForTables(
            IEnumerable<ITableExtensionReference> tables)
        {
            var referencesBySource = new Dictionary<string, HashSet<ISourceDataCookerFactory>>(StringComparer.Ordinal);

            if (tables == null)
            {
                return referencesBySource;
            }

            foreach (var tableReference in tables)
            {
                if (tableReference.Availability != DataExtensionAvailability.Available)
                {
                    throw new InvalidOperationException("Only available tables may be enabled.");
                }

                AddTableSourceCookers(tableReference, referencesBySource);
            }

            return referencesBySource;
        }

        private void AddTableSourceCookers(
            ITableExtensionReference tableReference,
            IDictionary<string, HashSet<ISourceDataCookerFactory>> referencesBySource)
        {
            Guard.NotNull(tableReference, nameof(tableReference));
            Guard.NotNull(referencesBySource, nameof(referencesBySource));

            if (tableReference.Availability != DataExtensionAvailability.Available)
            {
                // if a table is selected, it came from the available tables list which were all
                // confirmed available when the list was populated. how did it change?
                Debug.Assert(false);
                return;
            }

            var requiredDataCookers = tableReference.DependencyReferences.RequiredSourceDataCookerPaths;
            foreach (var dataCookerPath in requiredDataCookers)
            {
                var dataCookerReference = this.dataExtensions.GetSourceDataCookerReference(dataCookerPath);
                if (dataCookerReference == null)
                {
                    // we're only operating on tables that are marked as available.
                    // this means that all required resources are also available in the data extension repository
                    //
                    Debug.Assert(false);
                    continue;
                }

                if (string.IsNullOrWhiteSpace(dataCookerReference.Path.SourceParserId))
                {
                    // we just pulled this from the RequiredSourceDataCookerPaths, it should have no
                    // data cookers without a valid source Id
                    //
                    Debug.Assert(false);
                    continue;
                }

                if (!referencesBySource.ContainsKey(dataCookerReference.Path.SourceParserId))
                {
                    referencesBySource.Add(dataCookerReference.Path.SourceParserId, new HashSet<ISourceDataCookerFactory>());
                }

                referencesBySource[dataCookerReference.Path.SourceParserId].Add(dataCookerReference);
            }
        }
    }
}
