// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.

using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;

namespace Microsoft.Performance.SDK
{
    /// <summary>
    ///     Provides static (Shared in Visual Basic) extensions for interacting
    ///     with collection instances.
    /// </summary>
    public static class CollectionExtensions
    {
        /// <summary>
        ///     Creates a readonly wrapper for the given array.
        /// </summary>
        /// <typeparam name="T">
        ///     The <see cref="Type" /> of elements in the array.
        /// </typeparam>
        /// <param name="self">
        ///     The array to wrap in a readonly collection.
        /// </param>
        /// <returns>
        ///     A readonly wrapper for the given array.
        /// </returns>
        /// <exception cref="System.ArgumentNullException">
        ///     <paramref name="self" /> is <c>null</c>.
        /// </exception>
        public static ReadOnlyCollection<T> AsReadOnly<T>(this T[] self)
        {
            return Array.AsReadOnly(self);
        }

        /// <summary>
        ///     Creates a readonly wrapper for the given <see cref="IList{T}" />.
        /// </summary>
        /// <typeparam name="T">
        ///     The <see cref="Type" /> of elements in the list.
        /// </typeparam>
        /// <param name="self">
        ///     The list to wrap in a readonly collection.
        /// </param>
        /// <returns>
        ///     A readonly wrapper for the given list.
        /// </returns>
        /// <exception cref="System.ArgumentNullException">
        ///     <paramref name="self" /> is <c>null</c>.
        /// </exception>
        public static ReadOnlyCollection<T> AsReadOnly<T>(this IList<T> self)
        {
            return new ReadOnlyCollection<T>(self);
        }
    }
}
