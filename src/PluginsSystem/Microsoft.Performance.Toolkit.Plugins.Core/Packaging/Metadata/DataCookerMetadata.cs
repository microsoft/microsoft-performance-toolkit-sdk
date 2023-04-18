// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.

using System;
using System.Text.Json.Serialization;
using Microsoft.Performance.SDK;

namespace Microsoft.Performance.Toolkit.Plugins.Core.Packaging.Metadata
{
    /// <summary>
    ///     Represents the metadata of a data cooker.
    /// </summary>
    public sealed class DataCookerMetadata
        : IEquatable<DataCookerMetadata>
    {
        /// <summary>
        ///     Initializes an instance of <see cref="DataCookerMetadata".
        /// </summary>
        [JsonConstructor]
        public DataCookerMetadata(
            string name,
            string description,
            Guid processingSourceGuid)
        {
            this.Name = name;
            this.Description = description;
            this.ProcessingSourceGuid = processingSourceGuid;
        }

        /// <summary>
        ///     Gets the name of this data cooker.
        /// </summary>
        public string Name { get; }

        /// <summary>
        ///     Gets the user friendly description of this data cooker.
        /// </summary>
        public string Description { get; }

        /// <summary>
        ///     Gets the Guid of the processing source this data cooker roots from.
        /// </summary>
        public Guid ProcessingSourceGuid { get; }

        /// <inheritdoc />
        public override bool Equals(object obj)
        {
            return Equals(obj as DataCookerMetadata);
        }

        /// <inheritdoc />
        public bool Equals(DataCookerMetadata other)
        {
            if (other == null)
            {
                return false;
            }

            if (ReferenceEquals(this, other))
            {
                return true;
            }

            return string.Equals(this.Name, other.Name, StringComparison.Ordinal)
                && string.Equals(this.Description, other.Description, StringComparison.Ordinal)
                && this.ProcessingSourceGuid == other.ProcessingSourceGuid;
        }

        /// <inheritdoc />
        public override int GetHashCode()
        {
            return HashCodeUtils.CombineHashCodeValues(
                this.Name?.GetHashCode() ?? 0,
                this.Description?.GetHashCode() ?? 0,
                this.ProcessingSourceGuid.GetHashCode());
        }
    }
}
