// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.

using System.IO;

namespace Microsoft.Performance.SDK.Processing
{
    /// <summary>
    ///     Contains static (Shared in Visual Basic) methods for interacting
    ///     with <see cref="IDataSource"/> instances.
    /// </summary>
    public static class DataSourceExtensions
    {
        /// <summary>
        ///     Determines whether the given <see cref="IDataSource"/> represents
        ///     a file.
        /// </summary>
        /// <param name="dataSource">
        ///     The Data Source.
        /// </param>
        /// <returns>
        ///     <c>true</c> if the <see cref="IDataSource"/>represents a file;
        ///     <c>false</c> otherwise.
        /// </returns>
        public static bool IsFile(this IDataSource dataSource)
        {
            return dataSource is FileDataSource;
        }

        /// <summary>
        ///     Determines whether the given <see cref="IDataSource"/> represents
        ///     an extensionless file.
        /// </summary>
        /// <param name="dataSource">
        ///     The Data Source.
        /// </param>
        /// <returns>
        ///     <c>true</c> if the <see cref="IDataSource"/> represents an extensionless
        ///     file; <c>false</c> otherwise.
        /// </returns>
        public static bool IsExtensionlessFile(this IDataSource dataSource)
        {
            return (dataSource is FileDataSource fd) &&
                   (Path.GetExtension(fd.FullPath) == string.Empty);
        }

        /// <summary>
        ///     Determines whether the given <see cref="IDataSource"/> represents
        ///     a directory.
        /// </summary>
        /// <param name="dataSource">
        ///     The Data Source.
        /// </param>
        /// <returns>
        ///     <c>true</c> if the <see cref="IDataSource"/> represents a directory;
        ///     <c>false</c> otherwise.
        /// </returns>
        public static bool IsDirectory(this IDataSource dataSource)
        {
            return dataSource is DirectoryDataSource;
        }
    }
}
