// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.

using System.Collections.Generic;

namespace Microsoft.Performance.Toolkit.Plugins.Runtime.Common
{
    /// <summary>
    ///     Represents a repository for storing a collection of entities of type <typeparamref name="TEntity"/>.
    ///     This repository supports adding and removing items.
    /// </summary>
    /// <typeparam name="TEntity">
    ///     The type of the resources stored in this repository.
    /// </typeparam>
    public interface IRepository<TEntity>
        : IRepositoryRO<TEntity>
    {
        /// <summary>
        ///     Adds a new item to the repository.
        /// </summary>
        /// <param name="item">
        ///     The item to add.
        /// </param>
        /// <returns>
        ///     <c>true</c> if the item was added; <c>false</c> otherwise.
        /// </returns>    
        bool Add(TEntity item);

        /// <summary>
        ///     Adds a collection of new items to the repository.
        /// </summary>
        /// <param name="items">
        ///     The items to add.
        /// </param>
        /// <returns>
        ///     The items that were added.
        /// </returns>
        IEnumerable<TEntity> Add(IEnumerable<TEntity> items);

        /// <summary>
        ///     Removes an item from the repository.
        /// </summary>
        /// <param name="item">
        ///     The item to remove.
        /// </param>
        /// <returns>
        ///     <c>true</c> if the item was removed; <c>false</c> otherwise.
        /// </returns>
        bool Remove(TEntity item);

        /// <summary>
        ///     Removes a collection of items from the repository.
        /// </summary>
        /// <param name="items">
        ///     The items to remove.
        /// </param>
        /// <returns>
        ///     The items that were removed.
        /// </returns>
        IEnumerable<TEntity> Remove(IEnumerable<TEntity> items);
    }
}
