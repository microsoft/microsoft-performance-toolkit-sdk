using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.Performance.SDK.Processing;
using Microsoft.Performance.SDK.Runtime;

namespace Microsoft.Performance.Toolkit.Plugins.Runtime.Common
{
    public class DirectoryAccessor
        : IDirectoryAccessor
    {
        private readonly ILogger logger;

        const int bufferSize = 4096;
        const int defaultAsyncBufferSize = 81920;

        public DirectoryAccessor(ILogger logger)
        {
            this.logger = logger;
        }

        public DirectoryAccessor()
            : this(Logger.Create<DirectoryAccessor>())
        {
        }

        public IEnumerable<DirectoryInfo> CleanData(DirectoryInfo targetDir, Func<DirectoryInfo, bool> subdirFilter = null, CancellationToken cancellationToken = default)
        {
            if (!targetDir.Exists)
            {
                return Enumerable.Empty<DirectoryInfo>();
            }

            if (subdirFilter == null)
            {
                this.logger.Info($"Deleting files in {targetDir.FullName}");
                targetDir.Delete(true);
                return new[] { targetDir };
            }

            var deletedDirs = new List<DirectoryInfo>();
            foreach (DirectoryInfo subDir in targetDir.GetDirectories())
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

        public Task CopyStreamAsync(FileInfo destFile, Stream stream, CancellationToken cancellationToken)
        {
            destFile.Create();

            using (var deststream = new FileStream(
                destFile.FullName,
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
