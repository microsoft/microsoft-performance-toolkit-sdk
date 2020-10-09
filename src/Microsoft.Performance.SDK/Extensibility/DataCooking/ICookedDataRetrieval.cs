// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.

namespace Microsoft.Performance.SDK.Extensibility.DataCooking
{
    /// <summary>
    ///     Defines methods for retrieving data output from a data cooker.
    /// </summary>
    public interface ICookedDataRetrieval
    {
        /// <summary>
        ///     Retrieves data by name, typecast to the given type.
        ///     The caller is coupled to the data extension and expected to know the
        ///     data type.
        /// </summary>
        /// <typeparam name="T">
        ///     Type of the data to retrieve.
        /// </typeparam>
        /// <param name="identifier">
        ///     Unique identifier for the data to retrieve.
        /// </param>
        /// <returns>
        ///     The uniquely identified data.
        /// </returns>
        T QueryOutput<T>(DataOutputPath identifier);

        /// <summary>
        ///     Retrieves data by name.
        /// </summary>
        /// <param name="identifier">
        ///     Unique identifier for the data to retrieve.
        /// </param>
        /// <returns>
        ///     The uniquely identified data, as an object.
        /// </returns>
        object QueryOutput(DataOutputPath identifier);
    }
}
