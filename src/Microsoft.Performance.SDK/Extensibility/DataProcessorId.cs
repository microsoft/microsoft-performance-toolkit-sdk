// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.

using System;

namespace Microsoft.Performance.SDK.Extensibility
{
    /// <summary>
    ///     Identifies a data processor
    /// </summary>
    public struct DataProcessorId
        : IEquatable<DataProcessorId>
    {
        /// <summary>
        ///     Constructor
        /// </summary>
        /// <param name="dataProcessorId">
        ///     Data processor Id.
        /// </param>
        public DataProcessorId(string dataProcessorId)
        {
            Guard.NotNullOrWhiteSpace(dataProcessorId, nameof(dataProcessorId));
            this.Id = dataProcessorId;
        }

        /// <summary>
        ///     Gets the Data processor Id.
        /// </summary>
        public string Id { get; }

        /// <inheritdoc />
        public bool Equals(DataProcessorId other)
        {
            return string.Equals(this.Id, other.Id);
        }

        /// <inheritdoc />
        public override bool Equals(object obj)
        {
            if (ReferenceEquals(null, obj)) return false;
            return obj is DataProcessorId other && Equals(other);
        }

        /// <inheritdoc />
        public override int GetHashCode()
        {
            return (this.Id != null ? this.Id.GetHashCode() : 0);
        }
    }
}
