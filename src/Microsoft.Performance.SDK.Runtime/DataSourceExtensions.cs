// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.

using Microsoft.Performance.SDK.Processing;

namespace Microsoft.Performance.SDK.Runtime
{
    /// <summary>
    ///     Contains static (Shared in Visual Basic) methods for interacting
    ///     with <see cref="IDataSource"/> instances.
    /// </summary>
    public static class DataSourceExtensions
    {
        /// <summary>
        ///     Determines whether the given Data Source represents
        ///     a file.
        /// </summary>
        /// <param name="dataSource">
        ///     The Data Source.
        /// </param>
        /// <returns>
        ///     <c>true</c> if the Data Source represents a file;
        ///     <c>false</c> otherwise.
        /// </returns>
        public static bool IsFile(this IDataSource dataSource)
        {
            return dataSource is FileDataSource;
        }

        /// <summary>
        ///     Determines whether the given Data Source represents
        ///     a directory.
        /// </summary>
        /// <param name="dataSource">
        ///     The Data Source.
        /// </param>
        /// <returns>
        ///     <c>true</c> if the Data Source represents a directory;
        ///     <c>false</c> otherwise.
        /// </returns>
        public static bool IsDirectory(this IDataSource dataSource)
        {
            return dataSource is DirectoryDataSource;
        }
    }
}
