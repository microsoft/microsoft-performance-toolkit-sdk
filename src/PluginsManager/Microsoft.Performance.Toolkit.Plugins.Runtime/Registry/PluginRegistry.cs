// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.

using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text.Json;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.Performance.SDK;
using Microsoft.Performance.SDK.Processing;
using Microsoft.Performance.SDK.Runtime;
using Microsoft.Performance.Toolkit.Plugins.Core.Serialization;
using Microsoft.Performance.Toolkit.Plugins.Runtime.Exceptions;

namespace Microsoft.Performance.Toolkit.Plugins.Runtime
{
    /// <summary>
    ///     Contains a registry file that records information of installed plugins 
    ///     and an ephemeral lock file that indicates whether a process is currently interacting with the registry.
    ///     Only one version of a same plugin may be be registered at a time 
    /// </summary>
    public sealed class PluginRegistry
        : ISynchronizedObject
    {
        private static readonly string registryFileName = "installedPlugins.json";
        private static readonly string lockFileName = ".lockRegistry";
        private static readonly string backupRegistryFileExtension = ".bak";

        private readonly string registryFilePath;
        private readonly string lockFilePath;
        private readonly string backupRegistryFilePath;
        private readonly ILogger logger;

        private static readonly TimeSpan sleepDuration = TimeSpan.FromMilliseconds(500);
        private string lockToken;

        /// <summary>
        ///     Creates an instance of <see cref="PluginRegistry"/> with a registry root.
        /// </summary>
        /// <param name="registryRoot">
        ///     The root directory of the registry.
        /// </param>
        public PluginRegistry(string registryRoot)
            : this(registryRoot, Logger.Create(typeof(PluginRegistry)))
        {         
        }

        /// <summary>
        ///     Creates an instance of <see cref="PluginRegistry"/> with a registry root and a logger.
        /// </summary>
        /// <param name="registryRoot">
        ///     The root directory of the registry.
        /// </param>
        /// <param name="logger">
        ///     Used to log information.
        /// </param>
        /// <exception cref="ArgumentException">
        ///     Throws when <paramref name="registryRoot"/> is not a rooted path.
        /// </exception>
        /// <exception cref="ArgumentNullException">
        ///    Throws when <paramref name="registryRoot"/> or <paramref name="logger"/> is null.
        /// </exception>  
        public PluginRegistry(string registryRoot, ILogger logger)
        {
            Guard.NotNull(registryRoot, nameof(registryRoot));
            Guard.NotNull(logger, nameof(logger));

            if (!Path.IsPathRooted(registryRoot))
            {
                throw new ArgumentException("Registry root must be a rooted path.");
            }

            // TODO: #256 Create a backup registry
            this.RegistryRoot = registryRoot;
            this.registryFilePath = Path.Combine(registryRoot, registryFileName);
            this.backupRegistryFilePath = this.registryFilePath + backupRegistryFileExtension;
            this.lockFilePath = Path.Combine(registryRoot, lockFileName);
            this.logger = logger;
        }

        /// <summary>
        ///     Gets the root directory of the registry.
        /// </summary>
        public string RegistryRoot { get; }

        /// <summary>
        ///     Gets all installed plugin records from the registry;
        /// </summary>
        /// <returns>
        ///     A collection of <see cref="InstalledPluginInfo"/> objects.
        /// </returns>
        /// <exception cref="PluginRegistryCorruptedException">
        ///     Throws when the plugin registry is in an invalid state.
        /// </exception>
        /// <exception cref="PluginRegistryReadWriteException">
        ///     Throws when the plugin registry cannot be read or written.
        /// </exception>
        public async Task<IReadOnlyCollection<InstalledPluginInfo>> GetAllInstalledPlugins()
        {
            List<InstalledPluginInfo> installedPlugins = await ReadInstalledPlugins();
            return installedPlugins.AsReadOnly();
        }

        /// <summary>
        ///     Checks if any plugin with the given ID has been installed to the plugin registry
        ///     and returns the matching record.
        /// </summary>
        /// <param name="pluginId">
        ///     A plugin identifier.
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
        public async Task<InstalledPluginInfo> TryGetInstalledPluginByIdAsync(string pluginId)
        {
            Guard.NotNull(pluginId, nameof(pluginId));

            List<InstalledPluginInfo> installedPlugins = await ReadInstalledPlugins();
            try
            {
                return installedPlugins.SingleOrDefault(
                    plugin => plugin.Id.Equals(pluginId, StringComparison.OrdinalIgnoreCase));
            }
            catch
            {
                LogDuplicatedRegistered(pluginId);
                throw CreateDuplicatedRegisteredException(pluginId);
            }       
        }

        /// <summary>
        ///     Verifies the given installed plugin info matches the record in the plugin registry.
        /// </summary>
        /// <param name="installedPluginInfo">
        ///     The plugin info to check for.
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
        public async Task<bool> IsPluginRegisteredAsync(InstalledPluginInfo pluginInfo)
        {
            List<InstalledPluginInfo> installedPlugins = await ReadInstalledPlugins();

            int installedCount = installedPlugins.Count(p => p.Equals(pluginInfo));
            if (installedCount > 1)
            {
                LogDuplicatedRegistered(pluginInfo.Id);
                throw CreateDuplicatedRegisteredException(pluginInfo.Id);
            }

            return installedCount == 1;
        }

        /// <summary>
        ///     Registers the given plugin to the plugin registry.
        /// </summary>
        /// <param name="plugin">
        ///     The plugin to register.
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
        ///    Throws when the plugin registry cannot be read or written.
        /// </exception>
        /// <exception cref="ArgumentNullException">
        ///    Throws when <paramref name="plugin"/> is null.
        /// </exception>
        public async Task RegisterPluginAsync(InstalledPluginInfo plugin)
        {
            Guard.NotNull(plugin, nameof(plugin));

            List<InstalledPluginInfo> installedPlugins = await ReadInstalledPlugins();
            
            ThrowIfAlreadyRegistered(installedPlugins, plugin);
            installedPlugins.Add(plugin);

            await WriteInstalledPlugins(installedPlugins);
        }

        /// <summary>
        ///    Unregisters the given plugin from the plugin registry.
        /// </summary>
        /// <param name="plugin">
        ///     The plugin to unregister.
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
        public async Task UnregisterPluginAsync(InstalledPluginInfo plugin)
        {
            Guard.NotNull(plugin, nameof(plugin));

            List<InstalledPluginInfo> installedPlugins = await ReadInstalledPlugins();

            ThrowIfNotRegistered(installedPlugins, plugin);
            installedPlugins.RemoveAll(p => p.Equals(plugin));

            await WriteInstalledPlugins(installedPlugins);
        }

        /// <summary>
        ///    Updates the given plugin with the given updated plugin.
        /// </summary>
        /// <param name="currentPlugin">
        ///     The plugin to update.
        /// </param>
        /// <param name="updatedPlugin">
        ///     The updated plugin.
        /// </param>
        /// <returns>
        ///     An awaitable task.
        /// </returns>
        /// <exception cref="InvalidOperationException">
        ///     Throws when <paramref name="currentPlugin"/> is not registered or <paramref name="updatedPlugin"/> is already registered.
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
        public async Task UpdatePluginAsync(InstalledPluginInfo currentPlugin, InstalledPluginInfo updatedPlugin)
        {
            Guard.NotNull(currentPlugin, nameof(currentPlugin));
            Guard.NotNull(updatedPlugin, nameof(updatedPlugin));

            List<InstalledPluginInfo> installedPlugins = await ReadInstalledPlugins();

            ThrowIfNotRegistered(installedPlugins, currentPlugin);
            ThrowIfAlreadyRegistered(installedPlugins, updatedPlugin);

            installedPlugins.RemoveAll(p => p.Equals(currentPlugin));
            installedPlugins.Add(updatedPlugin);

            await WriteInstalledPlugins(installedPlugins);
        }

        // TODO: Might want to change this to a file lock that supports read lock.
        /// <summary>
        ///    Acquires a lock on the plugin registry.  This is used to prevent multiple processes from reading and writing to the plugin registry at the same time.
        /// </summary>
        /// <param name="cancellationToken">
        ///    Signals that the caller wishes to cancel the operation.
        /// </param>
        /// <returns>
        ///     An awaitable task.
        /// </returns>
        public async Task AcquireLock(CancellationToken cancellationToken)
        {
            string lockToken = Guid.NewGuid().ToString();

            Retry:
            while (File.Exists(this.lockFilePath))
            {
                await Task.Delay(sleepDuration, cancellationToken).ConfigureAwait(false);
            }

            Directory.CreateDirectory(this.RegistryRoot);
            try
            {
                File.WriteAllText(this.lockFilePath, lockToken);
                string readToken = File.ReadAllText(this.lockFilePath);
                if (readToken != lockToken)
                {
                    goto Retry;
                }
            }
            catch (IOException)
            {
                goto Retry;
            }

            this.lockToken = lockToken;
        }

        /// <summary>
        ///     Releases the lock on the plugin registry.
        /// </summary>
        public void ReleaseLock()
        {
            if (this.lockToken == null || !File.Exists(this.lockFilePath))
            {
                return;
            }
            try
            {
                string token = File.ReadAllText(this.lockFilePath);
                if (token == this.lockToken)
                {
                    File.Delete(this.lockFilePath);
                }
            }
            catch (IOException e)
            {
                if (e is FileNotFoundException || e is DirectoryNotFoundException)
                {
                    return;
                }

                throw;
            }

            this.lockToken = null;
        }

        private async Task<List<InstalledPluginInfo>> ReadInstalledPlugins()
        {
            if (!File.Exists(this.registryFilePath))
            {
                return new List<InstalledPluginInfo>();
            }

            try
            {
                using (var fileStream = new FileStream(this.registryFilePath, FileMode.Open, FileAccess.Read, FileShare.Read))
                {
                    return await JsonSerializer.DeserializeAsync<List<InstalledPluginInfo>>(
                        fileStream,
                        SerializationConfig.PluginsManagerSerializerDefaultOptions);
                }
            }
            catch (Exception e)
            {
                if (e is JsonException)
                {
                    this.logger.Error(e, $"Deserialization of plugin registry failed due to invalid JSON text.");
                }

                string errorMsg = $"Failed to read from plugin registry file.";
                this.logger.Error(e, errorMsg);
                throw new PluginRegistryReadWriteException(errorMsg, this.registryFilePath, e);
            }
        }

        private async Task WriteInstalledPlugins(IEnumerable<InstalledPluginInfo> installedPlugins)
        {
            Guard.NotNull(installedPlugins, nameof(installedPlugins));

            Directory.CreateDirectory(this.RegistryRoot);

            try
            {
                using (var fileStream = new FileStream(this.registryFilePath, FileMode.Create, FileAccess.Write))
                {
                    await JsonSerializer.SerializeAsync(
                        fileStream,
                        installedPlugins,
                        SerializationConfig.PluginsManagerSerializerDefaultOptions);
                }
            }
            catch (Exception e)
            {
                string errorMsg = $"Failed to write to plugin registry file.";
                this.logger.Error(e, errorMsg);
                throw new PluginRegistryReadWriteException(errorMsg, this.registryFilePath, e);
            }
        }

        private void ThrowIfAlreadyRegistered(
            IEnumerable<InstalledPluginInfo> installedPluginInfos,
            InstalledPluginInfo pluginToInstall)
        {
            int installedCount = installedPluginInfos.Count(p => p.Id.Equals(pluginToInstall.Id));
            if (installedCount >= 1)
            {
                if (installedCount > 1)
                {
                    LogDuplicatedRegistered(pluginToInstall.Id);
                }

                throw new InvalidOperationException($"Failed to register plugin {pluginToInstall} as it is already registered.");
            }
        }

        private void ThrowIfNotRegistered(
            IEnumerable<InstalledPluginInfo> installedPluginInfos,
            InstalledPluginInfo pluginToRemove)
        {
            int installedCount = installedPluginInfos.Count(p => p.Equals(pluginToRemove));
            if (installedCount == 0)
            {
                throw new InvalidOperationException($"Failed to unregister plugin {pluginToRemove} as it is not currently registered.");
            }

            if (installedCount > 1)
            {
                LogDuplicatedRegistered(pluginToRemove.Id);
            }
        }

        private PluginRegistryCorruptedException CreateDuplicatedRegisteredException(string pluginId)
        {
            return new PluginRegistryCorruptedException
                    ($"Duplicated records of plugin {pluginId} found in the plugin registry");
        }

        private void LogDuplicatedRegistered(string pluginId)
        {
            this.logger.Error($"Duplicated records of plugin {pluginId} found in the plugin registry.");
        }
    }
}
