// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.

using System.Collections.Generic;
using System.Collections.Specialized;
using System.Linq;
using Microsoft.Performance.SDK;

namespace Microsoft.Performance.Toolkit.Plugins.Runtime.Common
{
    /// <summary>
    ///     Represents a thread-safe repository of items.
    /// </summary>
    /// <typeparam name="T">
    ///     The type of items in the repository.
    /// </typeparam>
    public abstract class ThreadSafeRepository<T>
        : IRepository<T>
    {
        private readonly object mutex = new object();

        /// <inheritdoc/>
        public IEnumerable<T> Items
        {
            get
            {
                lock (this.mutex)
                {
                    return GetItemsInternal().ToList();
                }
            }
        }

        /// <inheritdoc/>
        public event NotifyCollectionChangedEventHandler CollectionChanged;

        /// <inheritdoc/>
        public bool Add(T item)
        {
            Guard.NotNull(item, nameof(item));

            lock (this.mutex)
            {
                bool success = AddInternal(item);
                if (success)
                {
                    OnCollectionChanged(
                        new NotifyCollectionChangedEventArgs(NotifyCollectionChangedAction.Add, item));
                }

                return success;
            }
        }

        /// <inheritdoc/>
        public IEnumerable<T> Add(IEnumerable<T> items)
        {
            Guard.NotNull(items, nameof(items));

            lock (this.mutex)
            {
                var addedItems = AddInternal(items).ToList();
                if (addedItems.Any())
                {
                    OnCollectionChanged(
                        new NotifyCollectionChangedEventArgs(NotifyCollectionChangedAction.Add, addedItems));
                }

                return addedItems;
            }
        }

        /// <inheritdoc/>
        public bool Remove(T item)
        {
            Guard.NotNull(item, nameof(item));

            lock (this.mutex)
            {
                bool success = RemoveInternal(item);
                if (success)
                {
                    OnCollectionChanged(
                        new NotifyCollectionChangedEventArgs(NotifyCollectionChangedAction.Remove, item));
                }

                return success;
            }
        }

        /// <inheritdoc/>
        public IEnumerable<T> Remove(IEnumerable<T> items)
        {
            Guard.NotNull(items, nameof(items));

            lock (this.mutex)
            {
                var removedItems = RemoveInternal(items).ToList();
                if (removedItems.Any())
                {
                    OnCollectionChanged(
                        new NotifyCollectionChangedEventArgs(NotifyCollectionChangedAction.Remove, removedItems));
                }

                return removedItems;
            }
        }

        /// <summary>
        ///     Gets the items in the repository.
        /// </summary>
        /// <returns>
        ///     The items in the repository.
        /// </returns>
        protected abstract IEnumerable<T> GetItemsInternal();

        /// <summary>
        ///     Adds the given item to the repository.
        /// </summary>
        /// <param name="item">
        ///     The item to add.
        /// </param>
        /// <returns>
        ///     <c>true</c> if the item was added; <c>false</c> otherwise.
        /// </returns>
        protected abstract bool AddInternal(T item);

        /// <summary>
        ///     Adds the given items to the repository.
        /// </summary>
        /// <param name="items">
        ///     The items to add.
        /// </param>
        /// <returns>
        ///     The items that were added.
        /// </returns>
        protected abstract IEnumerable<T> AddInternal(IEnumerable<T> items);

        /// <summary>
        ///     Removes the given item from the repository.
        /// </summary>
        /// <param name="item">
        ///     The item to remove.
        /// </param>
        /// <returns>
        ///     <c>true</c> if the item was removed; <c>false</c> otherwise.
        /// </returns>
        protected abstract bool RemoveInternal(T item);

        /// <summary>
        ///     Removes the given items from the repository.
        /// </summary>
        /// <param name="items">
        ///     The items to remove.
        /// </param>
        /// <returns>
        ///     The items that were removed.
        /// </returns>
        protected abstract IEnumerable<T> RemoveInternal(IEnumerable<T> items);

        /// <summary>
        ///     Raises the <see cref="CollectionChanged"/> event.
        /// </summary>
        /// <param name="e">
        ///     The event arguments.
        /// </param>
        protected virtual void OnCollectionChanged(NotifyCollectionChangedEventArgs e)
        {
            CollectionChanged?.Invoke(this, e);
        }
    }
}
