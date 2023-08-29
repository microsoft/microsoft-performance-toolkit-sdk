// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.

namespace Microsoft.Performance.Toolkit.Plugins.Cli
{
    /// <summary>
    ///     Contains utility methods.
    /// </summary>
    public static class Utils
    {
        /// <summary>
        ///     Gets an alternate file path for the given file path if the given file path already exists in the file system.
        /// </summary>
        /// <param name="fullfilePath">
        ///     The full file path to get an alternate file path for.
        /// </param>
        /// <returns>
        ///     An alternate file path for the given file path or the given file path if it does not already exist in the file system.
        /// </returns>
        public static string GetAlterDestFilePath(string fullfilePath)
        {
            string? directory = Path.GetDirectoryName(fullfilePath);
            string name = Path.GetFileNameWithoutExtension(fullfilePath);
            string extension = Path.GetExtension(fullfilePath);

            string alterFileName = fullfilePath;

            int fileCount = 1;
            while (File.Exists(alterFileName))
            {
                alterFileName = Path.Combine(directory!, $"{name}_({fileCount++}){extension}");
            }

            return alterFileName;
        }
    }
}
