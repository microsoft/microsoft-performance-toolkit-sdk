// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.

using System;
using Microsoft.Performance.SDK.Processing;

namespace Microsoft.Performance.SDK.Runtime
{
    /// <summary>
    ///     Enables building of dynamic tables.
    ///     <para />
    ///     Dynamic tables are normally created at runtime after processing the data source.
    /// </summary>
    public class DynamicTableBuilder
        : TableBuilder,
          IDynamicTableBuilder
    {
        private readonly Action<IDynamicTableBuilder> buildNewTable;

        /// <param name="buildNewTable">
        ///     Build table action.
        /// </param>
        /// <param name="tableDescriptor">
        ///     <see cref="TableDescriptor"/> for the dynamic table.
        /// </param>
        /// <param name="dataSourceInfo">
        ///     <see cref="DataSourceInfo"/> associated with the dynamic table.
        /// </param>
        public DynamicTableBuilder(
            Action<IDynamicTableBuilder> buildNewTable, 
            TableDescriptor tableDescriptor,
            DataSourceInfo dataSourceInfo)
        {
            Guard.NotNull(buildNewTable, nameof(buildNewTable));
            Guard.NotNull(tableDescriptor, nameof(tableDescriptor));
            Guard.NotNull(dataSourceInfo, nameof(dataSourceInfo));

            this.buildNewTable = buildNewTable;
            this.DataSourceInfo = dataSourceInfo;
            this.TableDescriptor = tableDescriptor;
        }

        /// <summary>
        ///     Description of the table to be created.
        /// </summary>
        public TableDescriptor TableDescriptor { get; }

        /// <summary>
        ///     Data about the source to which the table will belong.
        /// </summary>
        public DataSourceInfo DataSourceInfo { get; }

        /// <summary>
        ///     Options for table creation.
        /// </summary>
        public AddNewTableOption NewTableOption { get; private set; }

        /// <summary>
        ///     Create the new table
        /// </summary>
        /// <param name="option">
        ///     Options for table creation.
        /// </param>
        public void AddDynamicTable(AddNewTableOption option)
        {
            this.NewTableOption = option;
            this.buildNewTable(this);
        }
    }
}
