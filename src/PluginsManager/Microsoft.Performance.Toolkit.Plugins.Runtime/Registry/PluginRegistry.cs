﻿// Copyright (c) Microsoft Corporation.
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

namespace Microsoft.Performance.Toolkit.Plugins.Runtime
{
    // TODO: #254 Add docstrings
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

        public PluginRegistry(string registryRoot)
            : this(registryRoot, Logger.Create(typeof(PluginRegistry)))
        {         
        }

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

        public string RegistryRoot { get; }

        public Task<List<InstalledPluginInfo>> GetAllInstalledPlugins()
        {
            return ReadInstalledPlugins();
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

        public async Task RegisterPluginAsync(InstalledPluginInfo plugin)
        {
            Guard.NotNull(plugin, nameof(plugin));

            List<InstalledPluginInfo> installedPlugins = await ReadInstalledPlugins();
            
            ThrowIfAlreadyRegistered(installedPlugins, plugin);
            installedPlugins.Add(plugin);

            await WriteInstalledPlugins(installedPlugins);
        }

        public async Task UnregisterPluginAsync(InstalledPluginInfo plugin)
        {
            Guard.NotNull(plugin, nameof(plugin));

            List<InstalledPluginInfo> installedPlugins = await ReadInstalledPlugins();

            ThrowIfNotRegistered(installedPlugins, plugin);
            installedPlugins.RemoveAll(p => p.Equals(plugin));

            await WriteInstalledPlugins(installedPlugins);
        }

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

        private Task<List<InstalledPluginInfo>> ReadInstalledPlugins()
        {
            if (!File.Exists(this.registryFilePath))
            {
                return Task.FromResult(new List<InstalledPluginInfo>());
            }

            using (var fileStream = new FileStream(this.registryFilePath, FileMode.Open, FileAccess.Read, FileShare.Read))
            {
                return JsonSerializer.DeserializeAsync<List<InstalledPluginInfo>>(
                    fileStream,
                    SerializationConfig.PluginsManagerSerializerDefaultOptions).AsTask();          
            }
        }

        private Task WriteInstalledPlugins(IEnumerable<InstalledPluginInfo> installedPlugins)
        {
            Guard.NotNull(installedPlugins, nameof(installedPlugins));

            Directory.CreateDirectory(this.RegistryRoot);

            using (var fileStream = new FileStream(this.registryFilePath, FileMode.Create, FileAccess.Write))
            {
                return JsonSerializer.SerializeAsync(
                    fileStream,
                    installedPlugins,
                    SerializationConfig.PluginsManagerSerializerDefaultOptions);
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
