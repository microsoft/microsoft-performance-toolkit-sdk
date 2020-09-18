// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.

using System.Collections.Concurrent;
using System.Collections.Generic;

namespace Microsoft.Performance.SDK.Runtime
{
    /// <summary>
    ///     Concurrent version of <see cref="HashSet{T}"/>.
    /// </summary>
    /// <typeparam name="T">
    ///     <inheritdoc cref="HashSet{T}"/>
    /// </typeparam>
    public sealed class ConcurrentSet<T>
        : ICollection<T>
    {
        private ConcurrentDictionary<T, object> items;

        /// <summary>
        ///     Initializes a new instance of the <see cref="ConcurrentSet{T}"/> class
        ///     that is empty and uses the default equality comparer for the set type.
        /// </summary>
        public ConcurrentSet()
        {
            this.items = new ConcurrentDictionary<T, object>();
        }

        /// <summary>
        ///     Initializes a new instance of the <see cref="ConcurrentSet{T}"/> class
        ///     that uses the default equality comparer for the set type, contains elements copied
        ///     from the specified collection, and has sufficient capacity to accommodate the
        ///     number of elements copied.
        /// </summary>
        /// <param name="items">
        ///     The collection whose elements are copied to the new set.
        /// </param>
        public ConcurrentSet(IEnumerable<T> items)
        {
            this.items = new ConcurrentDictionary<T, object>();
            foreach (T item in items)
            {
                Add(item);
            }
        }

        /// <inheritdoc cref="HashSet{T}.Add(T)"/>
        public bool Add(T item)
        {
            return this.items.TryAdd(item, null);
        }

        void ICollection<T>.Add(T item)
        {
            Add(item);
        }

        /// <summary>
        ///     Removes all elements from the set.
        /// </summary>
        public void Clear()
        {
            this.items.Clear();
        }

        /// <inheritdoc cref="HashSet{T}.Contains(T)"/>
        public bool Contains(T item)
        {
            return this.items.ContainsKey(item);
        }

        /// <inheritdoc cref="HashSet{T}.CopyTo(T[], int)"/>
        public void CopyTo(T[] array, int arrayIndex)
        {
            this.items.Keys.CopyTo(array, arrayIndex);
        }

        /// <inheritdoc cref="HashSet{T}.Count"/>
        public int Count
        {
            get
            {
                return this.items.Count;
            }
        }

        public bool IsReadOnly
        {
            get
            {
                return false;
            }
        }

        /// <summary>
        ///     Removes the specified element from the set.
        /// </summary>
        /// <param name="item">
        ///     <inheritdoc cref="HashSet{T}.Remove(T)"/>
        /// </param>
        /// <returns>
        ///     <inheritdoc cref="HashSet{T}.Remove(T)"/>
        /// </returns>
        public bool Remove(T item)
        {
            object value;
            return this.items.TryRemove(item, out value);
        }

        /// <summary>
        ///     Creates an array of the current set.
        /// </summary>
        /// <returns>
        ///     An array that contains the elements from the input sequence.
        /// </returns>
        public T[] ToArray()
        {
            // ConcurrentDictionary implements ToArray() in a properly atomic manner, so we don't need locks or any other wizardry
            KeyValuePair<T, object>[] kvpArray = this.items.ToArray();

            T[] array = new T[kvpArray.Length];

            for (int i = 0; i < kvpArray.Length; ++i)
            {
                array[i] = kvpArray[i].Key;
            }

            return array;
        }

        public IEnumerator<T> GetEnumerator()
        {
            return this.items.Keys.GetEnumerator();
        }

        System.Collections.IEnumerator System.Collections.IEnumerable.GetEnumerator()
        {
            return GetEnumerator();
        }
    }
}
