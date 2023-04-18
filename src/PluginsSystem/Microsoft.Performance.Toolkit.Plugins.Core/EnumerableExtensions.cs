// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.

using System;
using System.Collections.Generic;
using System.Linq;

namespace Microsoft.Performance.Toolkit.Plugins.Core
{
    // TODO:  #293 This should be moved to the SDK.
    /// <summary>
    ///     Provides extension methods for <see cref="IEnumerable{T}"/>.
    /// </summary>
    internal static class EnumerableExtensions
    {
        /// <summary>
        ///     Determines whether two sequences are equal by comparing their elements by using the default equality comparer for their type.
        ///     If both sequences are null, they are considered equal.
        /// </summary>
        /// <typeparam name="T">
        ///     The type of the elements of the input sequences.s
        /// </typeparam>
        /// <param name="first">
        ///     The first sequence to compare.
        /// </param>
        /// <param name="second">
        ///     The second sequence to compare.
        /// </param>
        /// <returns>
        ///     <c>true</c> if the two source sequences are of equal length and their corresponding elements compare equal
        ///     according to the default equality comparer for their type; otherwise, <c>false</c>.
        /// </returns>
        public static bool EnumerableEqual<T>(this IEnumerable<T> first, IEnumerable<T> second)
        {
            if (first == null || second == null)
            {
                return first == null && second == null;
            }

            return first.SequenceEqual(second);
        }

        /// <summary>
        ///     Determines whether two sequences are equal by comparing their elements by using a specified <see cref="IEqualityComparer{T}"/>.
        /// </summary>
        /// <typeparam name="T">
        ///     The type of the elements of the input sequences.
        /// </typeparam>
        /// <param name="first">
        ///     The first sequence to compare.
        /// </param>
        /// <param name="second">
        ///     The second sequence to compare.
        /// </param>
        /// <param name="comparer">
        ///     An <see cref="IEqualityComparer{T}"/> to compare elements.
        /// </param>
        /// <returns>
        ///    <c>true</c> if the two source sequences are of equal length and their corresponding elements compare equal according to comparer;
        ///    <c>false</c> otherwise.
        /// </returns>
        public static bool EnumerableEqual<T>(this IEnumerable<T> first, IEnumerable<T> second, IEqualityComparer<T> comparer)
        {
            if (first == null || second == null)
            {
                return first == null && second == null;
            }

            return first.SequenceEqual(second, comparer);
        }
    }
}
