// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.

using System;
using System.Collections.Generic;
using System.IO;
using System.Text.Json;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.Performance.SDK;

namespace Microsoft.Performance.Toolkit.PluginsManager.Core.Registry
{
    // TODO: #238 Error handling
    public sealed class PluginRegistry : ISynchronizedObject
    {
        private static readonly string registryFileName = "installedPlugins.json";
        private static readonly string lockFileName = ".lockRegistry";
        private readonly string registryFilePath;
        private readonly string lockFilePath;
        private static readonly TimeSpan sleepDuration = TimeSpan.FromMilliseconds(500);

        public PluginRegistry(string registryRoot)
        {
            Guard.NotNull(registryRoot, nameof(registryRoot));
            if (!Path.IsPathRooted(registryRoot))
            {
                throw new ArgumentException("Registry root must be a rooted path.");
            }

            // TODO: Create a backup registry
            this.RegistryRoot = registryRoot;
            this.registryFilePath = Path.Combine(registryRoot, registryFileName);
            this.lockFilePath = Path.Combine(registryRoot, lockFileName);
        }

        public string RegistryRoot { get; }

        public string LockToken { get; private set; }

        public async Task<bool> RegisterPluginAsync(InstalledPlugin plugin)
        {
            Guard.NotNull(plugin, nameof(plugin));

            List<InstalledPlugin> installedPlugins = await ReadInstalledPlugins();

            installedPlugins.RemoveAll(p => p.Id == plugin.Id);
            installedPlugins.Add(plugin);

            await WriteInstalledPlugins(installedPlugins);
            return true;
        }

        public async Task<bool> UnregisterPluginAsync(InstalledPlugin plugin)
        {
            Guard.NotNull(plugin, nameof(plugin));

            List<InstalledPlugin> installedPlugins = await ReadInstalledPlugins();

            int removed = installedPlugins.RemoveAll(p => p.Id == plugin.Id);
            if (removed == 0)
            {
                return false;
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
            
            installedPlugins.RemoveAll(p => p.Id == currentPlugin.Id);
            installedPlugins.Add(updatedPlugin);

            await WriteInstalledPlugins(installedPlugins);
            return true;
        }

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

            this.LockToken = lockToken;
        }

        public void ReleaseLock()
        {
            if (this.LockToken == null || !File.Exists(this.lockFilePath))
            {
                return;
            }
            try
            {
                string token = File.ReadAllText(this.lockFilePath);
                if (token == this.LockToken)
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

            this.LockToken = null;
        }

        private Task<List<InstalledPlugin>> ReadInstalledPlugins()
        {
            if (!File.Exists(this.registryFilePath))
            {
                return Task.FromResult(new List<InstalledPlugin>());
            }
            
            using (var stream = new FileStream(this.registryFilePath, FileMode.Open, FileAccess.Read))
            {
                // TODO: Refatcor to a serialization/deserialization class
                return JsonSerializer.DeserializeAsync<List<InstalledPlugin>>(stream).AsTask();
            }
        }

        private Task WriteInstalledPlugins(IEnumerable<InstalledPlugin> installedPlugins)
        {
            Directory.CreateDirectory(this.RegistryRoot);

            using (var registryFileStream = new FileStream(this.registryFilePath, FileMode.Create, FileAccess.Write))
            {
                // TODO: Refatcor to a serialization/deserialization class
                return JsonSerializer.SerializeAsync(
                   registryFileStream,
                   installedPlugins,
                   typeof(InstalledPlugin[]),
                   new JsonSerializerOptions { WriteIndented = true });
            }
        }
    }
}
