// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.

using Microsoft.Performance.SDK.Processing;
using System.Collections.Generic;

namespace Microsoft.Performance.SDK.Runtime
{
    /// <summary>
    ///     Contains all the detail information of a row in the table that can be shown in the Table Details UI.
    /// </summary>
    public sealed class TableRowDetails
    {
        /// <summary>
        ///     Initialize a new instance of the <see cref="TableRowDetails"/> class.
        /// </summary>
        /// <param name="rowIndex"> The row number of this instance. </param>.
        /// <param name="tableRowDetailEntries"> A collection of <see cref="TableRowDetailEntry"/>
        ///     containing information of this row </param>
        /// <exception cref="System.ArgumentNullException">
        ///     <paramref name="tableRowDetailEntries"/> is <c>null</c>.
        /// </exception>
        public TableRowDetails(int rowIndex, IReadOnlyCollection<TableRowDetailEntry> tableRowDetailEntries)
        {
            Guard.NotNull(tableRowDetailEntries, nameof(tableRowDetailEntries));

            this.RowIndex = rowIndex;
            this.TableRowDetailEntries = tableRowDetailEntries;
        }

        /// <summary>
        ///     Gets the row number of this instance.
        /// </summary>
        public int RowIndex { get; }

        /// <summary>
        ///     Gets a collection of <see cref="TableRowDetailEntry"/>
        ///     containing information of this row.
        /// </summary>
        public IReadOnlyCollection<TableRowDetailEntry> TableRowDetailEntries { get; }
    }
}
