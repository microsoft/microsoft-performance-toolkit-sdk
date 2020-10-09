// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.

using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using Microsoft.Performance.SDK.Extensibility;
using Microsoft.Performance.SDK.Processing;
using Microsoft.Performance.SDK.Runtime.Extensibility.DataExtensions.DataCookers;
using Microsoft.Performance.SDK.Runtime.Extensibility.DataExtensions.Tables;

namespace Microsoft.Performance.SDK.Runtime.Extensibility.DataExtensions.Repository
{
    /// <summary>
    ///     Exposes extensions methods to IDataExtensionRepository.
    /// </summary>
    public static class DataExtensionRepositoryExtensions
    {
        /// <summary>
        ///     Attempts to get a data cooker reference given a data cooker path.
        /// </summary>
        /// <param name="self">
        ///     A data extension repository to search.
        /// </param>
        /// <param name="dataCookerPath">
        ///     A path to a data cooker.
        /// </param>
        /// <param name="cooker">
        ///     Receives the result: a data cooker reference, or <c>null</c> if not found.
        /// </param>
        /// <returns>
        ///     true if the path resulted in a data cooker reference; false otherwise.
        /// </returns>
        public static bool TryGetDataCookerReference(
            this IDataExtensionRepository self,
            DataCookerPath dataCookerPath,
            out IDataCookerReference cooker)
        {
            Guard.NotNull(self, nameof(self));

            cooker = self.GetSourceDataCookerReference(dataCookerPath);
            if (cooker is null)
            {
                cooker = self.GetCompositeDataCookerReference(dataCookerPath);
            }

            return !(cooker is null);
        }

        /// <summary>
        ///     Gets a set of table extensions references given a set of table extension identifiers and a set of
        ///     available source processors.
        /// </summary>
        /// <param name="self">
        ///     The data extension repository.
        /// </param>
        /// <param name="selectedTables">
        ///     Table identifiers that the caller is interested in,
        ///     or <c>null</c> if interested in all available tables.
        /// </param>
        /// <param name="processorsWithParsers">
        ///     The source processors that are available to generate data.
        /// </param>
        /// <returns>
        ///     A set of table extensions, limited to those whose ID was specified and where required source processors
        ///     are available.
        /// </returns>
        public static HashSet<ITableExtensionReference> GetEnabledTableExtensionReferences(
            this IDataExtensionRepository self,
            ISet<Guid> selectedTables,
            IEnumerable<ICustomDataProcessorWithSourceParser> processorsWithParsers)
        {
            //
            // selectedTables being null means everything is enabled
            //

            Guard.NotNull(self, nameof(self));
            Guard.NotNull(processorsWithParsers, nameof(processorsWithParsers));

            var sourceParserIds = 
                processorsWithParsers.Select(p => p.SourceParserId);
            var uniqueSourceParserIds = new HashSet<string>(sourceParserIds, StringComparer.Ordinal);

            bool SourceParsersAvailable(ITableExtensionReference table)
            {
                foreach (var requiredSource in table.DependencyReferences.RequiredSourceDataCookerPaths)
                {
                    if (!uniqueSourceParserIds.Contains(requiredSource.SourceParserId))
                    {
                        return false;
                    }
                }

                return true;
            }

            var enabledTables = new HashSet<ITableExtensionReference>();
            if (selectedTables != null)
            {
                foreach (var tableId in selectedTables)
                {
                    if (self.TablesById.TryGetValue(tableId, out var tableReference))
                    {
                        Debug.Assert(tableReference.Availability == DataExtensionAvailability.Available);
                        if (SourceParsersAvailable(tableReference))
                        {
                            enabledTables.Add(tableReference);
                        }
                    }
                }
            }
            else
            {
                foreach (var tableReference in self.TablesById.Values)
                {
                    if (tableReference.Availability == DataExtensionAvailability.Available)
                    {
                        if (SourceParsersAvailable(tableReference))
                        {
                            enabledTables.Add(tableReference);
                        }
                    }
                }
            }

            return enabledTables;
        }

