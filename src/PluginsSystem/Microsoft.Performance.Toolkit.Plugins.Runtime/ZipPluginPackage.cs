// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.

using System;
using System.IO;
using System.IO.Compression;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.Performance.SDK;
using Microsoft.Performance.SDK.Processing;
using Microsoft.Performance.Toolkit.Plugins.Core.Packaging.Metadata;
using Microsoft.Performance.Toolkit.Plugins.Runtime.Exceptions;

namespace Microsoft.Performance.Toolkit.Plugins.Runtime
{
    /// <summary>
    ///     Represents a read-only plugin package.
    /// </summary>
    public sealed class ZipPluginPackage
        : PluginPackage
    {
        private const int bufferSize = 4096;
        private const int defaultAsyncBufferSize = 81920;

        private bool disposedValue;
        private readonly ZipArchive zip;
        
        /// <summary>
        ///     Creates an instance of <see cref="ZipPluginPackage"/>.
        /// </summary>
        /// <param name="stream">
        ///     Stream for reading the plugin package file.
        /// </param>
        /// <param name="metadataSerializer">
        ///     Used to deserialize the plugin metadata.
        /// </param>
        /// <param name="leaveOpen">
        ///     <c>true</c> to leave <paramref name = "stream" /> open after <see cref="ZipPluginPackage"/> is disposed.
        ///     <c>false</c> otherwise.
        /// </param>
        /// <param name="logger">
        ///     Used to log information.
        /// </param>
        public ZipPluginPackage(
            PluginMetadata metadata,
            ZipArchive zip,
            Func<Type, ILogger> loggerFactory)
            : base(metadata, loggerFactory)
        {
            Guard.NotNull(zip, nameof(zip));

            this.zip = zip;
        }

        /// <summary>
        ///     Extracts all files in this package.
        /// </summary>
        /// <param name="extractPath">
        ///     The path to which the files will be extracted.
        /// </param>
        /// <param name="cancellationToken">
        ///     Signals that the caller wishes to cancel the operation.
        /// </param>
        /// <param name="progress">
        ///     Indicates the progress of the extraction.
        /// </param>
        /// <returns>
        ///     An await-able task.
        /// </returns>
        public override async Task ExtractPackageAsync(
            string extractPath,
            CancellationToken cancellationToken,
            IProgress<int> progress)
        {
            Guard.NotNullOrWhiteSpace(extractPath, nameof(extractPath));

            // TODO: #257 Report progress
            try
            {
                foreach (ZipArchiveEntry entry in this.zip.Entries)
                {
                    cancellationToken.ThrowIfCancellationRequested();

                    bool isDirectory = entry.FullName.EndsWith("/");

                    string destPath = Path.GetFullPath(Path.Combine(extractPath, entry.FullName));
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
                string errorMsg = $"Unable to extract plugin content to {extractPath}";
                this.logger.Error(e, errorMsg);
                throw new PluginPackageExtractionException(errorMsg, e);
            }
        }

        protected override void Dispose(bool disposing)
        {
            if (!this.disposedValue)
            {
                if (disposing)
                {
                    this.zip.Dispose();
                }

                this.disposedValue = true;
            }
        }
    }
}
