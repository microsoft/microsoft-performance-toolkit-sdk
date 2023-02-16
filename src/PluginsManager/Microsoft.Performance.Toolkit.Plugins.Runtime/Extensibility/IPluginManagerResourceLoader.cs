// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.

using Microsoft.Performance.Toolkit.Plugins.Core.Extensibility;

namespace Microsoft.Performance.Toolkit.Plugins.Runtime.Extensibility
{
    /// <summary>
    ///     Represents a loader that facilitates the loading of <see cref="IPluginManagerResource"/>s.
    ///     to its consumers.
    /// </summary>
    public interface IPluginManagerResourceLoader
    {
        /// <summary>
        ///     Tries to load <see cref="IPluginManagerResource"/>s from the given directory
        ///     to the subscribed <see cref="IPluginManagerResourcesConsumer">s.
        /// <param name="directory">
        ///     The directoty to load resouces from.
        /// </param>
        /// <returns>
        ///     <c>true</c> if all resources are successfully loaded from the directory. <c>false</c>
        ///     otherwise.
        /// </returns>
        bool TryLoad(string directory);

        /// <summary>
        ///     Registers a <see cref="IPluginManagerResourcesConsumer}"/> to receive all future loaded resources
        ///     and sends all previously loaded plugins to its <see cref="IPluginManagerResourcesConsumer.OnNewResourcesLoaded(
        ///     System.Collections.Generic.IEnumerable{SDK.Runtime.PluginManagerResourceReference})"/> handler.
        /// </summary>
        /// <param name="consumer">
        ///     The <see cref="IPluginManagerResourcesConsumer"/> to be registered to this loader.
        /// </param>
        /// <returns>
        ///     Whether or not the <paramref name="consumer"/> is successfully subscribed.
        /// </returns>
        bool Subscribe(IPluginManagerResourcesConsumer consumer);

        /// <summary>
        ///     Unregisters a <see cref="IPluginManagerResourcesConsumer"/> from receiving future loaded resources.
        /// </summary>
        /// <param name="consumer">
        ///     The <see cref="IPluginManagerResourcesConsumer"/> to be unregistered from this loader.
        /// </param>
        /// <returns>
        ///     Whether or not the <paramref name="consumer"/> is successfully unsubscribed.
        /// </returns>
        bool Unsubscribe(IPluginManagerResourcesConsumer consumer);
    }
}
