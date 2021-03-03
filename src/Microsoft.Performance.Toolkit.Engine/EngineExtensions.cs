// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.

using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using Microsoft.Performance.SDK.Processing;
using Microsoft.Performance.SDK.Runtime;

namespace Microsoft.Performance.Toolkit.Engine
{
    /// <summary>
    ///     Contains static (Shared in Visual Basic) methods for interacting
    ///     with <see cref="Engine"/> instances.
    /// </summary>
    public static class EngineExtensions
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
        /// <exception cref="UnsupportedDataSourceException">
        ///     <paramref name="filePath"/> cannot be processed by any
        ///     discovered extensions.
        /// </exception>
        public static void AddFile(this Engine self, string filePath)
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
        /// <param name="dataSourceType">
        ///     The data source to use to process the file.
        /// </param>
        /// <exception cref="ArgumentNullException">
        ///     <paramref name="filePath"/> is <c>null</c>.
        ///     - or -
        ///     <paramref name="customDataSourceType"/> is <c>null</c>.
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
        /// <exception cref="UnsupportedDataSourceException">
        ///     The specified <paramref name="dataSourceType"/> cannot handle
        ///     the given file.
        /// </exception>
        /// <exception cref="UnsupportedCustomDataSourceException">
        ///     The specified <paramref name="dataSourceType"/> is unknown.
        /// </exception>
        public static void AddFile(this Engine self, string filePath, Type customDataSourceType)
        {
            self.AddDataSource(CreateFSDataSource(filePath), customDataSourceType);
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
        ///     or the instance has already been processed. Note that <c>false</c>
        ///     is always returned when <see cref="IsProcessed"/> is <c>true</c>.
        /// </returns>
        public static bool TryAddFile(this Engine self, string filePath)
        {
            try
            {
                self.AddDataSource(CreateFSDataSource(filePath));
                return true;
            }
            catch
            {
                return false;
            }
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
        /// <param name="customDataSourceType">
        ///     The Custom Data Source to use to process the file.
        /// </param>
        /// <returns>
        ///     <c>true</c> if the file has been added for processing by this Custom Data Source;
        ///     <c>false</c> otherwise. Note that <c>false</c>
        ///     is always returned when <see cref="IsProcessed"/> is <c>true</c>.
        /// </returns>
        public static bool TryAddFile(this Engine self, string filePath, Type customDataSourceType)
        {
            try
            {
                self.AddDataSource(CreateFSDataSource(filePath), customDataSourceType);
                return true;
            }
            catch
            {
                return false;
            }
        }

        /// <summary>
        ///     Adds the given files to this instance for processing by
        ///     the specific source. All of the files will be processed
        ///     by the same instance of data processor. Use <see cref="AddFile(string, Type)"/>
        ///     to ensure each file is processed by a different instance, or
        ///     use multiple calls to <see cref="AddFiles(IEnumerable{string}, Type)"/>.
        /// </summary>
        /// <param name="self">
        ///     The <see cref="Engine"/> instance.
        /// </param>
        /// <param name="filePaths">
        ///     The path to the files to process.
        /// </param>
        /// <param name="customDataSourceType">
        ///     The data source to use to process the file.
        /// </param>
        /// <exception cref="ArgumentNullException">
        ///     One or more paths specified by <paramref name="filePaths"/> is <c>null</c>.
        ///     - or -
        ///     <paramref name="customDataSourceType"/> is <c>null</c>.
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
        /// <exception cref="UnsupportedDataSourceException">
        ///     The specified <paramref name="customDataSourceType"/> cannot handle
        ///     the given file.
        /// </exception>
        /// <exception cref="UnsupportedCustomDataSourceException">
        ///     The specified <paramref name="customDataSourceType"/> is unknown.
        /// </exception>
        public static void AddFiles(this Engine self, IEnumerable<string> filePaths, Type customDataSourceType)
        {
            self.AddDataSources(filePaths.Select(CreateFSDataSource), customDataSourceType);
        }

        /// <summary>
        ///     Attempts to add the given files to this instance. All of
        ///     the files will be processed by the same instance of 
        ///     data processor. Use <see cref="AddFile"/> to ensure
        ///     each file is processed by a different instance.
        /// </summary>
        /// <param name="self">
        ///     The <see cref="Engine"/> instance.
        /// </param>
        /// <param name="filePaths">
        ///     The paths to the files to add.
        /// </param>
        /// <returns>
        ///     <c>true</c> if the files are added;
        ///     <c>false</c> otherwise.
        /// </returns>
        public static bool TryAddFiles(this Engine self, IEnumerable<string> filePaths, Type customDataSourceType)
        {
            try
            {
                self.AddDataSources(filePaths.Select(CreateFSDataSource), customDataSourceType);
                return true;
            }
            catch
            {
                return false;
            }
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
