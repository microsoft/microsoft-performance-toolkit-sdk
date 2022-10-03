// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.

using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using Microsoft.Performance.SDK.Processing;

namespace Microsoft.Performance.Toolkit.Engine
{
    /// <summary>
    ///     Contains static (Shared in Visual Basic) methods for interacting
    ///     with <see cref="DataSourceSet"/> instances.
    /// </summary>
    public static class DataSourceSetExtensions
    {
        /// <summary>
        ///     Adds the given file to this instance for processing.
        /// </summary>
        /// <param name="self">
        ///     The <see cref="Engine"/> instance.
        /// </param>
        /// <param name="filePath">
        ///     The path to the file to process.
        /// </param>
        /// <exception cref="ArgumentNullException">
        ///     <paramref name="filePath"/> is <c>null</c>.
        /// </exception>
        /// <exception cref="ArgumentException">
        ///     <paramref name="filePath"/> is whitespace.
        /// </exception>
        /// <exception cref="FileNotFoundException">
        ///     The path specified by <paramref name="filePath"/> does not exist.
        /// </exception>
        /// <exception cref="InstanceAlreadyProcessedException">
        ///     This instance has already been processed.
        /// </exception>
        /// <exception cref="ObjectDisposedException">
        ///     This instance is disposed.
        /// </exception>
        /// <exception cref="UnsupportedDataSourceException">
        ///     <paramref name="filePath"/> cannot be processed by any
        ///     discovered extensions.
        /// </exception>
        public static void AddFile(this DataSourceSet self, string filePath)
        {
            self.AddDataSource(CreateFSDataSource(filePath));
        }

        /// <summary>
        ///     Adds the given file to this instance for processing by
        ///     the specific source.
        /// </summary>
        /// <param name="self">
        ///     The <see cref="Engine"/> instance.
        /// </param>
        /// <param name="filePath">
        ///     The path to the file to process.
        /// </param>
        /// <param name="processingSourceType">
        ///     The processing source to use to process the file.
        /// </param>
        /// <exception cref="ArgumentNullException">
        ///     <paramref name="filePath"/> is <c>null</c>.
        ///     - or -
        ///     <paramref name="processingSourceType"/> is <c>null</c>.
        /// </exception>
        /// <exception cref="ArgumentException">
        ///     <paramref name="filePath"/> is whitespace.
        /// </exception>
        /// <exception cref="FileNotFoundException">
        ///     The path specified by <paramref name="filePath"/> does not exist.
        /// </exception>
        /// <exception cref="InstanceAlreadyProcessedException">
        ///     This instance has already been processed.
        /// </exception>
        /// <exception cref="ObjectDisposedException">
        ///     This instance is disposed.
        /// </exception>
        /// <exception cref="UnsupportedDataSourceException">
        ///     The specified <paramref name="processingSourceType"/> cannot handle
        ///     the given file.
        /// </exception>
        /// <exception cref="UnsupportedProcessingSourceException">
        ///     The specified <paramref name="processingSourceType"/> is unknown.
        /// </exception>
        public static void AddFile(this DataSourceSet self, string filePath, Type processingSourceType)
        {
            self.AddDataSource(CreateFSDataSource(filePath), processingSourceType);
        }

        /// <summary>
        ///     Attempts to add the given file to this instance for processing.
        /// </summary>
        /// <param name="self">
        ///     The <see cref="Engine"/> instance.
        /// </param>
        /// <param name="filePath">
        ///     The path to the file to process.
        /// </param>
        /// <returns>
        ///     <c>true</c> if the file has been added for processing;
        ///     <c>false</c> if the file is not valid, cannot be processed,
        ///     or the instance has already been processed.
        /// </returns>
        /// <exception cref="ObjectDisposedException">
        ///     This instance is disposed.
        /// </exception>
        public static bool TryAddFile(this DataSourceSet self, string filePath)
        {
            return self.TryAddDataSource(CreateFSDataSource(filePath));
        }

