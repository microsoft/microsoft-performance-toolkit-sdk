// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.

using System;
using Microsoft.Performance.SDK.Processing;

namespace Microsoft.Performance.SDK.Runtime
{
    /// <summary>
    ///     Represents a file backed data source for process.
    /// </summary>
    public class FileDataSource
        : IDataSource
    {
        private readonly Uri fullPathUri;

        /// <summary>
        ///     Initializes a new <see cref="FileDataSource"/> with the specified file path.
        /// </summary>
        /// <param name="fullPath">
        ///     Full path to the file.
        /// </param>
        public FileDataSource(string fullPath)
        {
            Guard.NotNullOrWhiteSpace(fullPath, nameof(fullPath));

            this.fullPathUri = new Uri(fullPath);
            this.FullPath = fullPath;
        }

        /// <summary>
        ///     Gets the full path of the file.
        /// </summary>
        public string FullPath { get; }

        /// <summary>
        ///     Returns the <see cref="Uri" /> of the file.
        /// </summary>
        /// <returns></returns>
        public Uri GetUri()
        {
            return this.fullPathUri;
        }
    }
}
