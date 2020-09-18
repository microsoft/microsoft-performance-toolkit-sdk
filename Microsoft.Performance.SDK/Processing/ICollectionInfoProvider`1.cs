// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.

namespace Microsoft.Performance.SDK.Processing
{
    /// <summary>
    ///     Provides information about a collection.
    /// </summary>
    /// <typeparam name="TCollection">
    ///     The <see cref="System.Type"/> of collection.
    /// </typeparam>
    public interface ICollectionInfoProvider<TCollection> 
        : INullableInfoProvider<TCollection>
    {
        /// <summary>
        ///     Gets a value indicating whether the collection has
        ///     a unique starting value.
        /// </summary>
        bool HasUniqueStart { get; }

        /// <summary>
        ///     Gets the count of elements in the collection.
        /// </summary>
        /// <param name="collection">
        ///     The collection.
        /// </param>
        /// <returns>
        ///     The count of elements in <paramref name="collection"/>.
        /// </returns>
        int GetCount(TCollection collection);
    }
}
