﻿// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.

using System.IO;
using System.Threading.Tasks;
using Microsoft.Performance.SDK;
using Microsoft.Performance.Toolkit.Plugins.Core;

namespace Microsoft.Performance.Toolkit.Plugins.Runtime.Common
{
    /// <summary>
    ///     Represents a checksum provider that uses SHA256 to compute the checksum of a directory.
    /// </summary>
    public class SHA256DiretoryChecksumProvider
        : IChecksumProvider<DirectoryInfo>
    {
        public async Task<string> GetChecksumAsync(DirectoryInfo dirInfo)
        {
            Guard.NotNull(dirInfo, nameof(dirInfo));

            return await HashUtils.GetDirectoryHashAsync(dirInfo.FullName);
        }
    }
}
