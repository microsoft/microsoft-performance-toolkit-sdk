// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.

using Microsoft.Performance.SDK.Extensibility.DataCooking;

namespace Microsoft.Performance.SDK.Extensibility
{
    /// <summary>
    ///     Additional functionality for the <see cref="ICookedDataRetrieval"/> interface.
    /// </summary>
    public static class ICookedDataRetrievalExtensions
    {
        /// <summary>
        ///     Retrieves data by name, typecast to the given type.
        ///     The caller is coupled to the data extension and expected to know the
        ///     data type.
        /// </summary>
        /// <typeparam name="T">
        ///     Type of the data to retrieve.
        /// </typeparam>
        /// <param name="retrieval">
        ///     Instance on which to act.
        /// </param>
        /// <param name="dataOutputPath">
        ///     Unique identifier for the data to retrieve.
        /// </param>
        /// <param name="result">
        ///     The uniquely identified data.
        /// </param>
        /// <exception cref="InvalidOperationException">
        ///     The specified <paramref name="dataOutputPath"/> was not found.
        /// </exception>
        /// <exception cref="InvalidCastException">
        ///     The value at <paramref name="dataOutputPath"/> cannot be cast to <typeparamref name="T"/>.
        /// </exception>
        public static void QueryOutput<T>(
            this ICookedDataRetrieval retrieval,
            DataOutputPath dataOutputPath,
            out T result)
        {
            result = retrieval.QueryOutput<T>(dataOutputPath);
        }

        /// <summary>
        ///     Retrieves data by name.
        /// </summary>
        /// <param name="retrieval">
        ///     Instance on which to act.
        /// </param>
        /// <param name="cookerPath">
        ///     Identifies the data cooker to query.
        /// </param>
        /// <param name="outputId">
        ///     Identifies which data on the data cooker to retrieve.
        /// </param>
        /// <returns>
        ///     The uniquely identified data.
        /// </returns>
        /// <exception cref="InvalidOperationException">
        ///     The specified <paramref name="outputId"/> was not found on the data cooker 
        ///     <paramref name="cookerPath"/>.
        /// </exception>
        public static object QueryOutput(
            this ICookedDataRetrieval retrieval,
            DataCookerPath cookerPath,
            string outputId)
        {
            return retrieval.QueryOutput(new DataOutputPath(cookerPath, outputId));
        }

        /// <summary>
        ///     Retrieves data by name.
        /// </summary>
        /// <param name="retrieval">
        ///     Instance on which to act.
        /// </param>
        /// <param name="cookerPath">
        ///     Identifies the data cooker to query.
        /// </param>
        /// <param name="outputId">
        ///     Identifies which data on the data cooker to retrieve.
        /// </param>
        /// <param name="result">
        ///     The uniquely identified data.
        /// </param>
        /// <returns>
        ///     true if the value was successfully retrieved; false otherwise
        /// </returns>
        public static bool TryQueryOutput(
            this ICookedDataRetrieval retrieval,
            DataCookerPath cookerPath,
            string outputId,
            out object result)
        {
            return retrieval.TryQueryOutput(new DataOutputPath(cookerPath, outputId), out result);
        }

        /// <summary>
        ///     Retrieves data by name, typecast to the given type.
        ///     The caller is coupled to the data extension and expected to know the
        ///     data type.
        /// </summary>
        /// <typeparam name="T">
        ///     Type of the data to retrieve.
        /// </typeparam>
        /// <param name="retrieval">
        ///     Instance on which to act.
        /// </param>
        /// <param name="cookerPath">
        ///     Identifies the data cooker to query.
        /// </param>
        /// <param name="outputId">
        ///     Identifies which data on the data cooker to retrieve.
        /// </param>
        /// <returns>
        ///     The uniquely identified data.
        /// </returns>
        /// <exception cref="InvalidOperationException">
        ///     The specified <paramref name="outputId"/> was not found on the data cooker 
        ///     <paramref name="cookerPath"/>.
        /// </exception>
        /// <exception cref="InvalidCastException">
        ///     The value at <paramref name="identifier"/> cannot be cast to <typeparamref name="T"/>.
        /// </exception>
        public static T QueryOutput<T>(
            this ICookedDataRetrieval retrieval,
            DataCookerPath cookerPath,
            string outputId)
        {
            return retrieval.QueryOutput<T>(new DataOutputPath(cookerPath, outputId));
        }

        /// <summary>
        ///     Retrieves data by name, typecast to the given type.
        ///     The caller is coupled to the data extension and expected to know the
        ///     data type.
        /// </summary>
        /// <typeparam name="T">
        ///     Type of the data to retrieve.
        /// </typeparam>
        /// <param name="retrieval">
        ///     Instance on which to act.
        /// </param>
        /// <param name="cookerPath">
        ///     Identifies the data cooker to query.
        /// </param>
        /// <param name="outputId">
        ///     Identifies which data on the data cooker to retrieve.
        /// </param>
        /// <param name="result">
        ///     The uniquely identified data.
        /// </param>
        /// <exception cref="InvalidOperationException">
        ///     The specified <paramref name="outputId"/> was not found on the data cooker 
        ///     <paramref name="cookerPath"/>.
        /// </exception>
        /// <exception cref="InvalidCastException">
        ///     The value at <paramref name="identifier"/> cannot be cast to <typeparamref name="T"/>.
        /// </exception>
        public static void QueryOutput<T>(
            this ICookedDataRetrieval retrieval,
            DataCookerPath cookerPath,
            string outputId,
            out T result)
        {
            retrieval.QueryOutput(new DataOutputPath(cookerPath, outputId), out result);
        }

        /// <summary>
        ///     Retrieves data by name, typecast to the given type.
        ///     The caller is coupled to the data extension and expected to know the
        ///     data type.
        /// </summary>
        /// <typeparam name="T">
        ///     Type of the data to retrieve.
        /// </typeparam>
        /// <param name="retrieval">
        ///     Instance on which to act.
        /// </param>
        /// <param name="cookerPath">
        ///     Identifies the data cooker to query.
        /// </param>
        /// <param name="outputId">
        ///     Identifies which data on the data cooker to retrieve.
        /// </param>
        /// <param name="result">
        ///     The uniquely identified data.
        /// </param>
        /// <returns>
        ///     true if the value was successfully retrieved; false otherwise
        /// </returns>
        public static bool TryQueryOutput<T>(
            this ICookedDataRetrieval retrieval,
            DataCookerPath cookerPath,
            string outputId,
            out T result)
        {
            return retrieval.TryQueryOutput(new DataOutputPath(cookerPath, outputId), out result);
        }
    }
}
