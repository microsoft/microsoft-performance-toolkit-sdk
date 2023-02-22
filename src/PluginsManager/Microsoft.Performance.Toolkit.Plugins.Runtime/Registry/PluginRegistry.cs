// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.

using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.Performance.SDK;
using Microsoft.Performance.SDK.Processing;
using Microsoft.Performance.SDK.Runtime;
using Microsoft.Performance.Toolkit.Plugins.Core.Serialization;

namespace Microsoft.Performance.Toolkit.Plugins.Runtime
{
    // TODO: #238 Add proper error handling
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

        public async Task<List<InstalledPluginInfo>> GetInstalledPlugins()
        {
            return await ReadInstalledPlugins();
        }

        public async Task<bool> RegisterPluginAsync(InstalledPluginInfo plugin)
        {
            Guard.NotNull(plugin, nameof(plugin));

            List<InstalledPluginInfo> installedPlugins = await ReadInstalledPlugins();
            
            ThrowIfAlreadyRegistered(installedPlugins, plugin);
            installedPlugins.Add(plugin);

            return await WriteInstalledPlugins(installedPlugins);
        }

        public async Task<bool> UnregisterPluginAsync(InstalledPluginInfo plugin)
        {
            Guard.NotNull(plugin, nameof(plugin));

            List<InstalledPluginInfo> installedPlugins = await ReadInstalledPlugins();

            ThrowIfNotRegistered(installedPlugins, plugin);
            installedPlugins.Remove(plugin);

            return await WriteInstalledPlugins(installedPlugins);
        }

        public async Task<bool> UpdatePluginAsync(InstalledPluginInfo currentPlugin, InstalledPluginInfo updatedPlugin)
        {
            Guard.NotNull(currentPlugin, nameof(currentPlugin));
            Guard.NotNull(updatedPlugin, nameof(updatedPlugin));

            List<InstalledPluginInfo> installedPlugins = await ReadInstalledPlugins();

            ThrowIfNotRegistered(installedPlugins, currentPlugin);
            ThrowIfAlreadyRegistered(installedPlugins, updatedPlugin);

            installedPlugins.Remove(currentPlugin);
            installedPlugins.Add(updatedPlugin);

            return await WriteInstalledPlugins(installedPlugins);
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

            return SerializationUtils.ReadFromFileAsync<List<InstalledPluginInfo>>(
                this.registryFilePath,
                SerializationConfig.PluginsManagerSerializerDefaultOptions,
                this.logger);
        }

        private Task<bool> WriteInstalledPlugins(IEnumerable<InstalledPluginInfo> installedPlugins)
        {
            Guard.NotNull(installedPlugins, nameof(installedPlugins));

            Directory.CreateDirectory(this.RegistryRoot);

            return SerializationUtils.WriteToFileAsync(
                this.registryFilePath,
                installedPlugins,
                SerializationConfig.PluginsManagerSerializerDefaultOptions,
                this.logger);
        }

        private void ThrowIfAlreadyRegistered(
            IEnumerable<InstalledPluginInfo> installedPluginInfos,
            InstalledPluginInfo pluginToInstall)
        {
            int installedCount = installedPluginInfos.Count(p => p.Id.Equals(pluginToInstall.Id));
            if (installedCount == 1)
            {
                throw new InvalidOperationException($"Failed to register plugin {pluginToInstall} as it is already registered.");
            }

            if (installedCount > 1)
            {
                string errorMsg = $"Duplicated records of plugin {pluginToInstall} found in the plugin registry.";
                this.logger.Error(errorMsg);

                throw new PluginRegistryCorruptedException(errorMsg);
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
                string errorMsg = $"Duplicated records of plugin {pluginToRemove} found in the plugin registry.";
                this.logger.Error(errorMsg);

                throw new PluginRegistryCorruptedException(errorMsg);
            }
        }
    }
}
