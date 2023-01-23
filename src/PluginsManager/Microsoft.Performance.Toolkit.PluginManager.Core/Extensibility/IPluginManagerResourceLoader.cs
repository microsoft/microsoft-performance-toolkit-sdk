// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.

namespace Microsoft.Performance.Toolkit.PluginManager.Core.Extensibility
{
    /// <summary>
    ///     Represents a loader that facilitates the loading of <see cref="IPluginManagerResource"/>s.
    /// </summary>
    public interface IPluginManagerResourceLoader
    {
        /// <summary>
        ///     Tries to load <see cref="IPluginManagerResource"/>s from the given directory
        ///     to the subscribed <see cref="ResourceRepository{T}">s.
        /// <param name="directory">
        ///     The directoty to load resouces from.
        /// </param>
        /// <returns>
        ///     <c>true</c> if all resources are successfully loaded from the directory. <c>false</c>
        ///     otherwise.
        /// </returns>
        bool TryLoad(string directory);

        /// <summary>
        ///     Registers a <see cref="ResourceRepository{T}"/> to receive all future loaded resources.
        /// </summary>
        /// <typeparam name="T">
        ///     The type of the resources stored in the given <paramref name="resourceRepository"/>.
        /// </typeparam>
        /// <param name="resourceRepository">
        ///     The <see cref="ResourceRepository{T}"/> to be registered to this loader.
        /// </param>
        /// <returns>
        ///     Whether or not the <paramref name="resourceRepository"/> is successfully subscribed.
        /// </returns>
        bool Subscribe<T>(ResourceRepository<T> resourceRepository) where T : IPluginManagerResource;

        /// <summary>
        ///     Unregisters a <see cref="ResourceRepository{T}"/> from receiving future loaded resources.
        /// </summary>
        /// <typeparam name="T">
        ///     The type of the resources stored in the given <paramref name="resourceRepository"/>.
        /// </typeparam>
        /// <param name="resourceRepository">
        ///     The <see cref="ResourceRepository{T}"/> to be unregistered from this loader.
        /// </param>
        /// <returns>
        ///     Whether or not the <paramref name="resourceRepository"/> is successfully unsubscribed.
        /// </returns>
        bool Unsubscribe<T>(ResourceRepository<T> resourceRepository) where T: IPluginManagerResource;
    }
}
