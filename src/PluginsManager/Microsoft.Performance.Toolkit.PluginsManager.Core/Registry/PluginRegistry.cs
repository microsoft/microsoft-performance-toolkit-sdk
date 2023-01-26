// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.

using System.Collections.Generic;
using System.IO;
using Microsoft.Performance.SDK;
using System.Threading.Tasks;
using System.Threading;
using System.Text.Json;

namespace Microsoft.Performance.Toolkit.PluginsManager.Core.Registry
{
    public sealed class PluginRegistry
    {
        private static readonly string registryFileName = "installedPlugins.json";

        public PluginRegistry(string registryRoot)
        {
            this.RegistryRoot = registryRoot;
            this.RegistryFilePath = Path.Combine(registryRoot, registryFileName);
        }

        public string RegistryRoot { get; }

        public string RegistryFilePath { get; }

        public async Task RegisterPluginAsync(InstalledPlugin plugin, CancellationToken cancellationToken)
        {
            Guard.NotNull(plugin, nameof(plugin));

            List<InstalledPlugin> installedPlugins = await ReadInstalledPlugins();

            installedPlugins.RemoveAll(p => p.Id == plugin.Id);
            installedPlugins.Add(plugin);

            await WriteInstalledPlugins(installedPlugins);
        }

        public async Task<bool> UnregisterPluginAsync(InstalledPlugin plugin, CancellationToken cancellationToken)
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

        public async Task<List<InstalledPlugin>> GetInstalledPluginsAsync()
        {
            return await ReadInstalledPlugins();
        }

        public Task<bool> UpdatePlugin(InstalledPlugin currentPlugin, InstalledPlugin updatedPlugin)
        {
            // TODO: Add implementation

            return Task.FromResult(true);
        }

        private Task<List<InstalledPlugin>> ReadInstalledPlugins()
        {
            if (!File.Exists(this.RegistryFilePath))
            {
                return Task.FromResult(new List<InstalledPlugin>());
            }
            
            using (var configStream = new FileStream(this.RegistryFilePath, FileMode.Open, FileAccess.Read))
            {
                // TODO: Refatcor to a serialization/deserialization class
                return JsonSerializer.DeserializeAsync<List<InstalledPlugin>>(configStream).AsTask();
            }
        }

        private Task WriteInstalledPlugins(IEnumerable<InstalledPlugin> installedPlugins)
        {
            Directory.CreateDirectory(this.RegistryRoot);
            string fileName = Path.Combine(this.RegistryRoot, registryFileName);

            using (var registryFileStream = new FileStream(fileName, FileMode.Create, FileAccess.Write))
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
