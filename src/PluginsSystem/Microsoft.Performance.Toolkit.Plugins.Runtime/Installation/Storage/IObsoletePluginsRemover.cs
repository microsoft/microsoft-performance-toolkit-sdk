// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.

using System.Threading;
using System.Threading.Tasks;

namespace Microsoft.Performance.Toolkit.Plugins.Runtime.Installation
{
    /// <summary>
    ///    Represents a component that can clean up obsolete (unregistered) plugin files.
    /// </summary>
    public interface IObsoletePluginsRemover
    {
        /// <summary>
        ///     Attempts to clean up all obsolete (unregistered) plugin files.
        ///     This method should be called safely by plugins consumers.
        /// </summary>
        /// <returns>
        ///     A task that completes when the operation is finished or is canceled.
        /// </returns>
        Task ClearObsoleteAsync(CancellationToken cancellationToken);
    }
}
