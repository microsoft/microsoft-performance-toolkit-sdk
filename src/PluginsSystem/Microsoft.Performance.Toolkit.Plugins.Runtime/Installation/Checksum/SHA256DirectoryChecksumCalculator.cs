// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.

using System.Threading.Tasks;
using Microsoft.Performance.Toolkit.Plugins.Core;

namespace Microsoft.Performance.Toolkit.Plugins.Runtime.Installation
{
    /// <summary>
    ///     Represents a checksum calculator that uses SHA256.
    /// </summary>
    public sealed class SHA256DirectoryChecksumCalculator
        : IDirectoryChecksumCalculator
    {
        /// <inheritdoc />
        public async Task<string> GetDirectoryChecksumAsync(string directory)
        {
            return await HashUtils.GetDirectoryHashAsync(directory);
        }
    }
}
