// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.

using Microsoft.Performance.SDK;
using Microsoft.Performance.SDK.Processing;
using Microsoft.Performance.Toolkit.Plugins.Core;
using Microsoft.Performance.Toolkit.Plugins.Runtime.Exceptions;
using Microsoft.Performance.Toolkit.Plugins.Runtime.Package;
using System;
using System.IO;
using System.Threading;
using System.Threading.Tasks;

namespace Microsoft.Performance.Toolkit.Plugins.Runtime.Installation
{
    /// <summary>
    ///     A file system based implementation of <see cref="IInstalledPluginStorage"/>.
    /// </summary>
    public class FileSystemInstalledPluginStorage
        : IInstalledPluginStorage
    {
        private readonly string rootDirectory;
        private readonly ILogger logger;

        /// <summary>
        ///     Creates an instance of the <see cref="FileSystemInstalledPluginStorage"/>.
        /// </summary>
        /// <param name="rootDirectory">
        ///     The root directory of the storage.
        /// </param>
        /// <param name="loggerFactory">
        ///     The logger factory.
        /// </param>
        public FileSystemInstalledPluginStorage(
            string rootDirectory,
            Func<Type, ILogger> loggerFactory)
        {
            Guard.NotNull(loggerFactory, nameof(loggerFactory));
            Guard.NotNullOrWhiteSpace(rootDirectory, nameof(rootDirectory));

            this.rootDirectory = Path.GetFullPath(rootDirectory);
            this.logger = loggerFactory(typeof(FileSystemInstalledPluginStorage));
        }

        /// <inheritdoc/>
        public async Task AddAsync(
            PluginPackage package,
            CancellationToken cancellationToken,
            IProgress<int> progress)
        {
            Guard.NotNull(package, nameof(package));

            const int bufferSize = 4096;
            const int defaultAsyncBufferSize = 81920;

            var installDir = GetPluginContentDirectory(package.PluginIdentity);

            Guard.NotNullOrWhiteSpace(installDir, nameof(installDir));

            // TODO: #257 Report progress
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
                        destPath = Path.GetFullPath(Path.Combine(installDir, entry.ContentRelativePath));
                        isDirectory = entry.RawPath.EndsWith("/");
                    }
                    else
                    {
                        destPath = GetMetadataFilePath(package.PluginIdentity);
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
                        await entryStream.CopyToAsync(destStream, defaultAsyncBufferSize, cancellationToken).ConfigureAwait(false);
                    }
                }
            }
            catch (Exception e) when (!(e is OperationCanceledException))
            {
                string errorMsg = $"Unable to extract plugin content to {installDir}";
                this.logger.Error(e, errorMsg);
                throw new PluginPackageExtractionException(errorMsg, e);
            }
        }

        private string GetInstallDirectory(PluginIdentity pluginIdentity)
        {
            return PathUtils.GetPluginInstallDirectory(this.rootDirectory, pluginIdentity);
        }

        private string GetMetadataFilePath(PluginIdentity pluginIdentity)
        {
            return Path.Combine(GetInstallDirectory(pluginIdentity), PackageConstants.PluginMetadataFileName);
        }

        private string GetPluginContentDirectory(PluginIdentity pluginIdentity)
        {
            var directory = GetInstallDirectory(pluginIdentity);
            return Path.GetFullPath(Path.Combine(directory, PackageConstants.PluginContentFolderName));
        }
    }
}
