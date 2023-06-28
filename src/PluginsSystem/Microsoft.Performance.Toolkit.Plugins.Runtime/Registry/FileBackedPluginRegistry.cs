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
        private readonly Func<Type, ILogger> loggerFactory;

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
            : this(registryRoot, serializer, Logger.Create)
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
        /// <param name="loggerFactory">
        ///     Used to create a logger for the given type.
        /// </param>
        /// <exception cref="ArgumentException">
        ///     Throws when <paramref name="registryRoot"/> is not a rooted path.
        /// </exception>
        /// <exception cref="ArgumentNullException">
        ///     Throws when <paramref name="registryRoot"/>, <paramref name="serializer"/>, or <paramref name="loggerFactory"/> is null.
        /// </exception>
        public FileBackedPluginRegistry(
            string registryRoot,
            ISerializer<List<InstalledPluginInfo>> serializer,
            Func<Type, ILogger> loggerFactory)
        {
            Guard.NotNull(registryRoot, nameof(registryRoot));
            Guard.NotNull(serializer, nameof(serializer));
            Guard.NotNull(loggerFactory, nameof(loggerFactory));

            // TODO: #256 Create a backup registry
            this.registryRoot = Path.GetFullPath(registryRoot);
            this.registryFilePath = Path.Combine(registryRoot, registryFileName);
            this.registryFileSerializer = serializer;
            this.loggerFactory = loggerFactory;
            this.logger = loggerFactory(typeof(FileBackedPluginRegistry));

            string lockFilePath = Path.Combine(registryRoot, lockFileName);
            this.fileDistributedLock = new FileDistributedLock(new FileInfo(lockFilePath));
        }

        /// <summary>
        ///     Gets the name of the registry file.
        /// </summary>
        public static string RegistryFileName
        {
            get
            {
                return registryFileName;
            }
        }

        /// <summary>
        ///     Gets the name of the lock file.
        /// </summary>
        public static string LockFileName
        {
            get
            {
                return lockFileName;
            }
        }

        /// <summary>
        ///     Gets the root directory of the registry.
        /// </summary>
        public string RegistryRoot
        {
            get
            {
                return this.registryRoot;
            }
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
                    installedPluginInfo => installedPluginInfo.PluginInfo.Identity.Id.Equals(pluginId, StringComparison.Ordinal));
            }
            catch
            {
                LogDuplicatedRegistered(pluginId);
                throw CreateDuplicatedRegisteredException(pluginId);
            }
        }

        /// <inheritdoc/>
        public async Task<bool> ExistsAsync(
            InstalledPluginInfo installedPluginInfo,
            CancellationToken cancellationToken)
        {
            List<InstalledPluginInfo> installedPlugins = await ReadInstalledPlugins(cancellationToken);

            int installedCount = installedPlugins.Count(p => p.Equals(installedPluginInfo));
            if (installedCount > 1)
            {
                LogDuplicatedRegistered(installedPluginInfo.PluginInfo.Identity.Id);
                throw CreateDuplicatedRegisteredException(installedPluginInfo.PluginInfo.Identity.Id);
            }

            return installedCount == 1;
        }

        /// <inheritdoc/>
        public async Task AddAsync(
            InstalledPluginInfo installedPluginInfo,
            CancellationToken cancellationToken)
        {
            Guard.NotNull(installedPluginInfo, nameof(installedPluginInfo));

            List<InstalledPluginInfo> installedPlugins = await ReadInstalledPlugins(cancellationToken);

            ThrowIfIdExists(installedPlugins, installedPluginInfo.PluginInfo.Identity.Id);
            installedPlugins.Add(installedPluginInfo);

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

            if (!string.Equals(currentPlugin.PluginInfo.Identity.Id, updatedPlugin.PluginInfo.Identity.Id, StringComparison.Ordinal))
            {
                throw new InvalidOperationException(
                    $"{currentPlugin.PluginInfo.Identity.Id} cannot be updated with a plugin with a different ID: {updatedPlugin.PluginInfo.Identity.Id}");
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

        private void ThrowIfIdExists(
            IEnumerable<InstalledPluginInfo> installedPluginInfos,
            string pluginId)
        {
            int installedCount = installedPluginInfos.Count(p => p.PluginInfo.Identity.Id.Equals(pluginId));
            if (installedCount > 0)
            {
                if (installedCount > 1)
                {
                    LogDuplicatedRegistered(pluginId);
                }

                throw new InvalidOperationException($"Failed to add plugin {pluginId} as it is already registered.");
            }
        }

        private void ThrowIfAlreadyRegistered(
            IEnumerable<InstalledPluginInfo> installedPluginInfos,
            InstalledPluginInfo pluginToInstall)
        {
            int installedCount = installedPluginInfos.Count(p => p.Equals(pluginToInstall));
            if (installedCount >= 1)
            {
                if (installedCount > 1)
                {
                    LogDuplicatedRegistered(pluginToInstall.PluginInfo.Identity.Id);
                }

                throw new InvalidOperationException($"Failed to update plugin {pluginToInstall} as it is already registered.");
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
                LogDuplicatedRegistered(pluginToRemove.PluginInfo.Identity.Id);
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
