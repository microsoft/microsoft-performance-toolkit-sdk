// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.

using System.Collections.Generic;
using System.IO;
using Microsoft.Performance.SDK;
using System.Threading.Tasks;
using System.Threading;
using System.Text.Json;
using System;

namespace Microsoft.Performance.Toolkit.PluginsManager.Core.Registry
{
    // TODO: #238 Error handling
    public sealed class PluginRegistry : IDisposable
    {
        private static readonly string registryFileName = "installedPlugins.json";
        private static readonly string lockFileName = ".lockRegistry";
        private readonly string registryFilePath;
        private readonly string lockFilePath;
        private bool disposedValue;

        public PluginRegistry(string registryRoot)
        {
            // TODO: Create a backup registry
            this.RegistryRoot = registryRoot;
            this.registryFilePath = Path.Combine(registryRoot, registryFileName);
            this.lockFilePath = Path.Combine(registryRoot, lockFileName);
        }

        public string RegistryRoot { get; }

        public async Task<bool> RegisterPluginAsync(InstalledPlugin plugin, CancellationToken cancellationToken)
        {
            Guard.NotNull(plugin, nameof(plugin));

            List<InstalledPlugin> installedPlugins = await ReadInstalledPlugins();

            installedPlugins.RemoveAll(p => p.Id == plugin.Id);
            installedPlugins.Add(plugin);

            await WriteInstalledPlugins(installedPlugins);
            return true;
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

        public async Task<bool> UpdatePlugin(InstalledPlugin currentPlugin, InstalledPlugin updatedPlugin)
        {
            // TODO: Add implementation

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
            // TODO: Implement
        }

        public async void ReleaseLock()
        {
            // TODO: Implement
        }

        private Task<List<InstalledPlugin>> ReadInstalledPlugins()
        {
            if (!File.Exists(this.registryFilePath))
            {
                return Task.FromResult(new List<InstalledPlugin>());
            }
            
            using (var configStream = new FileStream(this.registryFilePath, FileMode.Open, FileAccess.Read))
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

        private void Dispose(bool disposing)
        {
            if (!disposedValue)
            {
                if (disposing)
                {
                    // TODO: dispose managed state (managed objects)
                }

                // TODO: free unmanaged resources (unmanaged objects) and override finalizer
                // TODO: set large fields to null
                disposedValue = true;
            }
        }

        public void Dispose()
        {
            // Do not change this code. Put cleanup code in 'Dispose(bool disposing)' method
            Dispose(disposing: true);
            GC.SuppressFinalize(this);
        }
    }
}
