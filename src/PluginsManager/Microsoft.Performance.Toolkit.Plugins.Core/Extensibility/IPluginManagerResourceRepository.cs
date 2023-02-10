// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.

using System;
using System.Collections.Generic;

namespace Microsoft.Performance.Toolkit.Plugins.Core.Extensibility
{
    /// <summary>
    ///     Represents a repository for storing a collection of <see cref=" IPluginManagerResource"/>.
    /// </summary>
    /// <typeparam name="T">
    ///     The type of the resources stored in this repository.
    /// </typeparam>
    public interface IPluginManagerResourceRepository<T>
        where T : class, IPluginManagerResource
    {
        /// <summary>
        ///     Gets all plugin resources contained in this repository.
        /// </summary>
        IEnumerable<T> PluginResources { get; }

        /// <summary>
        ///     Adds new resources and notifies when they are added to the repository.
        /// </summary>
        /// <param name="loadedResources">
        ///     The newly loaded resources.
        /// </param>
        void OnNewResourcesLoaded(IEnumerable<T> loadedResources);

        /// <summary>
        ///    Raised when new resources are added to this repository.
        /// </summary>
        event EventHandler<NewResourcesEventArgs<T>> ResourcesAdded;
    }
}