        /// <summary>
        ///     This enables all required source data cookers for each specified table. Doing so will ensure that the
        ///     table has the required data to build.
        /// </summary>
        /// <param name="self">
        ///     The data extension repository.
        /// </param>
        /// <param name="processors">
        ///     The source processors that are available to generate data.
        /// </param>
        /// <param name="enabledTables">
        ///     Table references that the caller is interested in building.
        /// </param>
        /// <returns>
        ///     The set of custom data processors that have data cookers enabled in order to build the requested set
        ///     of tables.
        /// </returns>
        public static ISet<ICustomDataProcessorWithSourceParser> EnableSourceDataCookersForTables(
            this IDataExtensionRepository self,
            IEnumerable<ICustomDataProcessorWithSourceParser> processors,
            HashSet<ITableExtensionReference> enabledTables)
        {
            Guard.NotNull(self, nameof(self));
            Guard.NotNull(processors, nameof(processors));
            Guard.NotNull(enabledTables, nameof(enabledTables));

            var customProcessors = new HashSet<ICustomDataProcessorWithSourceParser>();

            var availableTableExtensions = new TableExtensionSelector(self);
            var requiredReferencesBySourceId = availableTableExtensions.GetSourceDataCookersForTables(enabledTables);

            foreach (var processor in processors)
            {
                if (requiredReferencesBySourceId.TryGetValue(
                    processor.SourceParserId,
                    out var requiredCookerReferences))
                {
                    if (!requiredCookerReferences.Any())
                    {
                        continue;
                    }

                    customProcessors.Add(processor);

                    foreach (var cookerReference in requiredCookerReferences)
                    {
                        processor.EnableCooker(cookerReference);
                    }
                }
            }

            return customProcessors;
        }

        /// <summary>
        ///     Enables a set of data cookers to participate in source processing.
        /// </summary>
        /// <param name="self">
        ///     The data extension repository.
        /// </param>
        /// <param name="processors">
        ///     The source processors that are available to generate data.
        /// </param>
        /// <param name="cookersToEnable">
        ///     The requested data cookers to enable.
        /// </param>
        /// <returns>
        ///     The set of custom data processors that have data cookers enabled from the requested set.
        /// </returns>
        public static ISet<ICustomDataProcessorWithSourceParser> EnableDataCookers(
            this IDataExtensionRepository self,
            IEnumerable<ICustomDataProcessorWithSourceParser> processors,
            HashSet<DataCookerPath> cookersToEnable)
        {
            Guard.NotNull(self, nameof(self));
            Guard.NotNull(processors, nameof(processors));
            Guard.NotNull(cookersToEnable, nameof(cookersToEnable));

            var customProcessors = new HashSet<ICustomDataProcessorWithSourceParser>();

            void EnableCooker(
                DataCookerPath cooker,
                ICustomDataProcessorWithSourceParser processor)
            {
                if (self.TryGetDataCookerReference(cooker, out IDataCookerReference cookerReference))
                {
                    if (cookerReference is ISourceDataCookerReference sdcr)
                    {
                        var parserId = cookerReference.Path.SourceParserId;
                        if (!StringComparer.OrdinalIgnoreCase.Equals(processor.SourceParserId, parserId))
                        {
                            return;
                        }

                        processor.EnableCooker(sdcr);
                        customProcessors.Add(processor);
                    }

                    foreach (var cookerToEnable in cookerReference.RequiredDataCookers)
                    {
                        EnableCooker(cookerToEnable, processor);
                    }
                }
            }

            foreach (var processor in processors)
            {
                foreach (var cookerToEnable in cookersToEnable)
                {
                    EnableCooker(cookerToEnable, processor);
                }
            }

            return customProcessors;
        }
    }
}
