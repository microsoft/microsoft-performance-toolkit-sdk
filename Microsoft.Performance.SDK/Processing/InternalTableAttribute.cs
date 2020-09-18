// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.

namespace Microsoft.Performance.SDK.Processing
{
    /// <summary>
    /// This derives from <see cref="TableAttribute"/> and automatically sets the InternalTable
    /// property to true.
    /// </summary>
    public class InternalTableAttribute
        : TableAttribute
    {
        /// <summary>
        ///     Initializes a new instance of the <see cref="TableAttribute"/> class.
        /// </summary>
        /// <param name="tableDescriptorPropertyName">
        ///     Name of the static class property which returns the
        ///     <see cref="TableDescriptor"/>
        /// </param>
        /// <param name="buildTableActionMethodName">
        ///     The name of the static class method which builds a table.
        ///     BuildTable(Action&lt;ITableBuilder, object&gt;)
        /// </param>
        /// <param name="isDataAvailableMethodName">
        ///     The name of the static class method which checks if the table has data.
        ///     IsDataAvailable(IDataExtensionRetrieval)
        /// </param>
        public InternalTableAttribute(
            string tableDescriptorPropertyName = DefaultTableDescriptorPropertyName,
            string buildTableActionMethodName = DefaultTableBuilderMethodName,
            string isDataAvailableFuncMethodName = DefaultIsDataAvailableMethodName)
            : base(tableDescriptorPropertyName, buildTableActionMethodName, isDataAvailableFuncMethodName, true)
        {
        }
    }
}
