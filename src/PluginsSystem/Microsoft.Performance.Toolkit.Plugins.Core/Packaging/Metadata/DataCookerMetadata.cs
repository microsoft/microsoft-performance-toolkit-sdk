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
            string sourceParserId)
        {
            this.Name = name;
            this.Description = description;
            this.SourceParserId = sourceParserId;
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
        ///     Gets the ID of the source parser this data cooker is associated with.
        /// </summary>
        public string SourceParserId { get; }

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
                && string.Equals(this.SourceParserId, other.SourceParserId, StringComparison.Ordinal);
        }

        /// <inheritdoc />
        public override int GetHashCode()
        {
            return HashCodeUtils.CombineHashCodeValues(
                this.Name?.GetHashCode() ?? 0,
                this.Description?.GetHashCode() ?? 0,
                this.SourceParserId.GetHashCode());
        }
    }
}
