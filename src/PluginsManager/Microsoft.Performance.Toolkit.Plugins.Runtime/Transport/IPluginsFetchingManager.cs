// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.

using System;
using System.IO;
using System.Threading;
using System.Threading.Tasks;

namespace Microsoft.Performance.Toolkit.Plugins.Runtime.Transport
{
    public interface IPluginsFetchingManager
    {
        Task<Stream> FetchPluginStream(
            AvailablePlugin plugin,
            CancellationToken cancellationToken,
            IProgress<int> progress);
    }
}
