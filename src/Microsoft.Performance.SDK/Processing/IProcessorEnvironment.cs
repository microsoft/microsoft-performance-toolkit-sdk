// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.

using System;

namespace Microsoft.Performance.SDK.Processing
{
    /// <summary>
    ///     Presents the services offered by the application for
    ///     a given session. You use this interface to interact with
    ///     the caller on a session by session basis.
    /// </summary>
    public interface IProcessorEnvironment
    {
        /// <summary>
        ///     Used to generate a processor-specific logger.
        /// </summary>
        ILogger CreateLogger(Type processorType);

        /// <summary>
        ///     Requests a new table builder from the application.
        ///     See <see cref="ITableBuilder.AddTableCommand"/>.
        ///     <para/>
        ///     Note that this is used to create tables from tables that already
        ///     exist in the UI (as created by <see cref="ICustomDataProcessor.BuildTable(TableDescriptor, ITableBuilder)"/>.
        ///     Scenarios where you would use this are in:
        ///     <list type="bullet">
        ///         <item>A table command to create a new table. See <see cref="ITableBuilder.AddTableCommand"/>.</item>
        ///     </list>
        /// </summary>
        /// <param name="descriptor">
        ///     The description of the table you are going to build. This descriptor
        ///     can be completely unique, and does not necessarily need to correspond
        ///     to an already known / existing table.
        /// </param>
        /// <returns>
        ///     A new table builder.
        /// </returns>
        /// <exception cref="System.ArgumentNullException">
        ///     <paramref name="descriptor"/> is <c>null</c>.
        /// </exception>
        /// <exception cref="System.InvalidOperationException">
        ///     The services are not in a state that allows for requesting new tables.
        /// </exception>
        IDynamicTableBuilder RequestDynamicTableBuilder(TableDescriptor descriptor);
    }

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
