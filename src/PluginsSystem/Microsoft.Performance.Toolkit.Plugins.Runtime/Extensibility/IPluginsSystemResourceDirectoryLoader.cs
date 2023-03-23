// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.

namespace Microsoft.Performance.Toolkit.Plugins.Runtime.Extensibility
{
    /// <summary>
    ///     Represents a loader that facilitates the loading of <see cref="PluginsSystemResourceReference"/>s 
    ///     from a director to its consumers.
    /// </summary>
    public interface IPluginsSystemResourceDirectoryLoader
    {
        /// <summary>
        ///     Tries to load <see cref="PluginsSystemResourceReference"/>s from the given directory
        ///     to the subscribed <see cref="IPluginsSystemResourcesReferenceConsumer">s.
        /// <param name="directory">
        ///     The directoty to load resouces from.
        /// </param>
        /// <returns>
        ///     <c>true</c> if all resources are successfully loaded from the directory. <c>false</c>
        ///     otherwise.
        /// </returns>
        bool TryLoad(string directory);

        /// <summary>
        ///     Registers a <see cref="IPluginsSystemResourcesReferenceConsumer}"/> to receive all future loaded resources
        ///     and sends all previously loaded plugins to its <see cref="IPluginsSystemResourcesReferenceConsumer.OnNewResourcesLoaded(
        ///     System.Collections.Generic.IEnumerable{PluginsSystemResourceReference})"/> handler.
        /// </summary>
        /// <param name="consumer">
        ///     The <see cref="IPluginsSystemResourcesReferenceConsumer"/> to be registered to this loader.
        /// </param>
        /// <returns>
        ///     Whether or not the <paramref name="consumer"/> is successfully subscribed.
        /// </returns>
        bool Subscribe(IPluginsSystemResourcesReferenceConsumer consumer);

        /// <summary>
        ///     Unregisters a <see cref="IPluginsSystemResourcesReferenceConsumer"/> from receiving future loaded resources.
        /// </summary>
        /// <param name="consumer">
        ///     The <see cref="IPluginsSystemResourcesReferenceConsumer"/> to be unregistered from this loader.
        /// </param>
        /// <returns>
        ///     Whether or not the <paramref name="consumer"/> is successfully unsubscribed.
        /// </returns>
        bool Unsubscribe(IPluginsSystemResourcesReferenceConsumer consumer);
    }
}
