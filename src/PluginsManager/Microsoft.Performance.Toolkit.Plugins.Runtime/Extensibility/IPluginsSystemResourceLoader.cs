// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.

using System.Collections.Generic;

namespace Microsoft.Performance.Toolkit.Plugins.Runtime.Extensibility
{
    /// <summary>
    ///    Represents a loader that supports loading plugins manager resources of type <typeparamref name="T"/>
    ///    from a collection of <see cref="PluginsSystemResourceReference"/> provided by the <see cref="IPluginsSystemResourceDirectoryLoader"/>,
    ///    or from this loader itself.
    /// </summary>
    /// <typeparam name="T">
    ///     The type of the resources loaded by this loader.
    /// </typeparam>
    public interface IPluginsManagerResourceLoader<T>
        : IPluginsManagerResourcesReferenceConsumer
    {
        /// <summary>
        ///     Add a new item.
        /// </summary>
        /// <param name="item">
        ///     The item to add.
        /// </param>
        void Add(T item);

        /// <summary>
        ///     Add a collection of new items.
        /// </summary>
        /// <param name="items">
        ///     The items to add.
        /// </param>
        void Add(IEnumerable<T> items);
    }
}
