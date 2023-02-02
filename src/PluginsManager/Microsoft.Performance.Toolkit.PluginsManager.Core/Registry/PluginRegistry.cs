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
using Microsoft.Performance.Toolkit.PluginsManager.Core.Concurrency;
using Microsoft.Performance.Toolkit.PluginsManager.Core.Installation;

namespace Microsoft.Performance.Toolkit.PluginsManager.Core.Registry
{
    // TODO: #238 Add proper error handling
    // TODO: #254 Add docstrings
    /// <summary>
    ///     Contains a registry file that records information of installed plugins 
    ///     and an ephemeral lock file that indicates whether a process is currently interacting with the registry.
    ///     Only one version of a same plugin may be be registered at a time 
    /// </summary>
    public sealed class PluginRegistry : ISynchronizedObject
    {
        private static readonly string registryFileName = "installedPlugins.json";
        private static readonly string lockFileName = ".lockRegistry";
        private static readonly string backupRegistryFileExtension = ".bak";

        private readonly string registryFilePath;
        private readonly string lockFilePath;
        private readonly string backupRegistryFilePath;
        private static readonly TimeSpan sleepDuration = TimeSpan.FromMilliseconds(500);
        private string lockToken;

        public PluginRegistry(string registryRoot)
        {
            Guard.NotNull(registryRoot, nameof(registryRoot));
            if (!Path.IsPathRooted(registryRoot))
            {
                throw new ArgumentException("Registry root must be a rooted path.");
            }

            // TODO: #256 Create a backup registry
            this.RegistryRoot = registryRoot;
            this.registryFilePath = Path.Combine(registryRoot, registryFileName);
            this.backupRegistryFilePath = this.registryFilePath + backupRegistryFileExtension;
            this.lockFilePath = Path.Combine(registryRoot, lockFileName);
        }

        public string RegistryRoot { get; }

        public async Task<bool> RegisterPluginAsync(InstalledPlugin plugin)
        {
            Guard.NotNull(plugin, nameof(plugin));

            List<InstalledPlugin> installedPlugins = await ReadInstalledPlugins();
            if (installedPlugins.Any(p => p.Id.Equals(plugin.Id)))
            {
                throw new InvalidOperationException("Plugin already registered.");
            }

            installedPlugins.Add(plugin);

            await WriteInstalledPlugins(installedPlugins);
            return true;
        }

        public async Task<bool> UnregisterPluginAsync(InstalledPlugin plugin)
        {
            Guard.NotNull(plugin, nameof(plugin));

            List<InstalledPlugin> installedPlugins = await ReadInstalledPlugins();
            int removed = installedPlugins.RemoveAll(p => p.Equals(plugin));
            if (removed == 0)
            {
                throw new InvalidOperationException("Failed to unregister plugin as it is not currently registered.");
            }

            await WriteInstalledPlugins(installedPlugins);
            return true;
        }

        public Task<List<InstalledPlugin>> GetInstalledPlugins()
        {
            return ReadInstalledPlugins();
        }

        public async Task<bool> UpdatePlugin(InstalledPlugin currentPlugin, InstalledPlugin updatedPlugin)
        {
            Guard.NotNull(currentPlugin, nameof(currentPlugin));
            Guard.NotNull(updatedPlugin, nameof(updatedPlugin));

            List<InstalledPlugin> installedPlugins = await ReadInstalledPlugins();
            
            int removed = installedPlugins.RemoveAll(p => p.Id == currentPlugin.Id);
            if (removed == 0)
            {
                throw new InvalidOperationException("Failed to update plugin as it is not currently registered.");
            }
            if (installedPlugins.Any(p => p.Equals(updatedPlugin)))
            {
                throw new InvalidOperationException("Failed to update plugin as the new version is already installed.");
            }

            installedPlugins.Add(updatedPlugin);

            await WriteInstalledPlugins(installedPlugins);
            return true;
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

        private Task<List<InstalledPlugin>> ReadInstalledPlugins()
        {
            if (!File.Exists(this.registryFilePath))
            {
                return Task.FromResult(new List<InstalledPlugin>());
            }
            
            using (var stream = new FileStream(this.registryFilePath, FileMode.Open, FileAccess.Read))
            {
                // TODO: #255 Refatcor to a serialization/deserialization class
                return JsonSerializer.DeserializeAsync<List<InstalledPlugin>>(stream).AsTask();
            }
        }

        private Task WriteInstalledPlugins(IEnumerable<InstalledPlugin> installedPlugins)
        {
            Directory.CreateDirectory(this.RegistryRoot);

            using (var registryFileStream = new FileStream(this.registryFilePath, FileMode.Create, FileAccess.Write))
            {
                // TODO: #255 Refatcor to a serialization/deserialization class
                return JsonSerializer.SerializeAsync(
                   registryFileStream,
                   installedPlugins,
                   typeof(InstalledPlugin[]),
                   new JsonSerializerOptions { WriteIndented = true });
            }
        }
    }
}
