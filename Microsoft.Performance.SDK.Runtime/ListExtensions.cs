// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.

using System;
using System.Collections.Generic;

namespace Microsoft.Performance.SDK.Runtime
{
    internal static class ListExtensions
    {
        internal static void EnsureCapacity<T>(this List<T> list, int newCapacity)
        {
            // TODO: This must be made "smarter" about what capacity value to use
            //       since setting List<T>.Capacity is very expensive. It always
            //       allocates a new array and copies its data into it, even for
            //       small changes to Capacity.
            if (list.Capacity < newCapacity)
            {
                list.Capacity = newCapacity;
            }
        }

        internal static IEnumerable<T> Range<T>(this IList<T> items, int startIndex, int length)
        {
            if (items == null)
            {
                throw new ArgumentNullException("items");
            }

            if (startIndex < 0 || length < 0 || (startIndex + length) > items.Count)
            {
                throw new ArgumentOutOfRangeException(null, "startIndex and length must be within the bounds of the list");
            }

            return RangeImpl(items, startIndex, length);
        }

        private static IEnumerable<T> RangeImpl<T>(IList<T> items, int startIndex, int length)
        {
            for (int i = startIndex; i < startIndex + length; ++i)
            {
                yield return items[i];
            }
        }
    }
}
