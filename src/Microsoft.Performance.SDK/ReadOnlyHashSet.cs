// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.

using System.Collections.Generic;

namespace Microsoft.Performance.SDK
{
    /// <summary>
    ///     Contains static (Shared in Visual Basic) methods for interacting
    ///     with <see cref="ReadOnlyHashSet{T}"/> instances.
    /// </summary>
    public static class ReadOnlyHashSet
    {
        /// <summary>
        ///     Gets the empty instance of <see cref="ReadOnlyHashSet{T}"/>.
        /// </summary>
        /// <typeparam name="T">
        ///     The <see cref="System.Type"/> of item contained in the collection.
        /// </typeparam>
        /// <returns>
        ///     The empty instance.
        /// </returns>
        public static ReadOnlyHashSet<T> Empty<T>()
        {
            return EmptyHelper<T>.Instance;
        }

        private static class EmptyHelper<T>
        {
            public static readonly ReadOnlyHashSet<T> Instance =
                new ReadOnlyHashSet<T>(new HashSet<T>(EmptyArray<T>.Instance));
        }
    }
}
