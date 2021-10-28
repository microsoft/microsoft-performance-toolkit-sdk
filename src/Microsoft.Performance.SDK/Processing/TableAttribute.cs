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
        public const string DefaultTableDescriptorPropertyName = "TableDescriptor";

        /// <summary>
        ///     The default name of the static class method which builds a table.
        /// </summary>
        public const string DefaultTableBuilderMethodName = "BuildTable";

        /// <summary>
        ///     The default name of the static class method which checks if a table has data.
        /// </summary>
        public const string DefaultIsDataAvailableMethodName = "IsDataAvailable";

        /// <summary>
        ///     Initializes a new instance of the <see cref="TableAttribute"/> class.
        /// </summary>
        /// <param name="tableDescriptorPropertyName">
        ///     Name of the static class property which returns the
        ///     <see cref="TableDescriptor"/>
        /// </param>
        /// <param name="buildTableActionMethodName">
        ///     The name of the static class method which builds a table.
        ///     This method must have the following signature:
        ///     <code>
        ///         void BuildTable(<see cref="ITableBuilder"/>, <see cref="SDK.Extensibility.IDataExtensionRetrieval"/>)
        ///     </code>
        /// </param>
        /// <param name="isDataAvailableMethodName">
        ///     The name of the static class method which checks if the table has data.
        ///     This method must have the following signature:
        ///     <code>
        ///         bool IsDataAvailable(<see cref="SDK.Extensibility.IDataExtensionRetrieval"/>)
        ///     </code>
        /// </param>
        /// <param name="internalTable">
        ///     When this is set, the table will not be exposed through the data extension repository.
        ///     Note that the <see cref="IProcessingSource"/> that owns this will be required to create this table
        ///     when this is set.
        /// </param>
        public TableAttribute(
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
        ///     The name of the static class property which returns the <see cref="TableDescriptor"/>.
        /// </summary>
        public string TableDescriptorPropertyName { get; }

        /// <summary>
        ///     The name of the static class method which builds a table.
        ///     BuildTable(Action&lt;ITableBuilder, IDataExtensionRetrieval&gt;)
        /// </summary>
        public string BuildTableActionMethodName { get; }

        /// <summary>
        ///     When this is set, the table will not be exposed through the data extension repository.
        ///     Note that the <see cref="IProcessingSource"/> that owns this will be required to create this table
        ///     when this is set.
        /// </summary>
        public bool InternalTable { get; set; }

        /// <summary>
        ///     The name of the static class method which checks if the table has data.
        ///     IsDataAvailable(IDataExtensionRetrieval)
        /// </summary>
        public string IsDataAvailableMethodName { get; }
    }
}
