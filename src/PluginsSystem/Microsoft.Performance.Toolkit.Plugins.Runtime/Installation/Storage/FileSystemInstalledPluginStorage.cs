// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.

using System;
using System.IO;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.Performance.SDK;
using Microsoft.Performance.SDK.Processing;
using Microsoft.Performance.SDK.Runtime.Progress;
using Microsoft.Performance.Toolkit.Plugins.Core;
using Microsoft.Performance.Toolkit.Plugins.Core.Packaging;
using Microsoft.Performance.Toolkit.Plugins.Core.Packaging.Metadata;
using Microsoft.Performance.Toolkit.Plugins.Core.Serialization;
using Microsoft.Performance.Toolkit.Plugins.Runtime.Exceptions;

namespace Microsoft.Performance.Toolkit.Plugins.Runtime.Installation
{
    /// <summary>
    ///     A file system based implementation of <see cref="IInstalledPluginStorage"/>.
    /// </summary>
    public sealed class FileSystemInstalledPluginStorage
        : IInstalledPluginStorage
    {
        private readonly IPluginsStorageDirectory pluginStorageDirectory;
        private readonly ISerializer<PluginMetadata> pluginMetadataSerializer;
        private readonly IDirectoryChecksumCalculator checksumCalculator;
        private readonly ILogger logger;

        /// <summary>
        ///     Creates an instance of the <see cref="FileSystemInstalledPluginStorage"/>.
        /// </summary>
        /// <param name="pluginStorageDirectory">
        ///     The directory where the plugins are stored.
        /// </param>
        /// <param name="metadataSerializer">
        ///     The serializer to use to serialize and deserialize the plugin metadata.
        /// </param>
        /// <param name="checksumCalculator">
        ///     The checksum calculator to use to calculate the checksum of the plugin content.
        /// </param>
        /// <param name="loggerFactory">
        ///     The logger factory.
        /// </param>
        internal FileSystemInstalledPluginStorage(
            IPluginsStorageDirectory pluginStorageDirectory,
            ISerializer<PluginMetadata> metadataSerializer,
            IDirectoryChecksumCalculator checksumCalculator,
            Func<Type, ILogger> loggerFactory)
        {
            Guard.NotNull(pluginStorageDirectory, nameof(pluginStorageDirectory));
            Guard.NotNull(checksumCalculator, nameof(checksumCalculator));
            Guard.NotNull(metadataSerializer, nameof(metadataSerializer));
            Guard.NotNull(loggerFactory, nameof(loggerFactory));

            this.pluginStorageDirectory = pluginStorageDirectory;
            this.pluginMetadataSerializer = metadataSerializer;
            this.checksumCalculator = checksumCalculator;
            this.logger = loggerFactory(typeof(FileSystemInstalledPluginStorage));
        }

        /// <inheritdoc/>
        public async Task<string> AddAsync(
            PluginPackage package,
            CancellationToken cancellationToken,
            IProgress<int> progress)
        {
            Guard.NotNull(package, nameof(package));
            Guard.NotNull(cancellationToken, nameof(cancellationToken));

            const int bufferSize = 4096;
            const int defaultAsyncBufferSize = 81920;

            string installDir = this.pluginStorageDirectory.GetPluginRootDirectory(package.PluginIdentity);

            long totalCopied = 0;
            long totalBytesToCopy = package.Entries.Select(e => e.InstalledSize).Sum();

            progress.Report(0);

            try
            {
                foreach (PluginPackageEntry entry in package.Entries)
                {
                    cancellationToken.ThrowIfCancellationRequested();

                    if (!entry.IsMetadataFile && !entry.IsPluginContentFile)
                    {
                        continue;
                    }

                    string destPath;
                    bool isDirectory = false;

                    if (entry.IsPluginContentFile)
                    {
                        string contentDir = this.pluginStorageDirectory.GetPluginContentDirectory(package.PluginIdentity);
                        destPath = Path.GetFullPath(Path.Combine(contentDir, entry.ContentRelativePath));
                        isDirectory = entry.RawPath.EndsWith("/");
                    }
                    else
                    {
                        destPath = this.pluginStorageDirectory.GetPluginMetadataFilePath(package.PluginIdentity);
                    }

                    string destDir = isDirectory ? destPath : Path.GetDirectoryName(destPath);
                    Directory.CreateDirectory(destDir);

                    if (isDirectory)
                    {
                        continue;
                    }

                    using (Stream entryStream = entry.Open())
                    using (var destStream = new FileStream(
                        destPath,
                        FileMode.Create,
                        FileAccess.Write,
                        FileShare.None,
                        bufferSize,
                        FileOptions.Asynchronous | FileOptions.SequentialScan))
                    {
                        long entryLength = entry.InstalledSize;

                        byte[] buffer = new byte[defaultAsyncBufferSize];

                        int read = 0;
                        while ((read = await entryStream.ReadAsync(buffer, 0, buffer.Length, cancellationToken)) > 0)
                        {
                            totalCopied += read;
                            await destStream.WriteAsync(buffer, 0, read, cancellationToken);

                            // report progress back
                            progress.Report((int)(totalCopied / (double)totalBytesToCopy * 100));
                        }
                    }
                }

                progress.Report(100);
            }
            catch (Exception e) when (!(e is OperationCanceledException))
            {
                string errorMsg = $"Unable to extract plugin content to {installDir}";
                this.logger.Error(e, errorMsg);
                throw new PluginPackageExtractionException(errorMsg, e);
            }

            string checksum = await this.checksumCalculator.GetDirectoryChecksumAsync(installDir);

            return checksum;
        }

        /// <inheritdoc/>
        public Task RemoveAsync(PluginIdentity pluginIdentity, CancellationToken cancellationToken)
        {
            Guard.NotNull(pluginIdentity, nameof(pluginIdentity));

            string installDir = this.pluginStorageDirectory.GetPluginRootDirectory(pluginIdentity);

            return Task.Run(() =>
            {
                if (Directory.Exists(installDir))
                {
                    Directory.Delete(installDir, true);
                }
            }, cancellationToken);
        }

        /// <inheritdoc/>
        public async Task<PluginMetadata> TryGetPluginMetadataAsync(
            PluginIdentity installedPlugin,
            CancellationToken cancellationToken)
        {
            Guard.NotNull(installedPlugin, nameof(installedPlugin));

            string metadataPath = this.pluginStorageDirectory.GetPluginMetadataFilePath(installedPlugin);

            if (!File.Exists(metadataPath))
            {
                return null;
            }

            PluginMetadata pluginMetadata;
            using (var fileStream = new FileStream(metadataPath, FileMode.Open, FileAccess.Read, FileShare.Read))
            {
                pluginMetadata = await this.pluginMetadataSerializer.DeserializeAsync(
                    fileStream,
                    cancellationToken);
            }

            return pluginMetadata;
        }
    }
}
