// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.

using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.Performance.Toolkit.Plugins.Runtime.Exceptions;

namespace Microsoft.Performance.Toolkit.Plugins.Runtime
{
    /// <summary>
    ///     Represents a registry that stores information about installed plugins and supports
    ///     registering, unregistering and updating plugins.
    /// </summary>
    public interface IPluginRegistry
        : ISynchronizedObject
    {
        /// <summary>
        ///     Gets the root directory of the registry.
        /// </summary>
        string RegistryRoot { get; }

        /// <summary>
        ///     Gets all installed plugin records from the registry;
        /// </summary>
        /// <param name="cancellationToken">
        ///     Signals that the caller wishes to cancel the operation.
        /// </param>
        /// <returns>
        ///     A collection of <see cref="InstalledPluginInfo"/> objects.
        /// </returns>
        /// <exception cref="PluginRegistryCorruptedException">
        ///     Throws when the plugin registry is in an invalid state.
        /// </exception>
        /// <exception cref="PluginRegistryReadWriteException">
        ///     Throws when the plugin registry cannot be read or written.
        /// </exception>
        Task<IReadOnlyCollection<InstalledPluginInfo>> GetAllInstalledPlugins(CancellationToken cancellationToken);

        /// <summary>
        ///     Checks if any plugin with the given ID has been installed to the plugin registry
        ///     and returns the matching record.
        /// </summary>
        /// <param name="pluginId">
        ///     A plugin identifier.PluginsManager\Microsoft.Performance.Toolkit.Plugins.Runtime\Registry
        /// </param>
        /// <param name="cancellationToken">
        ///     Signals that the caller wishes to cancel the operation.
        /// </param>
        /// <returns>
        ///     The matching <see cref="InstalledPluginInfo"/> or <c>null</c> if no matching record found.
        /// </returns>
        /// <exception cref="PluginRegistryCorruptedException">
        ///     Throws when the plugin registry is in an invalid state.
        /// </exception>
        /// <exception cref="PluginRegistryReadWriteException">
        ///     Throws when the plugin registry cannot be read or written.
        /// </exception>
        /// <exception cref="ArgumentNullException">
        ///     Throws when <paramref name="pluginId"/> is null.
        /// </exception>
        Task<InstalledPluginInfo> TryGetInstalledPluginByIdAsync(string pluginId, CancellationToken cancellationToken);

        /// <summary>
        ///     Verifies the given installed plugin info matches the record in the plugin registry.
        /// </summary>
        /// <param name="plugin">
        ///     The plugin info to check for.
        /// </param>
        /// <param name="cancellationToken">
        ///     Signals that the caller wishes to cancel the operation.
        /// </param>    
        /// <returns>
        ///     <c>true</c> if the installed plugin matches the record in the plugin registry. <c>false</c> otherwise.
        /// </returns>
        /// <exception cref="PluginRegistryCorruptedException">
        ///     Throws when the plugin registry is in an invalid state.
        /// </exception>
        /// <exception cref="PluginRegistryReadWriteException">
        ///     Throws when the plugin registry cannot be read or written.
        /// </exception>
        /// <exception cref="ArgumentNullException">
        ///     Throws when <paramref name="installedPluginInfo"/> is null.
        /// </exception>
        Task<bool> IsPluginRegisteredAsync(InstalledPluginInfo pluginInfo, CancellationToken cancellationToken);

        /// <summary>
        ///     Registers a new plugin to the plugin registry.
        /// </summary>
        /// <param name="plugin">
        ///     The plugin to register.
        /// </param>
        /// <param name="cancellationToken">
        ///     Signals that the caller wishes to cancel the operation.
        /// </param>
        /// <returns>
        ///     An awaitable task.
        /// </returns>
        /// <exception cref="InvalidOperationException">
        ///     Throws when the plugin is already registered.
        /// </exception>
        /// <exception cref="PluginRegistryCorruptedException">
        ///     Throws when the plugin registry is in an invalid state.
        /// </exception>
        /// <exception cref="PluginRegistryReadWriteException">
        ///     Throws when the plugin registry cannot be read or written.
        /// </exception>
        /// <exception cref="ArgumentNullException">
        ///     Throws when <paramref name="plugin"/> is null.
        /// </exception>
        Task RegisterPluginAsync(InstalledPluginInfo plugin, CancellationToken cancellationToken);

        /// <summary>
        ///     Unregisters an existing plugin from the plugin registry.
        /// </summary>
        /// <param name="plugin">
        ///     The plugin to unregister.
        /// </param>
        /// <param name="cancellationToken">
        ///     Signals that the caller wishes to cancel the operation.
        /// </param>
        /// <returns>
        ///     An awaitable task.
        /// </returns>
        /// <exception cref="InvalidOperationException">
        ///     Throws when the plugin is not registered.
        /// </exception>
        /// <exception cref="PluginRegistryCorruptedException">
        ///     Throws when the plugin registry is in an invalid state.
        /// </exception>
        /// <exception cref="PluginRegistryReadWriteException">
        ///     Throws when the plugin registry cannot be read or written.
        /// </exception>  
        Task UnregisterPluginAsync(InstalledPluginInfo plugin, CancellationToken cancellationToken);

        /// <summary>
        ///     Updates an registered plugin with a different version of the same plugin.
        /// </summary>
        /// <param name="currentPlugin">
        ///     The currently registered plugin.
        /// </param>
        /// <param name="updatedPlugin">
        ///     The plugin to update the current plugin with.
        /// </param>
        /// <param name="cancellationToken">
        ///     Signals that the caller wishes to cancel the operation.
        /// </param>
        /// <returns>
        ///     An awaitable task.
        /// </returns>
        /// <exception cref="InvalidOperationException">
        ///     Throws when <paramref name="currentPlugin"/> is not registered or <paramref name="updatedPlugin"/> is already registered
        ///     or <paramref name="currentPlugin"/> and <paramref name="updatedPlugin"/> are not the same plugin.
        /// </exception>
        /// <exception cref="PluginRegistryCorruptedException">
        ///     Throws when the plugin registry is in an invalid state.
        /// </exception>
        /// <exception cref="PluginRegistryReadWriteException">
        ///     Throws when the plugin registry cannot be read or written.
        /// </exception>
        /// <exception cref="ArgumentNullException">
        ///     Throws when <paramref name="currentPlugin"/> or <paramref name="updatedPlugin"/> is null.
        /// </exception>   
        Task UpdatePluginAsync(InstalledPluginInfo currentPlugin, InstalledPluginInfo updatedPlugin, CancellationToken cancellationToken);
    }
}
