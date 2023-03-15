// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.

using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.Performance.SDK;
using Microsoft.Performance.SDK.Processing;
using Microsoft.Performance.SDK.Runtime;
using Microsoft.Performance.Toolkit.Plugins.Runtime.Common;
using Microsoft.Performance.Toolkit.Plugins.Runtime.Exceptions;

namespace Microsoft.Performance.Toolkit.Plugins.Runtime
{
    /// <summary>
    ///     Extracts the entries from the plugin package to the specified directory.
    /// </summary>
    public class LocalPluginPackageExtractor
        : IPluginPackageExtractor<DirectoryInfo>
    {
        private readonly IStreamCopier<FileInfo> streamCopier;
        private readonly ILogger logger;

        /// <summary>
        ///    Creates a new instance of <see cref="LocalPluginPackageExtractor"/> using the default
        /// </summary>
        public LocalPluginPackageExtractor()
            : this(new DirectoryAccessor())
        {
        }

        /// <summary>
        ///     Creates a new instance of <see cref="LocalPluginPackageExtractor"/>.
        /// </summary>
        /// <param name="streamCopier">
        ///     Used to copy the plugin package entries to the specified directory.
        /// </param>
        public LocalPluginPackageExtractor(
            IStreamCopier<FileInfo> streamCopier)
            : this(streamCopier, Logger.Create<LocalPluginPackageExtractor>())
        {
        }

        /// <summary>
        ///     Creates a new instance of <see cref="LocalPluginPackageExtractor"/>.
        /// </summary>
        /// <param name="streamCopier">
        ///     Used to copy the plugin package entries to the specified directory.
        /// </param>
        /// <param name="logger">
        ///     Used to log messages.
        /// </param>
        public LocalPluginPackageExtractor(
            IStreamCopier<FileInfo> streamCopier,
            ILogger logger)
        {
            Guard.NotNull(streamCopier, nameof(streamCopier));
            Guard.NotNull(logger, nameof(logger));

            this.streamCopier = streamCopier;
            this.logger = logger;
        }

        /// <inheritdoc/>
        public Task ExtractEntriesAsync(
            PluginPackage pluginPackage,
            DirectoryInfo extractPath,
            Func<PluginPackageEntry, bool> predicate,
            CancellationToken cancellationToken,
            IProgress<int> progress)
        {
            Guard.NotNull(pluginPackage, nameof(pluginPackage));

            return ExtractEntriesInternalAsync(
                pluginPackage.Entries.Where(e => predicate(e)),
                extractPath,
                this.logger,
                cancellationToken,
                progress);
        }

        /// <inheritdoc/>
        public Task ExtractPackageAsync(
            PluginPackage pluginPackage,
            DirectoryInfo extractPath,
            CancellationToken cancellationToken,
            IProgress<int> progress)
        {
            Guard.NotNull(pluginPackage, nameof(pluginPackage));

            return ExtractEntriesInternalAsync(
              pluginPackage.Entries,
              extractPath,
              this.logger,
              cancellationToken,
              progress);
        }

        private async Task ExtractEntriesInternalAsync(
           IEnumerable<PluginPackageEntry> entries,
           DirectoryInfo extractDir,
           ILogger logger,
           CancellationToken cancellationToken,
           IProgress<int> progress)
        {
            Guard.NotNull(entries, nameof(entries));
            Guard.NotNull(extractDir, nameof(extractDir));
            Guard.NotNull(logger, nameof(logger));

            // TODO: #257 Report progress
            try
            {
                foreach (PluginPackageEntry entry in entries)
                {
                    cancellationToken.ThrowIfCancellationRequested();

                    string destPath = Path.GetFullPath(Path.Combine(extractDir.FullName, entry.RelativePath));
                    string destDir = entry.IsDirectory ? destPath : Path.GetDirectoryName(destPath);

                    using (Stream entryStream = entry.Open())
                    {
                        await this.streamCopier.CopyStreamAsync(
                            new FileInfo(destPath),
                            entryStream,
                            cancellationToken);
                    }
                }
            }
            catch (Exception e) when (!(e is OperationCanceledException))
            {
                string errorMsg = $"Unable to extract plugin content to {extractDir}";
                logger.Error(e, errorMsg);
                throw new PluginPackageExtractionException(errorMsg, e);
            }
        }
    }
}
