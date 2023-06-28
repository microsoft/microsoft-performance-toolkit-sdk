// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.

using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using System.Threading;

namespace Microsoft.Performance.Toolkit.Plugins.Runtime.Common
{
    /// <summary>
    ///     Represents a repository of <typeparamref name="TEntity"/> objects.
    /// </summary>
    /// <typeparam name="TEntity">
    ///     The type of entity stored in the repository.
    /// </typeparam>
    /// <typeparam name="TIdentifier">
    ///     The type of the identifier used to retrieve <typeparamref name="TEntity"/> objects.
    /// </typeparam>
    public interface IKeyedRepository<TEntity, TIdentifier>
    {
        /// <summary>
        ///     Gets all <typeparamref name="TEntity"/> objects in the repository.
        /// </summary>
        /// <param name="cancellationToken">
        ///     Signals that the caller wishes to cancel the operation.
        /// </param>
        /// <returns>
        ///     A collection of <see cref="TEntity"/> objects.
        /// </returns>
        /// <exception cref="Exceptions.RepositoryCorruptedException">
        ///     Throws when the repository is in an invalid state.
        /// </exception>
        /// <exception cref="Exceptions.RepositoryDataAccessException">
        ///     Throws when the repository cannot be read or written.
        /// </exception>
        Task<IReadOnlyCollection<TEntity>> GetAllAsync(CancellationToken cancellationToken);

        /// <summary>
        ///     Gets the <typeparamref name="TEntity"/> with the given identifier. If no matching record is found, returns null.
        /// </summary>
        /// <param name="Id">
        ///     The identifier of the <see cref="TEntity"/> to retrieve.
        /// </param>
        /// <param name="cancellationToken">
        ///     Signals that the caller wishes to cancel the operation.
        /// </param>
        /// <returns>
        ///     The matching <see cref="TEntity"/> or <c>null</c> if no matching record found.
        /// </returns>
        /// <exception cref="Exceptions.RepositoryCorruptedException">
        ///     Throws when the repository is in an invalid state.
        /// </exception>
        /// <exception cref="Exceptions.RepositoryDataAccessException">
        ///     Throws when the repository cannot be read or written.
        /// </exception>
        /// <exception cref="ArgumentNullException">
        ///     Throws when <paramref name="Id"/> is null.
        /// </exception>
        Task<TEntity> TryGetByIdAsync(TIdentifier Id, CancellationToken cancellationToken);

        /// <summary>
        ///     Checks if the given <typeparamref name="TEntity"/> exists in the repository.
        /// </summary>
        /// <param name="entity">
        ///     The <see cref="TEntity"/> to check for.
        /// </param>
        /// <param name="cancellationToken">
        ///     Signals that the caller wishes to cancel the operation.
        /// </param>
        /// <returns>
        ///     <c>true</c> if the <see cref="TEntity"/> exists in the repository, <c>false</c> otherwise.
        /// </returns>
        /// <exception cref="Exceptions.RepositoryCorruptedException">
        ///     Throws when the repository is in an invalid state.
        /// </exception>
        /// <exception cref="Exceptions.RepositoryDataAccessException">
        ///     Throws when the repository cannot be read or written.
        /// </exception>
        /// <exception cref="ArgumentNullException">
        ///     Throws when <paramref name="entity"/> is null.
        /// </exception>
        Task<bool> ExistsAsync(TEntity entity, CancellationToken cancellationToken);

        /// <summary>
        ///     Adds a new entity to the repository.
        /// </summary>
        /// <param name="installedPluginInfo">
        ///     The entity to add.
        /// </param>
        /// <param name="cancellationToken">
        ///     Signals that the caller wishes to cancel the operation.
        /// </param>
        /// <returns>
        ///     An awaitable task that completes when the entity has been added.
        /// </returns>
        /// <exception cref="InvalidOperationException">
        ///     Throws when an entity with the same identifier already exists in the repository.
        /// </exception>
        /// <exception cref="Exceptions.RepositoryCorruptedException">
        ///     Throws when the repository is in an invalid state.
        /// </exception>
        /// <exception cref="Exceptions.RepositoryDataAccessException">
        ///     Throws when the repository cannot be read or written.
        /// </exception>
        /// <exception cref="ArgumentNullException">
        ///     Throws when <paramref name="installedPluginInfo"/> is null.
        /// </exception>
        Task AddAsync(TEntity installedPluginInfo, CancellationToken cancellationToken);

        /// <summary>
        ///     Deletes an existing entity in the repository.
        /// </summary>
        /// <param name="entity">
        ///     The entity to delete.
        /// </param>
        /// <param name="cancellationToken">
        ///     Signals that the caller wishes to cancel the operation.
        /// </param>
        /// <returns>
        ///     An awaitable task that completes when the entity has been deleted.
        /// </returns>
        /// <exception cref="InvalidOperationException">
        ///     Throws when the entity does not exist in the repository.
        /// </exception>
        /// <exception cref="Exceptions.RepositoryCorruptedException">
        ///     Throws when the repository is in an invalid state.
        /// </exception>
        /// <exception cref="Exceptions.RepositoryDataAccessException">
        ///     Throws when the repository cannot be read or written.
        /// </exception>
        Task DeleteAsync(TEntity entity, CancellationToken cancellationToken);

        /// <summary>
        ///     Updates an existing entity in the repository.
        /// </summary>
        /// <param name="currentEntity">
        ///     The entity to update.
        /// </param>
        /// <param name="updatedEntity">
        ///     The updated entity.
        /// </param>
        /// <param name="cancellationToken">
        ///     Signals that the caller wishes to cancel the operation.
        /// </param>
        /// <returns>
        ///     An awaitable task that completes when the entity has been updated.
        /// </returns>
        /// <exception cref="InvalidOperationException">
        ///     Throws when <paramref name="currentEntity"/> doesn't exist or <paramref name="updatedEntity"/> already exists,
        ///     or when <paramref name="currentEntity"/> and <paramref name="updatedEntity"/> don't have the same identifier.
        /// </exception>
        /// <exception cref="Exceptions.RepositoryCorruptedException">
        ///     Throws when the repository is in an invalid state.
        /// </exception>
        /// <exception cref="Exceptions.RepositoryDataAccessException">
        ///     Throws when the repository cannot be read or written.
        /// </exception>
        /// <exception cref="ArgumentNullException">
        ///     Throws when <paramref name="currentEntity"/> or <paramref name="updatedEntity"/> is null.
        /// </exception>
        Task UpdateAsync(TEntity currentEntity, TEntity updatedEntity, CancellationToken cancellationToken);
    }
}
