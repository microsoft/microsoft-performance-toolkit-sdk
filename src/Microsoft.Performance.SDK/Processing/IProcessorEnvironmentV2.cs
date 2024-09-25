// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.

namespace Microsoft.Performance.SDK.Processing
{
    /// <summary>
    ///     Presents the services offered by the application for
    ///     a given session. You use this interface to interact with
    ///     the caller on a session by session basis.
    /// </summary>
    public interface IProcessorEnvironmentV2
        : IProcessorEnvironment
    {
        /// <summary>
        ///     Provides the interface to be used to notify that data in
        ///     a table has changed.
        ///     <para/>
        ///     This may be <c>null</c>.
        /// </summary>
        ITableDataSynchronizationV2 TableDataSynchronizer { get; }
    }
}
