// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.

using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text.Json;
using System.Threading;
using System.Threading.Tasks;
using Medallion.Threading.FileSystem;
using Microsoft.Performance.SDK;
using Microsoft.Performance.SDK.Processing;
using Microsoft.Performance.SDK.Runtime;
using Microsoft.Performance.Toolkit.Plugins.Core.Serialization;
using Microsoft.Performance.Toolkit.Plugins.Runtime.Exceptions;

namespace Microsoft.Performance.Toolkit.Plugins.Runtime
{
    /// <summary>
    ///     Represents a file system based plugin registry that records information of installed plugins
    ///     and provides an ephemeral lock file that can be used to synchronize access to the registry.
    /// </summary>
    public sealed class FileBackedPluginRegistry
        : IPluginRegistry
    {
        private static readonly string registryFileName = "installedPlugins.json";
        private static readonly string lockFileName = ".lockRegistry";
        private readonly string registryFilePath;
        private readonly FileDistributedLock fileDistributedLock;

        private readonly string registryRoot;
        private readonly ISerializer<List<InstalledPluginInfo>> registryFileSerializer;
        private readonly ILogger logger;

        /// <summary>
        ///     Creates an instance of <see cref="FileBackedPluginRegistry"/> with a registry root.
        /// </summary>
        /// <param name="registryRoot">
        ///     The root directory of the registry.
        /// </param>
        public FileBackedPluginRegistry(string registryRoot)
            : this(registryRoot, SerializationUtils.GetJsonSerializerWithDefaultOptions<List<InstalledPluginInfo>>())
        {
        }

        /// <summary>
        ///     Creates an instance of <see cref="FileBackedPluginRegistry"/> with a registry root and a serializer.
        /// </summary>
        /// <param name="registryRoot">
        ///     The root directory of the registry.
        /// </param>
        /// <param name="serializer">
        ///     Used to serialize and deserialize the registry file.
        /// </param>
        public FileBackedPluginRegistry(
            string registryRoot,
            ISerializer<List<InstalledPluginInfo>> serializer)
            : this(registryRoot, serializer, Logger.Create(typeof(FileBackedPluginRegistry)))
        {
        }

        /// <summary>
        ///     Creates an instance of <see cref="FileBackedPluginRegistry"/> with a registry root and a logger.
        /// </summary>
        /// <param name="registryRoot">
        ///     The root directory of the registry.
        /// </param>
        /// <param name="serializer">
        ///     Used to serialize and deserialize the registry file.
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
        public FileBackedPluginRegistry(
            string registryRoot,
            ISerializer<List<InstalledPluginInfo>> serializer, 
            ILogger logger)
        {
            Guard.NotNull(registryRoot, nameof(registryRoot));
            Guard.NotNull(logger, nameof(logger));

            if (!Path.IsPathRooted(registryRoot))
            {
                throw new ArgumentException("Registry root must be a rooted path.");
            }

            // TODO: #256 Create a backup registry
            this.registryRoot = registryRoot;
            this.registryFilePath = Path.Combine(registryRoot, registryFileName);
            this.registryFileSerializer = serializer;
            this.logger = logger;

            string lockFilePath = Path.Combine(registryRoot, lockFileName);
            this.fileDistributedLock = new FileDistributedLock(new FileInfo(lockFilePath));
        }

        /// <inheritdoc/>
        public async Task<IReadOnlyCollection<InstalledPluginInfo>> GetAllAsync(CancellationToken cancellationToken)
        {
            List<InstalledPluginInfo> installedPlugins = await ReadInstalledPlugins(cancellationToken);
            return installedPlugins.AsReadOnly();
        }

        /// <inheritdoc/>
        public async Task<InstalledPluginInfo> TryGetByIdAsync(
            string pluginId,
            CancellationToken cancellationToken)
        {
            Guard.NotNull(pluginId, nameof(pluginId));

            List<InstalledPluginInfo> installedPlugins = await ReadInstalledPlugins(cancellationToken);
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

        /// <inheritdoc/>
        public async Task<bool> ExistsAsync(
            InstalledPluginInfo plugin,
            CancellationToken cancellationToken)
        {
            List<InstalledPluginInfo> installedPlugins = await ReadInstalledPlugins(cancellationToken);

            int installedCount = installedPlugins.Count(p => p.Equals(plugin));
            if (installedCount > 1)
            {
                LogDuplicatedRegistered(plugin.Id);
                throw CreateDuplicatedRegisteredException(plugin.Id);
            }

            return installedCount == 1;
        }

        /// <inheritdoc/>
        public async Task AddAsync(
            InstalledPluginInfo plugin,
            CancellationToken cancellationToken)
        {
            Guard.NotNull(plugin, nameof(plugin));

            List<InstalledPluginInfo> installedPlugins = await ReadInstalledPlugins(cancellationToken);

            ThrowIfAlreadyRegistered(installedPlugins, plugin);
            installedPlugins.Add(plugin);

            await WriteInstalledPlugins(installedPlugins, cancellationToken);
        }

        /// <inheritdoc/> 
        public async Task DeleteAsync(
            InstalledPluginInfo plugin,
            CancellationToken cancellationToken)
        {
            Guard.NotNull(plugin, nameof(plugin));

            List<InstalledPluginInfo> installedPlugins = await ReadInstalledPlugins(cancellationToken);

            ThrowIfNotRegistered(installedPlugins, plugin);
            installedPlugins.RemoveAll(p => p.Equals(plugin));

            await WriteInstalledPlugins(installedPlugins, cancellationToken);
        }

        /// <inheritdoc/>
        public async Task UpdateAsync(
            InstalledPluginInfo currentPlugin,
            InstalledPluginInfo updatedPlugin,
            CancellationToken cancellationToken)
        {
            Guard.NotNull(currentPlugin, nameof(currentPlugin));
            Guard.NotNull(updatedPlugin, nameof(updatedPlugin));

            if (!string.Equals(currentPlugin.Id, updatedPlugin.Id, StringComparison.OrdinalIgnoreCase))
            {
                throw new InvalidOperationException(
                    $"{currentPlugin.Id} cannot be updated with a plugin with a different ID: {updatedPlugin.Id}");
            }

            List<InstalledPluginInfo> installedPlugins = await ReadInstalledPlugins(cancellationToken);

            ThrowIfNotRegistered(installedPlugins, currentPlugin);
            ThrowIfAlreadyRegistered(installedPlugins, updatedPlugin);

            installedPlugins.RemoveAll(p => p.Equals(currentPlugin));
            installedPlugins.Add(updatedPlugin);

            await WriteInstalledPlugins(installedPlugins, cancellationToken);
        }

        /// <inheritdoc/>
        /// <remarks>
        ///    This methods asynchronously aquires an exclusive file lock that ensures only one process is interacting with the registry at a time.
        ///    Always make sure to acquire this lock before interacting with the registry and release it after the interaction.
        ///    Any other operations that depend on reading from the registry or resut in writing to the registry should be surrounded by the lock as well.
        /// </remarks>
        public async Task<IDisposable> AquireLockAsync(
            CancellationToken cancellationToken,
            TimeSpan? timeout)
        {
            return await this.fileDistributedLock.AcquireAsync(timeout, cancellationToken);
        }

        private async Task<List<InstalledPluginInfo>> ReadInstalledPlugins(
            CancellationToken cancellationToken)
        {
            if (!File.Exists(this.registryFilePath))
            {
                return new List<InstalledPluginInfo>();
            }

            try
            {
                using (var fileStream = new FileStream(this.registryFilePath, FileMode.Open, FileAccess.Read, FileShare.Read))
                {
                    return await this.registryFileSerializer.DeserializeAsync(
                        fileStream,
                        cancellationToken);
                }
            }
            catch (Exception e)
            {
                string errorMsg = null;
                if (e is JsonException)
                {
                    errorMsg = $"Deserialization of plugin registry failed due to invalid JSON text.";
                }
                else if (e is IOException)
                {
                    errorMsg = $"Failed to read the plugin registry file at {this.registryFilePath}.";
                }

                if (errorMsg != null)
                {
                    this.logger.Error(e, errorMsg);
                    throw new RepositoryDataAccessException(errorMsg, e);
                }

                throw;
            }
        }

        private async Task WriteInstalledPlugins(
            List<InstalledPluginInfo> installedPlugins,
            CancellationToken cancellationToken)
        {
            Guard.NotNull(installedPlugins, nameof(installedPlugins));

            Directory.CreateDirectory(this.registryRoot);

            try
            {
                using (var fileStream = new FileStream(this.registryFilePath, FileMode.Create, FileAccess.Write))
                {
                    await this.registryFileSerializer.SerializeAsync(
                        fileStream,
                        installedPlugins,
                        cancellationToken);
                }
            }
            catch (IOException e)
            {
                string errorMsg = $"Failed to write to plugin registry file at {this.registryFilePath}.";
                this.logger.Error(e, errorMsg);
                throw new RepositoryDataAccessException(errorMsg, e);
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

        private RepositoryCorruptedException CreateDuplicatedRegisteredException(string pluginId)
        {
            return new RepositoryCorruptedException(
                $"Duplicated records of plugin {pluginId} found in the plugin registry {this.registryFilePath}.");
        }

        private void LogDuplicatedRegistered(string pluginId)
        {
            this.logger.Error($"Duplicated records of plugin {pluginId} found in the plugin registry {this.registryFilePath}");
        }
    }
}
