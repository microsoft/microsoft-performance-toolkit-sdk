// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.

using System;
using Microsoft.Performance.SDK.Extensibility;

namespace Microsoft.Performance.SDK.Processing
{
    /// <summary>
    ///     Represents a table discovered by an <see cref="ITableProvider"/>
    ///     instance.
    /// </summary>
    public sealed class DiscoveredTable
    {
        /// <summary>
        ///     Initializes a new instance of the <see cref="DiscoveredTable"/>
        ///     class, indicating that the table with the given <see cref="TableDescriptor"/>
        ///     exists, and uses the default build action.
        /// </summary>
        /// <param name="descriptor">
        ///     The descriptor of the table.
        /// </param>
        /// <exception cref="System.ArgumentNullException">
        ///     <paramref name="descriptor"/> is <c>null</c>.
        /// </exception>
        public DiscoveredTable(TableDescriptor descriptor)
            : this(descriptor, null)
        {
        }

        /// <summary>
        ///     Initializes a new instance of the <see cref="DiscoveredTable"/>
        ///     class, indicating that the table with the given <see cref="TableDescriptor"/>
        ///     exists, and is built via the specified build action.
        /// </summary>
        /// <param name="descriptor">
        ///     The descriptor of the table.
        /// </param>
        /// <param name="buildTable">
        ///     The build action for the table. This parameter may be <c>null</c> to specify
        ///     that the default build action should be used.
        /// </param>
        /// <exception cref="System.ArgumentNullException">
        ///     <paramref name="descriptor"/> is <c>null</c>.
        /// </exception>
        public DiscoveredTable(
            TableDescriptor descriptor,
            Action<ITableBuilder, IDataExtensionRetrieval> buildTable)
        {
            Guard.NotNull(descriptor, nameof(descriptor));

            this.Descriptor = descriptor;
            this.BuildTable = buildTable;
        }

        /// <summary>
        ///     Gets the <see cref="TableDescriptor"/> of the table.
        /// </summary>
        public TableDescriptor Descriptor { get; }

        /// <summary>
        ///     Gets the build action of the table. This property may be <c>null</c>.
        /// </summary>
        public Action<ITableBuilder, IDataExtensionRetrieval> BuildTable { get; }

        /// <summary>
        ///     Gets a value indicating whether this table is a metadata table.
        /// </summary>
        public bool IsMetadata => this.Descriptor.IsMetadataTable;
    }
}
