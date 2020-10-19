// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.

using System;
using System.Collections.Generic;

namespace Microsoft.Performance.SDK.Processing
{
    /// <summary>
    ///     Provides a way to synchronize data changes in columns with the user interface displaying the columns.
    /// </summary>
    public interface ITableDataSynchronization
    {
        /// <summary>
        ///     The data of these columns is changing and that should be reflected by the user interface.
        ///     Note that the second action, if provided, will only be executed once all tables affected
        ///     by the first action are updated and ready. It's the equivalent of calling this twice, with
        ///     the guarantee that the operations execute sequentially.
        /// </summary>
        /// <remarks>
        ///     This is a multi-step process to ensure data read from tables is synchronized with changes:
        ///     1. Lock tables affected by the change (based on <paramref name="columns"/>) and wait
        ///        for all existing operations on those tables to complete.
        ///     2. Execute <param name="onReadyForChange"/> on a background thread.
        ///     3. On UI-thread, notify all affected columns that data has changed and re-apply the preset.
        ///        Note that this may result in work on background threads for sorting/grouping.
        ///     4. Mark table as ready.
        ///     5. Execute <param name="onChangeComplete"/>.
        /// </remarks>
        /// <param name="columns">
        ///     Identifiers for the columns that are changing.
        /// </param>
        /// <param name="onReadyForChange">
        ///     Callback when the tables are ready for the change.
        /// </param>
        /// <param name="onChangeComplete">
        ///     Callback when the tables are finished updated post change.
        /// </param>
        /// <param name="requestInitialFilterReevaluation">
        ///     Reset initial filter from table configuration.
        ///     <seealso cref="TableConfiguration.InitialFilterQuery"/>
        /// </param>
        void SubmitColumnChangeRequest(
            IEnumerable<Guid> columns,
            Action onReadyForChange,
            Action onChangeComplete,
            bool requestInitialFilterReevaluation = false);

        /// <summary>
        ///     The data of these columns is changing and that should be reflected by the user interface.
        ///     Note that the second action, if provided, will only be executed once all tables affected
        ///     by the first action are updated and ready. It's the equivalent of calling this twice, with
        ///     the guarantee that the operations execute sequentially.
        /// </summary>
        /// <remarks>
        ///     This is a multi-step process to ensure data read from tables is synchronized with changes:
        ///     1. Lock tables affected by the change (based on <paramref name="predicate"/>) and wait
        ///        for all existing operations on those tables to complete.
        ///     2. Execute <param name="onReadyForChange"/> on a background thread.
        ///     3. On UI-thread, notify all affected columns that data has changed and re-apply the preset.
        ///        Note that this may result in work on background threads for sorting/grouping.
        ///     4. Mark table as ready.
        ///     5. Execute <param name="onChangeComplete"/>.
        /// </remarks>
        /// <param name="predicate">
        ///     A predicate used to decide which projections are changing.
        /// </param>
        /// <param name="onReadyForChange">
        ///     Callback when the tables are ready for the change.
        /// </param>
        /// <param name="onChangeComplete">
        ///     Callback when the tables are finished updated post change.
        /// </param>
        /// <param name="requestInitialFilterReevaluation">
        ///     Reset initial filter from table configuration.
        /// </param>
        void SubmitColumnChangeRequest(
            Func<IProjectionDescription, bool> predicate,
            Action onReadyForChange,
            Action onChangeComplete,
            bool requestInitialFilterReevaluation = false);
    }
}
