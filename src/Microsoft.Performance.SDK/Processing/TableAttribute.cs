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
        /// </summary>
        protected const string DefaultTableBuilderMethodName = "BuildTable";

        /// <summary>
        ///     The default name of the static class method which checks if a table has data.
        /// </summary>
        protected const string DefaultIsDataAvailableMethodName = "IsDataAvailable";

        /// <summary>
        ///     Initializes a new instance of the <see cref="TableAttribute"/> class.
        /// </summary>
        public TableAttribute()
            : this(DefaultTableDescriptorPropertyName, DefaultTableBuilderMethodName, DefaultIsDataAvailableMethodName)
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
        protected TableAttribute(
            string tableDescriptorPropertyName = DefaultTableDescriptorPropertyName,
            string buildTableActionMethodName = DefaultTableBuilderMethodName,
            string isDataAvailableMethodName = DefaultIsDataAvailableMethodName)
        {
            Guard.NotNullOrWhiteSpace(tableDescriptorPropertyName, nameof(tableDescriptorPropertyName));

            this.TableDescriptorPropertyName = tableDescriptorPropertyName;
            this.BuildTableActionMethodName = buildTableActionMethodName;
            this.IsDataAvailableMethodName = isDataAvailableMethodName;
        }

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
