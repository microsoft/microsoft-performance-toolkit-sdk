// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.

using System;
using System.IO;

namespace Microsoft.Performance.SDK.Processing
{
    /// <summary>
    ///     Represents a file backed data source for process.
    /// </summary>
    public class FileDataSource
        : DataSource
    {
        /// <summary>
        ///     Initializes a new <see cref="FileDataSource"/> with the specified file path.
        /// </summary>
        /// <param name="path">
        ///     The path to the file.
        /// </param>
        public FileDataSource(string path)
            : base(new Uri(Path.GetFullPath(path)))
        {
            Guard.NotNullOrWhiteSpace(path, nameof(path));

            this.FullPath = this.GetUri().LocalPath;
        }

        /// <summary>
        ///     Gets the full path of the file.
        /// </summary>
        public string FullPath { get; }

        /// <inheritdoc cref="System.Object.Equals(object)"/>
        public override bool Equals(object other)
        {
            if (other is null)
            {
                return false;
            }

            if (ReferenceEquals(this, other))
            {
                return true;
            }

            return other is FileDataSource fds &&
                this.FullPath.Equals(fds.FullPath);

        }

        /// <inheritdoc cref="System.Object.Equals(object)"/>
        public override int GetHashCode()
        {
            return this.FullPath.GetHashCode();
        }
    }
}
