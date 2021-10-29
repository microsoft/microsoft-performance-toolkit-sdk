// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.

namespace Microsoft.Performance.SDK.Processing
{
    /// <summary>
    ///     Defines the interface for building dynamic tables.
    ///     <para />
    ///     For more information on dynamic tables, please refer to
    ///     <see cref="IProcessorEnvironment.RequestDynamicTableBuilder(TableDescriptor)"/>.
    /// </summary>
    public interface IDynamicTableBuilder
        : ITableBuilder
    {
        /// <summary>
        ///     Adds a new table to the UI. This method is used in conjunction with
        ///     <see cref="IProcessorEnvironment.RequestDynamicTableBuilder(TableDescriptor)"/> to add brand new
        ///     tables.
        /// </summary>
        /// <param name="option">
        ///     Specifies where to add the new table.
        /// </param>
        /// <exception cref="System.InvalidOperationException">
        ///     This object was not created by
        ///     <see cref="IProcessorEnvironment.RequestDynamicTableBuilder(TableDescriptor)"/>.
        /// </exception>
        void AddDynamicTable(AddNewTableOption option);
    }
}
