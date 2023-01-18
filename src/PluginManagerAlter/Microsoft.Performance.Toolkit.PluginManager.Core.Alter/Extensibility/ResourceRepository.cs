// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.

using System;
using System.Collections.Generic;
using System.Linq;

namespace Microsoft.Performance.Toolkit.PluginManager.Core.Alter.Extensibility
{
    /// <summary>
    ///     Represents a repository for storing a collection of <see cref=" IPluginManagerResource"/>.
    /// </summary>
    /// <typeparam name="T">
    ///     The type of the resources stored in this repository.
    /// </typeparam>
    public class ResourceRepository<T> where T : IPluginManagerResource
    {
        private readonly HashSet<T> resources;

        /// <summary>
        ///     Intializes a <see cref="ResourceRepository{T}"/> with an existing collection of <paramref name="resources"/>.
        /// </summary>
        /// <param name="resources">
        ///     A collection of <see cref="IPluginManagerResource"s of type <see cref="T"/>.
        /// </param>
        public ResourceRepository(IEnumerable<T> resources)
        {
            this.resources = new HashSet<T>(resources);
        }

        public IEnumerable<T> PluginResources
        {
            get
            {
                return this.resources;
            }
        }

        /// <summary>
        ///     Perform proper operations upon new resources loaded.
        /// </summary>
        /// <param name="loadedResources"
        ///     The newly loaded resources.
        /// ></param>
        public void OnNewResourcesLoaded(IEnumerable<T> loadedResources)
        {
            lock (this.resources)
            {
                IEnumerable<T> newResources = loadedResources.Except(this.resources);
                this.resources.UnionWith(newResources);

                if (newResources.Any())
                {
                    ResourcesAdded?.Invoke(this, new NewResourcesEventArgs<T>(newResources));
                }
            }
        }

        /// <summary>
        ///    Raised when new resources are added to this repository.
        /// </summary>
        public event EventHandler<NewResourcesEventArgs<T>> ResourcesAdded;
    }
}
