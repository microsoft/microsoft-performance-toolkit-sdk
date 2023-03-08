﻿// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.

using Microsoft.Performance.Toolkit.Plugins.Core.Extensibility;

namespace Microsoft.Performance.Toolkit.Plugins.Runtime.Extensibility
{
    /// <summary>
    ///     Represents a loader that facilitates the loading of <see cref="IPluginsManagerResource"/>s.
    ///     to its consumers.
    /// </summary>
    public interface IPluginsManagerResourceLoader
    {
        /// <summary>
        ///     Tries to load <see cref="IPluginsManagerResource"/>s from the given directory
        ///     to the subscribed <see cref="IPluginsManagerResourcesConsumer">s.
        /// <param name="directory">
        ///     The directoty to load resouces from.
        /// </param>
        /// <returns>
        ///     <c>true</c> if all resources are successfully loaded from the directory. <c>false</c>
        ///     otherwise.
        /// </returns>
        bool TryLoad(string directory);

        /// <summary>
        ///     Registers a <see cref="IPluginsManagerResourcesConsumer}"/> to receive all future loaded resources
        ///     and sends all previously loaded plugins to its <see cref="IPluginsManagerResourcesConsumer.OnNewResourcesLoaded(
        ///     System.Collections.Generic.IEnumerable{SDK.Runtime.PluginsManagerResourceReference})"/> handler.
        /// </summary>
        /// <param name="consumer">
        ///     The <see cref="IPluginsManagerResourcesConsumer"/> to be registered to this loader.
        /// </param>
        /// <returns>
        ///     Whether or not the <paramref name="consumer"/> is successfully subscribed.
        /// </returns>
        bool Subscribe(IPluginsManagerResourcesConsumer consumer);

        /// <summary>
        ///     Unregisters a <see cref="IPluginsManagerResourcesConsumer"/> from receiving future loaded resources.
        /// </summary>
        /// <param name="consumer">
        ///     The <see cref="IPluginsManagerResourcesConsumer"/> to be unregistered from this loader.
        /// </param>
        /// <returns>
        ///     Whether or not the <paramref name="consumer"/> is successfully unsubscribed.
        /// </returns>
        bool Unsubscribe(IPluginsManagerResourcesConsumer consumer);
    }
}