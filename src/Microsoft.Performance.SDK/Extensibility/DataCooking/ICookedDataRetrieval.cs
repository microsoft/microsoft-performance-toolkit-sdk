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
        /// <exception cref="InvalidOperationException">
        ///     <paramref name="identifier"/> references a <see cref="DataOutputPath"/> that does not exist.
        /// </exception>
        /// <exception cref="InvalidCastException">
        ///     The value at <paramref name="identifier"/> cannot be cast to <typeparamref name="T"/>.
        /// </exception>
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
        /// <exception cref="InvalidOperationException">
        ///     <paramref name="identifier"/> references a <see cref="DataOutputPath"/> that does not exist.
        /// </exception>
        object QueryOutput(DataOutputPath identifier);

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
        /// <param name="result">
        ///     The uniquely identified data.
        /// </param>
        /// <returns>
        ///     true if the value was successfully retrieved; false otherwise
        /// </returns>
        bool TryQueryOutput<T>(DataOutputPath identifier, out T result);

        /// <summary>
        ///     Retrieves data by name.
        /// </summary>
        /// <param name="identifier">
        ///     Unique identifier for the data to retrieve.
        /// </param>
        /// <param name="result">
        ///     The uniquely identified data, as an object.
        /// </param>
        /// <returns>
        ///     true if the value was successfully retrieved; false otherwise
        /// </returns>
        bool TryQueryOutput(DataOutputPath identifier, out object result);
    }
}
