// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.

using System;

namespace Microsoft.Performance.SDK.Extensibility
{
    /// <summary>
    ///     The data exposed by through an <see cref="ISourceParser{T,TContext, TKey}"/> must implement this interface.
    /// </summary>
    /// <typeparam name="TKey">
    ///     A type that acts a key identifier for the data.
    /// </typeparam>
    public interface IKeyedDataType<TKey>
        : IComparable<TKey>
    {
        /// <summary>
        ///     The type returned from this is used to determine which <see cref="IDataCooker{T,TContext,TKey}"/> will receive this data for processing.
        /// </summary>
        /// <returns>
        ///     The type used as a key for this data.
        /// </returns>
        TKey GetKey();
    }
}
