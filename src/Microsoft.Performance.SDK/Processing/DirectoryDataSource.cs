// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.

using System;
using System.IO;

namespace Microsoft.Performance.SDK.Processing
{
    /// <summary>
    ///     Represents a directory backed data source for processing.
    /// </summary>
    public class DirectoryDataSource
        : DataSource
    {
        /// <summary>
        ///     Initializes a new <see cref="DirectoryDataSource"/> with the specified
        ///     directory path.
        /// </summary>
        /// <param name="path">
        ///     The path to the directory.
        /// </param>
        public DirectoryDataSource(string path)
            : base(new Uri(Path.GetFullPath(path)))
        {
            this.FullPath = this.Uri.LocalPath;
        }

        /// <summary>
        ///     Gets the full path of the directory.
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

            return other is DirectoryDataSource dds &&
                this.FullPath.Equals(dds.FullPath);

        }

        /// <inheritdoc cref="System.Object.Equals(object)"/>
        public override int GetHashCode()
        {
            return this.FullPath.GetHashCode();
        }
    }
}
