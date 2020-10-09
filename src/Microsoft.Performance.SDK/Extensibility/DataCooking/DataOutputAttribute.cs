// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.

using System;

namespace Microsoft.Performance.SDK.Extensibility.DataCooking
{
    /// <summary>
    ///     This attribute is used by data cookers deriving from <see cref="CookedDataReflector"/> to identify properties
    ///     to be exposed as output from an <see cref="ICookedDataSet"/>.
    /// </summary>
    [AttributeUsage(AttributeTargets.Property, AllowMultiple = false)]
    public class DataOutputAttribute
        : Attribute
    {
        /// <summary>
        ///     Creates a <see cref="DataOutputPath"/> based on the property name to which this attribute is attached.
        /// </summary>
        public DataOutputAttribute()
        {
        }

        /// <summary>
        ///     Creates a <see cref="DataOutputPath"/> based on the name provided.
        /// </summary>
        /// <param name="dataIdentifier">
        ///     The data cooker id which will later be combined with a <see cref="DataCookerPath"/> to form a
        ///     <see cref="DataOutputPath"/>.
        /// </param>
        public DataOutputAttribute(string dataIdentifier)
        {
            Guard.NotNullOrWhiteSpace(dataIdentifier, nameof(dataIdentifier));

            this.DataIdentifier = dataIdentifier;
        }

        /// <summary>
        ///     Gets a unique identifier for the data to expose through a <see cref="DataOutputPath"/>.
        ///     <remarks>
        ///         This identifier represents the data cooker Id, not the complete <see cref="DataCookerPath"/>. It will be
        ///         combined with a <see cref="DataCookerPath"/> for form a complete  <see cref="DataOutputPath"/>.
        ///     </remarks>
        /// </summary>
        public string DataIdentifier { get; }
    }
}
