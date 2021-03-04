// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.

using System;
using System.Collections.Generic;
using System.Linq;

namespace Microsoft.Performance.SDK.Runtime
{
    /// <summary>
    ///     Contains static (Shared in Visual Basic) methods for interacting
    ///     with <see cref="Enumerable"/> instances.
    /// </summary>
    internal static class EnumerableExtensions
    {
        /// <summary>
        ///     Generates a new sequence whose only item is the given
        ///     item.
        /// </summary>
        /// <typeparam name="T">
        ///     The <see cref="Type"/> of the item.
        /// </typeparam>
        /// <param name="self">
        ///     The sole item to be placed into a sequence.
        /// </param>
        /// <returns>
        ///     A new sequence consisting solely of the given item.
        /// </returns>
        internal static IEnumerable<T> AsEnumerableSingleton<T>(this T self)
        {
            yield return self;
        }

        /// <summary>
        ///     Generates a new sequence comprised of the given sequence
        ///     with the given item added at the end.
        /// </summary>
        /// <typeparam name="T">
        ///     The <see cref="Type"/> of the item.
        /// </typeparam>
        /// <param name="head">
        ///     The sole item to be placed into a sequence.
        /// </param>
        /// <returns>
        ///     An <see cref="IEnumerable{T}"/> that contains the concatenated
        ///     elements of the first sequence with the given element.
        /// </returns>
        /// <exception cref="System.ArgumentNullException">
        ///     <paramref name="head"/> is <c>null</c>.
        /// </exception>
        internal static IEnumerable<T> Concat<T>(this IEnumerable<T> head, T tail)
        {
            Guard.NotNull(head, nameof(head));

            foreach (var item in head)
            {
                yield return item;
            }

            yield return tail;
        }

        /// <summary>
        ///     Performs the given <see cref="Action{T}"/> upon each element
        ///     of the given sequence.
        /// </summary>
        /// <typeparam name="T">
        ///     The <see cref="Type"/> of the item.
        /// </typeparam>
        /// <param name="items">
        ///     The sequence upon whose items are to operated.
        /// </param>
        /// <param name="action">
        ///     The operation to apply to each element of <paramref name="items"/>.
        /// </param>
        /// <exception cref="System.ArgumentNullException">
        ///     <paramref name="items"/> is <c>null</c>.
        /// </exception>
        internal static void ForEach<T>(this IEnumerable<T> items, Action<T> action)
        {
            Guard.NotNull(items, nameof(items));
            if (action is null)
            {
                return;
            }

            foreach (var item in items)
            {
                action(item);
            }
        }

        /// <summary>
        ///     Creates a <see cref="HashSet{T}"/> from the items
        ///     in the given sequence, using the default equality
        ///     comparer.
        /// </summary>
        /// <typeparam name="T">
        ///     The <see cref="Type"/> of items in the sequence.
        /// </typeparam>
        /// <param name="items">
        ///     The sequence of items to be turned into a set.
        /// </param>
        /// <returns>
        ///     A new <see cref="HashSet{T}"/> whose contents are
        ///     the elements of <paramref name="items"/>.
        /// </returns>
        internal static HashSet<T> ToSet<T>(this IEnumerable<T> items)
        {
            return new HashSet<T>(items);
        }

        /// <summary>
        ///     Produces a sequence consisting of the given sequence
        ///     without the given item, using the default equality
        ///     comparer to compare the given item with the items in
        ///     the sequence.
        /// </summary>
        /// <typeparam name="T">
        ///     The <see cref="Type"/> of items in the sequence.
        /// </typeparam>
        /// <param name="items">
        ///     A sequence of items whose elements are not considered
        ///     to be equal to <paramref name="e"/> to be returned.
        /// </param>
        /// <param name="e">
        ///     The item to exclude from the new sequence.
        /// </param>
        /// <returns>
        ///     A sequence that contains the items of the original sequence
        ///     without the given item.
        /// </returns>
        /// <exception cref="System.ArgumentNullException">
        ///     <paramref name="items"/> is <c>null</c>.
        /// </exception>
        internal static IEnumerable<T> Without<T>(this IEnumerable<T> items, T e)
        {
            return items.Except(new[] { e, });
        }
    }
}
