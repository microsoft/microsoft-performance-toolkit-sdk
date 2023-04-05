// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.

using System;
using System.IO;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.Performance.SDK;
using Microsoft.Performance.SDK.Processing;
using Microsoft.Performance.Toolkit.Plugins.Runtime.Exceptions;
using Microsoft.Performance.Toolkit.Plugins.Runtime.Package;

namespace Microsoft.Performance.Toolkit.Plugins.Runtime.Installation
{
    /// <summary>
    ///     A file system based implementation of <see cref="IInstalledPluginStorage"/>.
    /// </summary>
    public class FileSystemInstalledPluginStorage
        : IInstalledPluginStorage
    {
        private readonly IFileSystemInstalledPluginLocator locator;
        private readonly ILogger logger;

        /// <summary>
        ///     Creates an instance of the <see cref="FileSystemInstalledPluginStorage"/>.
        /// </summary>
        /// <param name="locator">
        ///     The file system plugin locator.
        /// </param>
        /// <param name="loggerFactory">
        ///     The logger factory.
        /// </param>
        public FileSystemInstalledPluginStorage(
            IFileSystemInstalledPluginLocator locator,
            Func<Type, ILogger> loggerFactory)
        {
            Guard.NotNull(locator, nameof(locator));
            Guard.NotNull(loggerFactory, nameof(loggerFactory));

            this.locator = locator;
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

            string installDir = this.locator.GetPluginContentDirectory(package.PluginIdentity);

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
                        destPath = this.locator.GetPluginMetadataFilePath(package.PluginIdentity);
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
    }
}
