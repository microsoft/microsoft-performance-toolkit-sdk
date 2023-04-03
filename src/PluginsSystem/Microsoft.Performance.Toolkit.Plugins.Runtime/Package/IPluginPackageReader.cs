// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.

using System.IO;
using System.Threading;
using System.Threading.Tasks;

namespace Microsoft.Performance.Toolkit.Plugins.Runtime.Package
{
    public interface IPluginPackageReader
    {
        Task<PluginPackage> TryReadPackageAsync(
            Stream stream,
            CancellationToken cancellationToken);
    }
}