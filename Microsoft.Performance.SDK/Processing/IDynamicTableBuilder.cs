// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.

namespace Microsoft.Performance.SDK.Processing
{
    public interface IDynamicTableBuilder
        : ITableBuilder
    {
        /// <summary>
        ///     Adds a new table to the UI. This method is used in conjunction with
        ///     <see cref="IProcessorEnvironment.RequestDynamicTableBuilder(TableDescriptor, DataSourceInfo)"/> to add brand new
        ///     tables.
        /// </summary>
        /// <param name="option">
        ///     Specifies where to add the new table.
        /// </param>
        /// <exception cref="System.InvalidOperationException">
        ///     This object was not created by
        ///     <see cref="IProcessorEnvironment.RequestDynamicTableBuilder(TableDescriptor, DataSourceInfo)"/>.
        /// </exception>
        void AddDynamicTable(AddNewTableOption option);
    }
}
