// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.

using System;
using Microsoft.Performance.SDK.Extensibility;

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

        /// <summary>
        ///     Creates an instance of <see cref="IDataProcessorExtensibilitySupport"/> that is specific to this custom 
        ///     data processor. This is only used by custom data processors that support data extensions, implementing
        ///     <see cref="ICustomDataProcessorWithSourceParser"/>. This support object is used for internal data 
        ///     extension tables, such as a metadata table that requires a source data cooker.
        /// </summary>
        /// <param name="processor">
        ///     Custom data processor that supports data extensions. This should be the data processor to which this 
        ///     <see cref="IProcessorEnvironment"/> was passed.
        /// </param>
        /// <returns>
        ///     An instance of <see cref="IDataProcessorExtensibilitySupport"/>.
        /// </returns>
        IDataProcessorExtensibilitySupport CreateDataProcessorExtensibilitySupport(ICustomDataProcessorWithSourceParser processor);
    }
}
