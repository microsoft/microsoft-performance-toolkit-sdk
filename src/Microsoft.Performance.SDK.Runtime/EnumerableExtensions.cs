// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.

using System;
using System.Collections.Generic;

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
    }
}
