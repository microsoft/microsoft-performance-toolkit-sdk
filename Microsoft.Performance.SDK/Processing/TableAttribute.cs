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
        ///     BuildTable(Action&lt;ITableBuilder, IDataExtensionRetrieval&gt;)
        /// </summary>
        public const string DefaultTableBuilderMethodName = "BuildTable";

        /// <summary>
        ///     The default name of the static class method which checks if a table has data.
        ///     bool IsDataAvailable(IDataExtensionRetrieval)
        /// </summary>
        public const string DefaultIsDataAvailableMethodName = "IsDataAvailable";

        /// <summary>
        ///     Constructor is defunct.
        /// </summary>
        /// <param name="guid">
        ///     The unique identifier for this table. This MAY NOT be
        ///     the default (empty) <see cref="Guid"/>.
        /// </param>
        /// <param name="name">
        ///     The name of this table.
        /// </param>
        /// <param name="description">
        ///     A user friendly description of this table.
        /// </param>
        /// <param name="category">
        ///     The category into which this table belongs. This parameter
        ///     may be null, at which point the table is assumed to be in
        ///     the <see cref="TableDescriptor.DefaultCategory"/> category.
        /// </param>
        /// <exception cref="System.ArgumentException">
        ///     <paramref name="guid"/> is whitespace.
        ///     - or -
        ///     <paramref name="guid"/> parsed to a value
        ///     equal to <c>default(Guid)</c>.
        ///     - or -
        ///     <paramref name="name"/> is whitespace.
        ///     - or -
        ///     <paramref name="description"/> is whitespace.
        /// </exception>
        /// <exception cref="System.ArgumentNullException">
        ///     <paramref name="guid"/> is <c>null</c>.
        ///     - or -
        ///     <paramref name="name"/> is <c>null</c>.
        ///     - or -
        ///     <paramref name="description"/> is <c>null</c>.
        /// </exception>
        [Obsolete("This TableAttribute constructor has been deprecated. Please replace with new constructor.", true)]
        public TableAttribute(
            string guid,
            string name,
            string description,
            string category = "Other")
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
        /// <param name="internalTable">
        ///     When this is set, the table will not be exposed through the data extension repository.
        ///     Note that the custom data source that owns this will be required to create this table
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
        ///     Note that the custom data source that owns this will be required to create this table
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
