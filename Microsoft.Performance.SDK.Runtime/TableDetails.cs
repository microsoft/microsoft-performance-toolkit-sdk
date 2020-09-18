// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.

using System.Collections.Generic;

namespace Microsoft.Performance.SDK.Runtime
{
    /// <summary>
    ///     Represents the details information for the selected rows of a table.
    /// </summary>
    public sealed class TableDetails
    {
        /// <summary>
        ///      Initializes a new instance of the <see cref="TableDetails"/>
        /// </summary>
        /// <param name="tableRowDetailsCollection">A collection of <see cref="TableRowDetails"/>
        ///      that represents the detail information of the selected rows in the UI.
        /// </param>
        /// <exception cref="System.ArgumentNullException">
        ///     <paramref name="tableRowDetailsCollection"/> is <c>null</c>.
        /// </exception>
        public TableDetails(IReadOnlyCollection<TableRowDetails> tableRowDetailsCollection)
        {
            Guard.NotNull(tableRowDetailsCollection, nameof(tableRowDetailsCollection));

            this.TableRowDetailsCollection = tableRowDetailsCollection;
        }

        /// <summary>
        ///     Gets the collection of <see cref="TableRowDetails"/>.
        ///     that represents the detail information of the selected rows in the UI.
        /// </summary>
        public IReadOnlyCollection<TableRowDetails> TableRowDetailsCollection { get; }

        /// <summary>
        ///     Gets the count of the elements in <see cref="TableRowDetailsCollection"/>.
        /// </summary>
        public int Count => this.TableRowDetailsCollection.Count;
    }
}
