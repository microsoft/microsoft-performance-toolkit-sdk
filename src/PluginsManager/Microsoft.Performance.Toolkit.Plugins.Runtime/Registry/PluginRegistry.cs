// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.

using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text.Json;
using System.Text.Json.Serialization;
using System.Threading.Tasks;
using Medallion.Threading.FileSystem;
using Microsoft.Performance.SDK;

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
            this.FileLock = new FileDistributedLock(new FileInfo(this.lockFilePath));
        }

        /// <summary>
        ///    An exclusive file lock that indicates whether a process is currently interacting with the registry.
        ///    Always make sure to acquire this lock before interacting with the registry and release it after the interaction.
        ///    Usage:
        ///    <code>
        ///         using (await myLock.AcquireAsync(...))
        ///         {
        ///             /* we have the lock! */
        ///         }
        ///         // The lock is released here after the using block
        ///     </code>
        /// </summary>
        public FileDistributedLock FileLock { get; }

        public string RegistryRoot { get; }

        public async Task<List<InstalledPluginInfo>> GetInstalledPlugins()
        {
            return await ReadInstalledPlugins();
        }

        public async Task<bool> RegisterPluginAsync(InstalledPluginInfo plugin)
        {
            Guard.NotNull(plugin, nameof(plugin));

            List<InstalledPluginInfo> installedPlugins = await ReadInstalledPlugins();
            int installedCount = installedPlugins.Count(p => p.Id.Equals(plugin.Id));

            if (installedCount == 1)
            {
                throw new InvalidOperationException($"Plugin already registered: " +
                    $"{plugin.Id}-{plugin.Version}");
            }
            if (installedCount > 1)
            {
                throw new InvalidDataException($"Duplicated records found in the plugin registry.");
            }

            installedPlugins.Add(plugin);

            await WriteInstalledPlugins(installedPlugins);
            return true;
        }

        public async Task<bool> UnregisterPluginAsync(InstalledPluginInfo plugin)
        {
            Guard.NotNull(plugin, nameof(plugin));

            List<InstalledPluginInfo> installedPlugins = await ReadInstalledPlugins();
            int toRemove = installedPlugins.RemoveAll(p => p.Equals(plugin));
            if (toRemove == 0)
            {
                throw new InvalidOperationException($"Failed to unregister plugin as it is not currently registered: " +
                    $"{plugin.Id}-{plugin.Version}");
            }

            if (toRemove > 1)
            {
                throw new InvalidDataException($"Duplicated records found in the plugin registry.");
            }

            await WriteInstalledPlugins(installedPlugins);
            return true;
        }

        public async Task<bool> UpdatePluginAync(InstalledPluginInfo currentPlugin, InstalledPluginInfo updatedPlugin)
        {
            Guard.NotNull(currentPlugin, nameof(currentPlugin));
            Guard.NotNull(updatedPlugin, nameof(updatedPlugin));

            List<InstalledPluginInfo> installedPlugins = await ReadInstalledPlugins();
            
            // TODO: refactor duplicated code.
            int toRemove = installedPlugins.RemoveAll(p => p.Id == currentPlugin.Id);
            if (toRemove == 0)
            {
                throw new InvalidOperationException("Failed to update plugin as it is not currently registered: " +
                    $"{currentPlugin.Id}-{currentPlugin.Version}");
            }

            if (toRemove > 1)
            {
                throw new InvalidDataException($"Duplicated records found in the plugin registry.");
            }
            
            int installedCount = installedPlugins.Count(p => p.Id.Equals(updatedPlugin.Id));
            if (installedCount == 1)
            {
                throw new InvalidOperationException("Failed to update plugin as the new version is already installed: " +
                    $"{currentPlugin.Id}-{currentPlugin.Version}");
            }
            if (installedCount > 1)
            {
                throw new InvalidDataException($"Duplicated records found in the plugin registry.");
            }

            installedPlugins.Add(updatedPlugin);

            await WriteInstalledPlugins(installedPlugins);
            return true;
        }

        private async Task<List<InstalledPluginInfo>> ReadInstalledPlugins()
        {
            if (!File.Exists(this.registryFilePath))
            {
                return new List<InstalledPluginInfo>();
            }
            
            using (var stream = new FileStream(this.registryFilePath, FileMode.Open, FileAccess.Read))
            {
                // TODO: #255 Refatcor to a serialization/deserialization class
                return await JsonSerializer.DeserializeAsync<List<InstalledPluginInfo>>(stream);
            }
        }

        private async Task WriteInstalledPlugins(IEnumerable<InstalledPluginInfo> installedPlugins)
        {
            Guard.NotNull(installedPlugins, nameof(installedPlugins));

            Directory.CreateDirectory(this.RegistryRoot);

            using (var registryFileStream = new FileStream(this.registryFilePath, FileMode.Create, FileAccess.Write))
            {
                // TODO: #255 Refatcor to a serialization/deserialization class
                 await JsonSerializer.SerializeAsync(
                   registryFileStream,
                   installedPlugins,
                   typeof(IEnumerable<InstalledPluginInfo>),
                   new JsonSerializerOptions { 
                       WriteIndented = true,
                       Converters =
                       {
                           new JsonStringEnumConverter(),
                           new StringConverter(),
                       }
                   });
            }
        }

        // TODO: #255 Refatcor to a serialization/deserialization class
        internal class StringConverter : JsonConverter<Version>
        {
            public override Version Read(ref Utf8JsonReader reader, Type typeToConvert, JsonSerializerOptions options)
            {
                if (reader.TokenType == JsonTokenType.String)
                {
                    string stringValue = reader.GetString();
                    return new Version(stringValue);
                }

                throw new JsonException();
            }

            public override void Write(Utf8JsonWriter writer, Version value, JsonSerializerOptions options)
            {
                writer.WriteStringValue(value.ToString());
            }
        }
    }
}
