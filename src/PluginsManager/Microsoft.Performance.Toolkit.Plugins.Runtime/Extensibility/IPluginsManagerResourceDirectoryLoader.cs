// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.

using Microsoft.Performance.Toolkit.Plugins.Core.Extensibility;

namespace Microsoft.Performance.Toolkit.Plugins.Runtime.Extensibility
{
    /// <summary>
    ///     Represents a loader that facilitates the loading of <see cref="IPluginsManagerResource"/>s.
    ///     to its consumers.
    /// </summary>
    public interface IPluginsManagerResourceDirectoryLoader
    {
        /// <summary>
        ///     Tries to load <see cref="IPluginsManagerResource"/>s from the given directory
        ///     to the subscribed <see cref="IPluginsManagerResourcesReferenceConsumer">s.
        /// <param name="directory">
        ///     The directoty to load resouces from.
        /// </param>
        /// <returns>
        ///     <c>true</c> if all resources are successfully loaded from the directory. <c>false</c>
        ///     otherwise.
        /// </returns>
        bool TryLoad(string directory);

        /// <summary>
        ///     Registers a <see cref="IPluginsManagerResourcesReferenceConsumer}"/> to receive all future loaded resources
        ///     and sends all previously loaded plugins to its <see cref="IPluginsManagerResourcesReferenceConsumer.OnNewResourcesLoaded(
        ///     System.Collections.Generic.IEnumerable{SDK.Runtime.PluginsManagerResourceReference})"/> handler.
        /// </summary>
        /// <param name="consumer">
        ///     The <see cref="IPluginsManagerResourcesReferenceConsumer"/> to be registered to this loader.
        /// </param>
        /// <returns>
        ///     Whether or not the <paramref name="consumer"/> is successfully subscribed.
        /// </returns>
        bool Subscribe(IPluginsManagerResourcesReferenceConsumer consumer);

        /// <summary>
        ///     Unregisters a <see cref="IPluginsManagerResourcesReferenceConsumer"/> from receiving future loaded resources.
        /// </summary>
        /// <param name="consumer">
        ///     The <see cref="IPluginsManagerResourcesReferenceConsumer"/> to be unregistered from this loader.
        /// </param>
        /// <returns>
        ///     Whether or not the <paramref name="consumer"/> is successfully unsubscribed.
        /// </returns>
        bool Unsubscribe(IPluginsManagerResourcesReferenceConsumer consumer);
    }
}
