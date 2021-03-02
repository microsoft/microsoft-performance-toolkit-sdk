// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.

using System;
using System.Collections.Generic;
using System.Linq;

namespace Microsoft.Performance.SDK.Runtime
{
    internal static class EnumerableExtensions
    {
        internal static IEnumerable<T> AsEnumerableSingleton<T>(this T self)
        {
            yield return self;
        }

        internal static IEnumerable<T> Concat<T>(this IEnumerable<T> head, T tail)
        {
            foreach (var item in head)
            {
                yield return item;
            }

            yield return tail;
        }

        internal static void ForEach<T>(this IEnumerable<T> items, Action<T> action)
        {
            foreach (var item in items)
            {
                action(item);
            }
        }

        internal static HashSet<T> ToSet<T>(this IEnumerable<T> items)
        {
            return new HashSet<T>(items);
        }

        internal static HashSet<T> ToSet<T>(this IEnumerable<T> items, IEqualityComparer<T> cmp)
        {
            return new HashSet<T>(items, cmp);
        }

        internal static IEnumerable<T> Except<T>(this IEnumerable<T> items, T e)
        {
            return items.Except(new[] { e, });
        }
    }
}
