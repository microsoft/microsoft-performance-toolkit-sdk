// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.

using System.Threading.Tasks;
using System.Threading;

namespace Microsoft.Performance.Toolkit.PluginsManager.Core.Registry
{
    public interface ISynchronizedObject
    {
        Task AcquireLock(CancellationToken cancellationToken);

        void ReleaseLock();
    }
}
