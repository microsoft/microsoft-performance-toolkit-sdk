// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.

using System;
using System.Collections;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;

namespace Microsoft.Performance.SDK
{
    /// <summary>
    ///     Represents a readonly wrapper around a <see cref="HashSet{T}"/>.
    /// </summary>
    /// <typeparam name="T">
    ///     The <see cref="Type"/> of item contained in the collection.
    /// </typeparam>
    [SuppressMessage("Microsoft.Naming", "CA1710:IdentifiersShouldHaveCorrectSuffix")]
    public sealed class ReadOnlyHashSet<T>
        : ISet<T>
    {
        private readonly HashSet<T> source;

        /// <summary>
        ///     Initializes a new instance of the <see cref="ReadOnlyHashSet{T}"/>
        ///     class, wrapping the given <see cref="HashSet{T}"/>.
        /// </summary>
        /// <param name="source">
        ///     The <see cref="HashSet{T}"/> to wrap.
        /// </param>
        /// <exception cref="System.ArgumentNullException">
        ///     <paramref name="source"/> is <c>null</c>.
        /// </exception>
        public ReadOnlyHashSet(HashSet<T> source)
        {
            Guard.NotNull(source, nameof(source));

            this.source = source;
        }

        /// <inheritdoc />
        public void Add(T item)
        {
            throw new NotSupportedException();
        }

        /// <inheritdoc />
        public void Clear()
        {
            throw new NotSupportedException();
        }

        /// <inheritdoc />
        public bool Contains(T item)
        {
            return this.source.Contains(item);
        }

        /// <inheritdoc />
        public void CopyTo(T[] array, int arrayIndex)
        {
            this.source.CopyTo(array, arrayIndex);
        }

        /// <inheritdoc />
        public int Count
        {
            get
            {
                return this.source.Count;
            }
        }

        /// <inheritdoc />
        public bool IsReadOnly
        {
            get
            {
                return true;
            }
        }

        /// <inheritdoc />
        public bool Remove(T item)
        {
            throw new NotSupportedException();
        }

        /// <inheritdoc />
        public IEnumerator<T> GetEnumerator()
        {
            return this.source.GetEnumerator();
        }

        /// <inheritdoc />
        IEnumerator IEnumerable.GetEnumerator()
        {
            return ((IEnumerable)this.source).GetEnumerator();
        }

        /// <inheritdoc />
        bool ISet<T>.Add(T item)
        {
            throw new NotSupportedException();
        }

        /// <inheritdoc />
        public void UnionWith(IEnumerable<T> other)
        {
            throw new NotSupportedException();
        }

        /// <inheritdoc />
        public void IntersectWith(IEnumerable<T> other)
        {
            throw new NotSupportedException();
        }

        /// <inheritdoc />
        public void ExceptWith(IEnumerable<T> other)
        {
            throw new NotSupportedException();
        }

        /// <inheritdoc />
        public void SymmetricExceptWith(IEnumerable<T> other)
        {
            throw new NotSupportedException();
        }

        /// <inheritdoc />
        public bool IsSubsetOf(IEnumerable<T> other)
        {
            return this.source.IsSubsetOf(other);
        }

        /// <inheritdoc />
        public bool IsSupersetOf(IEnumerable<T> other)
        {
            return this.source.IsSupersetOf(other);
        }

        /// <inheritdoc />
        public bool IsProperSupersetOf(IEnumerable<T> other)
        {
            return this.source.IsProperSupersetOf(other);
        }

        /// <inheritdoc />
        public bool IsProperSubsetOf(IEnumerable<T> other)
        {
            return this.source.IsProperSubsetOf(other);
        }

        /// <inheritdoc />
        public bool Overlaps(IEnumerable<T> other)
        {
            return this.source.Overlaps(other);
        }

        /// <inheritdoc />
        public bool SetEquals(IEnumerable<T> other)
        {
            return this.source.SetEquals(other);
        }
    }
}
