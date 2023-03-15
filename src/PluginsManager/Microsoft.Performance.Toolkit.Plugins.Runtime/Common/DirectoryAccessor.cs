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

namespace Microsoft.Performance.Toolkit.Plugins.Runtime.Common
{
    /// <inheritdoc />
    public class DirectoryAccessor
        : IDirectoryAccessor
    {
        private readonly ILogger logger;

        const int bufferSize = 4096;
        const int defaultAsyncBufferSize = 81920;

        /// <summary>
        ///     Initializes a new instance of the <see cref="DirectoryAccessor"/> class.
        /// </summary>
        /// <param name="logger">
        ///     The logger to use to log messages.
        /// </param>
        public DirectoryAccessor(ILogger logger)
        {
            Guard.NotNull(logger, nameof(logger));

            this.logger = logger;
        }

        /// <summary>
        ///     Initializes a new instance of the <see cref="DirectoryAccessor"/> class.
        /// </summary>
        public DirectoryAccessor()
            : this(Logger.Create<DirectoryAccessor>())
        {
        }

        /// <inheritdoc/>
        public IEnumerable<DirectoryInfo> CleanDataAt(
            DirectoryInfo targetDirInfo,
            Func<DirectoryInfo, bool> subdirFilter,
            CancellationToken cancellationToken = default)
        {
            Guard.NotNull(targetDirInfo, nameof(targetDirInfo));
            Guard.NotNull(subdirFilter, nameof(subdirFilter));

            if (!targetDirInfo.Exists)
            {
                return Enumerable.Empty<DirectoryInfo>();
            }

            var deletedDirs = new List<DirectoryInfo>();
            foreach (DirectoryInfo subDir in targetDirInfo.GetDirectories())
            {
                cancellationToken.ThrowIfCancellationRequested();

                if (subdirFilter(subDir))
                {
                    this.logger.Info($"Deleting files in {subDir.FullName}");
                    subDir.Delete(true);
                    deletedDirs.Add(subDir);
                }
            }

            return deletedDirs;
        }

        /// <inheritdoc/>
        public void CleanData(DirectoryInfo dirInfo)
        {
            Guard.NotNull(dirInfo, nameof(dirInfo));

            if (dirInfo.Exists)
            {
                dirInfo.Delete(true);
            }
        }

        /// <inheritdoc/>
        public Task CopyStreamAsync(FileInfo destFileInfo, Stream stream, CancellationToken cancellationToken)
        {
            Guard.NotNull(destFileInfo, nameof(destFileInfo));
            Guard.NotNull(stream, nameof(stream));

            destFileInfo.Create();

            using (var deststream = new FileStream(
                destFileInfo.FullName,
                FileMode.Create,
                FileAccess.Write,
                FileShare.None,
                bufferSize,
                FileOptions.Asynchronous | FileOptions.SequentialScan))
            {
                return stream.CopyToAsync(deststream, defaultAsyncBufferSize, cancellationToken);
            }
        }
    }
}
