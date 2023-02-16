// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.

using System;
using System.Collections.Generic;
using Microsoft.Performance.Toolkit.Plugins.Core.Extensibility;

namespace Microsoft.Performance.Toolkit.Plugins.Runtime.Extensibility
{
    /// <summary>
    ///     Represents a repository for storing a collection of <see cref=" IPluginManagerResource"/> of type <typeparamref name="T"/>.
    /// </summary>
    /// <typeparam name="T">
    ///     The type of the resources stored in this repository.
    /// </typeparam>
    public interface IPluginManagerResourceRepository<T>
        where T : IPluginManagerResource
    {
        /// <summary>
        ///     Gets all plugins manager resources contained in this repository.
        /// </summary>
        IEnumerable<T> Resources { get; }

        /// <summary>
        ///    Raised when new resources are added to this repository.
        /// </summary>
        event EventHandler<NewResourcesEventArgs<T>> ResourcesAdded;
    }
}
