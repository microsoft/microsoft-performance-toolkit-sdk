// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.

using System;

namespace Microsoft.Performance.SDK.Processing
{
    /// <summary>
    ///     Denotes a class as implementing a data table.
    /// </summary>
    [AttributeUsage(AttributeTargets.Class, AllowMultiple = false, Inherited = false)]
    public class TableAttribute
        : Attribute
    {
        /// <summary>
        ///     The default name of the static class property which returns the <see cref="TableDescriptor"/>.
        /// </summary>
        protected const string DefaultTableDescriptorPropertyName = "TableDescriptor";

        /// <summary>
        ///     The default name of the static class method which builds a table.
        ///     BuildTable(Action&lt;ITableBuilder, IDataExtensionRetrieval&gt;)
        /// </summary>
        protected const string DefaultTableBuilderMethodName = "BuildTable";

        /// <summary>
        ///     The default name of the static class method which checks if a table has data.
        ///     bool IsDataAvailable(IDataExtensionRetrieval)
        /// </summary>
        protected const string DefaultIsDataAvailableMethodName = "IsDataAvailable";

        public TableAttribute(
            bool internalTable = false)
            : this(DefaultTableDescriptorPropertyName, DefaultTableBuilderMethodName, DefaultIsDataAvailableMethodName, internalTable)
        {
        }

        /// <summary>
        ///     Initializes a new instance of the <see cref="TableAttribute"/> class.
        /// </summary>
        /// <param name="tableDescriptorPropertyName">
        ///     Name of the static class property which returns the
        ///     <see cref="TableDescriptor"/>
        /// </param>
        /// <param name="buildTableActionMethodName">
        ///     The name of the static class method which builds a table.
        ///     BuildTable(Action&lt;ITableBuilder, IDataExtensionRetrieval&gt;)
        /// </param>
        /// <param name="isDataAvailableMethodName">
        ///     The name of the static class method which checks if the table has data.
        ///     IsDataAvailable(IDataExtensionRetrieval)
        /// </param>
        /// <param name="InternalTable">
        ///     When this is set, the table will not be exposed through the data extension repository.
        ///     Note that the <see cref="IProcessingSource"/> that owns this will be required to create this table
        ///     when this is set.
        /// </param>
        protected TableAttribute(
            string tableDescriptorPropertyName = DefaultTableDescriptorPropertyName,
            string buildTableActionMethodName = DefaultTableBuilderMethodName,
            string isDataAvailableMethodName = DefaultIsDataAvailableMethodName,
            bool internalTable = false)
        {
            Guard.NotNullOrWhiteSpace(tableDescriptorPropertyName, nameof(tableDescriptorPropertyName));

            this.TableDescriptorPropertyName = tableDescriptorPropertyName;
            this.BuildTableActionMethodName = buildTableActionMethodName;
            this.IsDataAvailableMethodName = isDataAvailableMethodName;
            this.InternalTable = internalTable;
        }

        /// <summary>
        ///     When this is set, the table will not be exposed through the data extension repository.
        ///     Note that the <see cref="IProcessingSource"/> that owns this will be required to create this table
        ///     when this is set.
        /// </summary>
        public bool InternalTable { get; set; }

        /// <summary>
        ///     The name of the static class property which returns the <see cref="TableDescriptor"/>.
        /// </summary>
        protected internal string TableDescriptorPropertyName { get; }

        /// <summary>
        ///     The name of the static class method which builds a table.
        ///     BuildTable(Action&lt;ITableBuilder, IDataExtensionRetrieval&gt;)
        /// </summary>
        protected internal string BuildTableActionMethodName { get; }

        /// <summary>
        ///     The name of the static class method which checks if the table has data.
        ///     IsDataAvailable(IDataExtensionRetrieval)
        /// </summary>
        protected internal string IsDataAvailableMethodName { get; }
    }
}
