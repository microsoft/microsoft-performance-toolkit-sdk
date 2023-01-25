// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.

using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using Microsoft.Performance.SDK;
using System.Threading.Tasks;
using System.Threading;
using System.Text.Json;

namespace Microsoft.Performance.Toolkit.PluginsManager.Core.Registry
{
    public sealed class PluginRegistry
    {
        public PluginRegistry(string registryRoot)
        {
            this.RegistryRoot = registryRoot;
        }

        public string RegistryRoot { get; }

        public Task RegisterPluginAsync(InstalledPlugin plugin, CancellationToken cancellationToken)
        {
            Guard.NotNull(plugin, nameof(plugin));

            var packages = GetInstalledPlugins(this.RegistryRoot);

            packages.RemoveAll(p => PluginEquals(p, plugin));
            packages.Add(plugin);

            WriteInstalledPlugins(packages);
            return Task.CompletedTask;
        }

        public Task<bool> UnregisterPluginAsync(InstalledPlugin package, CancellationToken cancellationToken)
        {
            if (package == null)
                throw new ArgumentNullException(nameof(package));

            var packages = GetInstalledPlugins(this.RegistryRoot);

            bool removed = packages.RemoveAll(p => PluginEquals(p, package)) > 0;
            if (removed)
                WriteInstalledPlugins(packages);

            return Task.FromResult(removed);
        }

        public Task<IReadOnlyList<InstalledPlugin>> GetInstalledPluginsAsync()
        {
            return Task.FromResult<IReadOnlyList<InstalledPlugin>>(GetInstalledPlugins(RegistryRoot));
        }

        public Task<Stream> TryOpenFromCacheAsync(PluginIdentity pluginId, CancellationToken cancellationToken = default)
        {
            Guard.NotNull(pluginId, nameof(pluginId));

            var fileName = this.GetCachedPackagePath(pluginId);
            if (!File.Exists(fileName))
                return Task.FromResult<Stream>(null);

            try
            {
                return Task.FromResult<Stream>(new FileStream(fileName, FileMode.Open, FileAccess.Read, FileShare.Read | FileShare.Delete));
            }
            catch (IOException)
            {
                // The File.Exists check above should handle most instances of this, but there is still
                // the possibility of a race condition if the file gets deleted before it is opened.
                return Task.FromResult<Stream>(null);
            }
        }

        public async Task WriteToCacheAsync(PluginIdentity pluginIdentity, Stream pluginPackageStream, CancellationToken cancellationToken = default)
        {
            Guard.NotNull(pluginIdentity, nameof(pluginIdentity));
            Guard.NotNull(pluginPackageStream, nameof(pluginPackageStream));

            var fileName = this.GetCachedPackagePath(pluginIdentity, true);
            try
            {
                using (var fileStream = new FileStream(fileName, FileMode.Create, FileAccess.Write, FileShare.None, 4096, FileOptions.Asynchronous | FileOptions.SequentialScan))
                {
                    await pluginPackageStream.CopyToAsync(fileStream, 81920, cancellationToken);
                }
            }
            catch (OperationCanceledException)
            {
                File.Delete(fileName);
                throw;
            }
        }

        private static List<InstalledPlugin> GetInstalledPlugins(string registryRoot)
        {
            var fileName = Path.Combine(registryRoot, "installedPlugins.json");

            if (!File.Exists(fileName))
            {
                return new List<InstalledPlugin>();
            }
            using (var configStream = new FileStream(fileName, FileMode.Open, FileAccess.Read))
            using (var doc = JsonDocument.Parse(configStream))
            {
                return doc.RootElement.EnumerateArray()
                    .Select(e => JsonSerializer.Deserialize<InstalledPlugin>(e.GetRawText()))
                    .ToList();
            }
        }

        private void WriteInstalledPlugins(IEnumerable<InstalledPlugin> installedPlugins)
        {
            Directory.CreateDirectory(RegistryRoot);
            var fileName = Path.Combine(RegistryRoot, "installedPlugins.json");

            using (var configStream = new FileStream(fileName, FileMode.Create, FileAccess.Write))
            {
                JsonSerializer.Serialize(
                   configStream,
                   installedPlugins,
                   typeof(Dictionary<string, object>[]),
                   new JsonSerializerOptions { WriteIndented = true });
            }
        }

        private string GetCachedPackagePath(PluginIdentity id, bool ensureDirectory = false)
        {
            var directoryName = "$" + id.Id;

            var fullPath = Path.Combine(this.RegistryRoot, "PluginsCache", directoryName);
            if (ensureDirectory)
                Directory.CreateDirectory(fullPath);

            var fileName = Path.Combine(fullPath, $"{id.Id}-{id.Version}.plugin");

            return Path.Combine(fullPath, fileName);
        }

        private static bool PluginEquals(InstalledPlugin p1, InstalledPlugin p2)
        {
            if (ReferenceEquals(p1, p2))
                return true;
            if (p1 is null || p2 is null)
                return false;

            return string.Equals(p1.Id, p2.Id, StringComparison.OrdinalIgnoreCase);
        }
    }
}
