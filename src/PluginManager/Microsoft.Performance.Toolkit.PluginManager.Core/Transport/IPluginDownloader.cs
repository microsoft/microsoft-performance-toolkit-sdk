using System.IO;
// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.

using System.Threading.Tasks;
using System.Threading;
using System;

namespace Microsoft.Performance.Toolkit.PluginManager.Core.Transport
{
    /// <summary>
    /// 
    /// </summary>
    public interface IPluginDownloader
    {
        Task<Stream> DownloadPluginAsync(
            PluginIdentity identity,
            IProgress<int> progress,
            CancellationToken cancellationToken);
    }
}
