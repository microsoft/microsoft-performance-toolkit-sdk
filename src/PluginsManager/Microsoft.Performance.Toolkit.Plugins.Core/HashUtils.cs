// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.

using System;
using System.IO;
using System.Security.Cryptography;
using System.Text;
using System.Threading.Tasks;
using Microsoft.Performance.SDK;

namespace Microsoft.Performance.Toolkit.Plugins.Core
{
    /// <summary>
    ///     Provides static methods for calculating hash.
    /// </summary>
    public static class HashUtils
    {
        /// <summary>
        ///     Calculates the hash of all files under the given directory including the file paths.
        /// </summary>
        /// <param name="directory">
        ///     Path to the directory.
        /// </param>
        /// <returns>
        ///     The combined SHA256 hash of all files including the file paths.
        /// </returns>
        public static async Task<string> GetDirectoryHash(string directory)
        {
            Guard.NotNull(directory, nameof(directory));
            Guard.IsTrue(Directory.Exists(directory), $"Directory {directory} doesn't exist.");

            using (var sha256 = SHA256.Create())
            using (var cs = new CryptoStream(Stream.Null, sha256, CryptoStreamMode.Write))
            {
                foreach (string filePath in Directory.EnumerateFiles(directory))
                {
                    // Hash file paths
                    byte[] pathBytes = Encoding.UTF8.GetBytes(filePath);
                    cs.Write(pathBytes, 0, pathBytes.Length);

                    // Hash file content
                    using (FileStream stream = File.OpenRead(filePath))
                    {
                        await stream.CopyToAsync(cs);
                    }
                }

                cs.FlushFinalBlock();
                return BitConverter.ToString(sha256.Hash).Replace("-", "").ToLower();
            }
        }
    }
}