        /// <summary>
        ///     Attempts to add the given file to this instance for processing by
        ///     the specific source.
        /// </summary>
        /// <param name="self">
        ///     The <see cref="Engine"/> instance.
        /// </param>
        /// <param name="filePath">
        ///     The path to the file to process.
        /// </param>
        /// <param name="processingSourceType">
        ///     The <see cref="IProcessingSource"/> to use to process the file.
        /// </param>
        /// <returns>
        ///     <c>true</c> if the file has been added for processing by this <see cref="IProcessingSource"/>;
        ///     <c>false</c> otherwise.
        /// </returns>
        /// <exception cref="ObjectDisposedException">
        ///     This instance is disposed.
        /// </exception>
        public static bool TryAddFile(this DataSourceSet self, string filePath, Type processingSourceType)
        {
            return self.TryAddDataSource(CreateFSDataSource(filePath), processingSourceType);
        }

        /// <summary>
        ///     Adds the given files to this instance for processing by
        ///     the specific source. All of the files will be processed
        ///     by the same instance of data processor. Use <see cref="AddFile(DataSourceSet, string, Type)"/>
        ///     to ensure each file is processed by a different instance, or
        ///     use multiple calls to <see cref="AddFiles(DataSourceSet, IEnumerable{string}, Type)"/>.
        /// </summary>
        /// <param name="self">
        ///     The <see cref="Engine"/> instance.
        /// </param>
        /// <param name="filePaths">
        ///     The path to the files to process.
        /// </param>
        /// <param name="processingSourceType">
        ///     The data source to use to process the file.
        /// </param>
        /// <exception cref="ArgumentNullException">
        ///     One or more paths specified by <paramref name="filePaths"/> is <c>null</c>.
        ///     - or -
        ///     <paramref name="processingSourceType"/> is <c>null</c>.
        /// </exception>
        /// <exception cref="ArgumentException">
        ///     One or more paths specified by <paramref name="filePaths"/> is whitespace.
        /// </exception>
        /// <exception cref="FileNotFoundException">
        ///     One or more paths specified by <paramref name="filePaths"/> does not exist.
        /// </exception>
        /// <exception cref="InstanceAlreadyProcessedException">
        ///     This instance has already been processed.
        /// </exception>
        /// <exception cref="ObjectDisposedException">
        ///     This instance is disposed.
        /// </exception>
        /// <exception cref="UnsupportedDataSourceException">
        ///     The specified <paramref name="processingSourceType"/> cannot handle
        ///     the given file.
        /// </exception>
        /// <exception cref="UnsupportedProcessingSourceException">
        ///     The specified <paramref name="processingSourceType"/> is unknown.
        /// </exception>
        public static void AddFiles(this DataSourceSet self, IEnumerable<string> filePaths, Type processingSourceType)
        {
            self.AddDataSources(filePaths.Select(CreateFSDataSource), processingSourceType);
        }


        /// <summary>
        ///     Attempts to add the given files to this instance. All of
        ///     the files will be processed by the same instance of 
        ///     data processor. Use <see cref="AddFile(DataSourceSet, string ,Type)"/> to ensure
        ///     each file is processed by a different instance.
        /// </summary>
        /// <param name="self">
        ///     The <see cref="Engine"/> instance.
        /// </param>
        /// <param name="filePaths">
        ///     The paths to the files to add.
        /// </param>
        /// <param name="processingSourceType">
        ///     The data source to use to process the file.
        /// </param>
        /// <returns>
        ///     <c>true</c> if the files are added;
        ///     <c>false</c> otherwise.
        /// </returns>
        /// <exception cref="ObjectDisposedException">
        ///     This instance is disposed.
        /// </exception>
        public static bool TryAddFiles(this DataSourceSet self, IEnumerable<string> filePaths, Type processingSourceType)
        {
            return self.TryAddDataSources(filePaths.Select(CreateFSDataSource), processingSourceType);
        }

        private static IDataSource CreateFSDataSource(string path)
        {
            var fullPath = Path.GetFullPath(path);
            if (Directory.Exists(fullPath))
            {
                return new DirectoryDataSource(fullPath);
            }

            return new FileDataSource(fullPath);
        }
    }
}
