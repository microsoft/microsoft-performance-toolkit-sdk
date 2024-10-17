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
        ///     Gets a factory to creates an instance of <see cref="ITableDataSynchronization"/> specific to the 
        ///     processor.
        ///     <para/>
        ///     This may be <c>null</c>.
        /// </summary>
        IProcessorTableDataSynchronizationFactory TableDataSynchronizerFactory { get; }
    }
}
