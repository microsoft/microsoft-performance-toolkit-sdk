// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.

using Microsoft.Performance.SDK.Extensibility;
using Microsoft.Performance.SDK.Extensibility.DataCooking;

namespace Microsoft.Performance.Toolkit.Engine
{
    /// <summary>
    ///     Retrieves data from a single cooker instance.
    /// </summary>
    public sealed class CookerOutput
    {
        private readonly DataCookerPath cookerPath;
        private readonly ICookedDataRetrieval retrieval;

        internal CookerOutput(
            DataCookerPath cookerPath,
            ICookedDataRetrieval retrieval)
        {
            this.cookerPath = cookerPath;
            this.retrieval = retrieval;
        }

        /// <summary>
        ///     Retrieves data by name, typecast to the given type.
        ///     The caller is coupled to the data extension and expected to know the
        ///     data type.
        /// </summary>
        /// <typeparam name="T">
        ///     Type of the data to retrieve.
        /// </typeparam>
        /// <param name="name">
        ///     The name of the data output.
        /// </param>
        /// <returns>
        ///     The uniquely identified data.
        /// </returns>
        /// <exception cref="InvalidOperationException">
        ///     <paramref name="name"/> references data that does not exist.
        /// </exception>
        /// <exception cref="InvalidCastException">
        ///     The value at <paramref name="name"/> cannot be cast to <typeparamref name="T"/>.
        /// </exception>
        public T QueryOutput<T>(string name)
        {
            var p = CreateOutputPath(this.cookerPath, name);
            return this.retrieval.QueryOutput<T>(p);
        }

        /// <summary>
        ///     Retrieves data by name.
        /// </summary>
        /// <param name="name">
        ///     Unique name of the data to retrieve.
        /// </param>
        /// <returns>
        ///     The uniquely identified data, as an <see cref="object"/>.
        /// </returns>
        /// <exception cref="InvalidOperationException">
        ///     <paramref name="name"/> references data that does not exist.
        /// </exception>
        public object QueryOutput(string name)
        {
            var p = CreateOutputPath(this.cookerPath, name);
            return this.retrieval.QueryOutput(p);
        }

        /// <summary>
        ///     Retrieves data by name, typecast to the given type.
        ///     The caller is coupled to the data extension and expected to know the
        ///     data type.
        /// </summary>
        /// <typeparam name="T">
        ///     Type of the data to retrieve.
        /// </typeparam>
        /// <param name="name">
        ///     Unique name of the data to retrieve.
        /// </param>
        /// <param name="result">
        ///     The uniquely identified data.
        /// </param>
        /// <returns>
        ///     <c>true</c> if the value was successfully retrieved; <c>false</c> otherwise.
        /// </returns>
        public bool TryQueryOutput<T>(string name, out T result)
        {
            var p = CreateOutputPath(this.cookerPath, name);
            return this.retrieval.TryQueryOutput<T>(p, out result);
        }

        /// <summary>
        ///     Retrieves data by name.
        /// </summary>
        /// <param name="name">
        ///     Unique name of the data to retrieve.
        /// </param>
        /// <param name="result">
        ///     The uniquely identified data, as an object.
        /// </param>
        /// <returns>
        ///     <c>true</c> if the value was successfully retrieved; <c>false</c> otherwise.
        /// </returns>
        public bool TryQueryOutput(string name, out object result)
        {
            var p = CreateOutputPath(this.cookerPath, name);
            return this.retrieval.TryQueryOutput(p, out result);
        }

        private static DataOutputPath CreateOutputPath(DataCookerPath cookerPath, string name)
        {
            return cookerPath.DataCookerType == DataCookerType.SourceDataCooker
                ? DataOutputPath.ForSource(cookerPath.SourceParserId, cookerPath.DataCookerId, name)
                : DataOutputPath.ForComposite(cookerPath.DataCookerId, name);
        }
    }
}
