// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.

using System;
using System.Collections.Generic;
using System.Text.Json.Serialization;
using Microsoft.Performance.SDK;

namespace Microsoft.Performance.Toolkit.Plugins.Core.Packaging.Metadata
{
    /// <summary>
    ///     Represents the contents of a plugin.
    /// </summary>
    public sealed class PluginContents
        : IEquatable<PluginContents>
    {
        /// <summary>
        ///     Initializes a new instance of the <see cref="PluginContents"/> class with the specified parameters.
        /// </summary>
        [JsonConstructor]
        public PluginContents(
            IEnumerable<ProcessingSourceMetadata> processingSources,
            IEnumerable<DataCookerMetadata> dataCookers,
            IEnumerable<TableMetadata> extensibleTables)
        {
            this.ProcessingSources = processingSources;
            this.DataCookers = dataCookers;
            this.ExtensibleTables = extensibleTables;
        }

        /// <summary>
        ///     Gets the metadata of the processing sources contained in this plugin.
        /// </summary>
        public IEnumerable<ProcessingSourceMetadata> ProcessingSources { get; }

        /// <summary>
        ///     Gets the metadata of the data cookers contained in this plugin.
        /// </summary>
        public IEnumerable<DataCookerMetadata> DataCookers { get; }

        /// <summary>
        ///     Gets the metadata of the extensible tables contained in this plugin.
        /// </summary>
        public IEnumerable<TableMetadata> ExtensibleTables { get; }

        /// <inheritdoc/>
        public override bool Equals(object obj)
        {
            return Equals(obj as PluginContents);
        }

        /// <inheritdoc/>
        public bool Equals(PluginContents other)
        {
            if (other is null)
            {
                return false;
            }

            if (ReferenceEquals(this, other))
            {
                return true;
            }

            return this.ProcessingSources.EnumerableEqual(other.ProcessingSources)
                && this.DataCookers.EnumerableEqual(other.DataCookers)
                && this.ExtensibleTables.EnumerableEqual(other.ExtensibleTables);
        }

        /// <inheritdoc/>
        public override int GetHashCode()
        {
            int result = 0;

            if (this.ProcessingSources != null)
            {
                foreach (ProcessingSourceMetadata source in this.ProcessingSources)
                {
                    result = HashCodeUtils.CombineHashCodeValues(result, source?.GetHashCode() ?? 0);
                }
            }

            if (this.DataCookers != null)
            {
                foreach (DataCookerMetadata cooker in this.DataCookers)
                {
                    result = HashCodeUtils.CombineHashCodeValues(result, cooker?.GetHashCode() ?? 0);
                }
            }

            if (this.ExtensibleTables != null)
            {
                foreach (TableMetadata table in this.ExtensibleTables)
                {
                    result = HashCodeUtils.CombineHashCodeValues(result, table?.GetHashCode() ?? 0);
                }
            }

            return result;
        }
    }
}
