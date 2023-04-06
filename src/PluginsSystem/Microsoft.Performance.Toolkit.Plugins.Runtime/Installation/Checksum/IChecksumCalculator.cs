// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.

using System.Threading.Tasks;

namespace Microsoft.Performance.Toolkit.Plugins.Runtime
{
    /// <summary>
    ///     Represents a checksum calculator.
    /// </summary>
    public interface IChecksumCalculator
    {
        /// <summary>
        ///     Calculates the checksum of the given file directory.
        /// </summary>
        /// <param name="directory">
        ///     The directory to calculate the checksum of.
        /// </param>
        /// <returns>
        ///     The checksum of the given directory.
        /// </returns>
        Task<string> GetDirectoryChecksumAsync(string directory);
    }
}
