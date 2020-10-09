// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.

using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using Microsoft.Performance.SDK.Extensibility;
using Microsoft.Performance.SDK.Extensibility.DataProcessing;

namespace Microsoft.Performance.SDK.Processing
{
    /// <summary>
    ///     Describes a table.
    /// </summary>
    public sealed class TableDescriptor
        : IEquatable<TableDescriptor>,
          IDataCookerDependent,
          IDataProcessorDependent
    {
        /// <summary>
        ///     The default table category name.
        /// </summary>
        public const string DefaultCategory = "Other";

        /// <summary>
        ///     Gets the default table layout style.
        /// </summary>
        public const TableLayoutStyle DefaultLayoutStyle = TableLayoutStyle.GraphAndTable;

        private readonly HashSet<DataCookerPath> dataCookers;
        private readonly HashSet<DataProcessorId> dataProcessors;

        /// <summary>
        ///     Initializes a new instance of the <see cref="TableDescriptor"/>
        ///     class.
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
        ///     the <see cref="TableAttribute.DefaultCategory"/> category.
        /// </param>
        /// <param name="isMetadataTable">
        ///     Whether the table is a metadata table.
        /// </param>
        /// <param name="defaultLayout">
        ///     The default layout style for the table.
        /// </param>
        /// <param name="requiredDataCookers">
        ///     Identifiers for data cookers required to instantiate this table.
        /// </param>
        /// <param name="requiredDataProcessors">
        ///     Identifiers for data processors required to instantiate this table.
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
        ///     - or -
        ///     <paramref name="category"/> is whitespace.
        /// </exception>
        /// <exception cref="System.ArgumentNullException">
        ///     <paramref name="guid"/> is <c>null</c>.
        ///     - or -
        ///     <paramref name="name"/> is <c>null</c>.
        ///     - or -
        ///     <paramref name="description"/> is <c>null</c>.
        ///     - or -
        ///     <paramref name="category"/> is <c>null</c>.
        /// </exception>
        public TableDescriptor(
            Guid guid,
            string name,
            string description,
            string category = DefaultCategory,
            bool isMetadataTable = false,
            TableLayoutStyle defaultLayout = TableLayoutStyle.GraphAndTable,
            IEnumerable<DataCookerPath> requiredDataCookers = null,
            IEnumerable<DataProcessorId> requiredDataProcessors = null)
        {
            Guard.NotDefault(guid, nameof(guid));
            Guard.NotNullOrWhiteSpace(name, nameof(name));
            Guard.NotNullOrWhiteSpace(description, nameof(description));
            Guard.NotNullOrWhiteSpace(category, nameof(category));

            this.Guid = guid;
            this.Name = name;
            this.Description = description;
            this.Category = category;
            this.DefaultLayout = defaultLayout;
            this.IsMetadataTable = isMetadataTable;
            this.ExtendedData = new Dictionary<string, object>();

            this.dataCookers = requiredDataCookers != null ? new HashSet<DataCookerPath>(requiredDataCookers) : new HashSet<DataCookerPath>();
            this.dataProcessors = requiredDataProcessors != null ? new HashSet<DataProcessorId>(requiredDataProcessors) : new HashSet<DataProcessorId>();
        }

        /// <summary>
        ///     Gets the unique identifier for the table.
        /// </summary>
        public Guid Guid { get; }

        /// <summary>
        ///     Gets the name of the table.
        /// </summary>
        public string Name { get; }

        /// <summary>
        ///     Gets the description of the table.
        /// </summary>
        public string Description { get; }

        /// <summary>
        ///     Gets the category of the table.
        /// </summary>
        public string Category { get; }

        /// <summary>
        ///     Gets the default layout style of the table.
        /// </summary>
        public TableLayoutStyle DefaultLayout { get; }

        /// <summary>
        ///     Gets a value indicating whether this instance describes a
        ///     metadata table.
        /// </summary>
        public bool IsMetadataTable { get; }

        /// <summary>
        ///     Provides a means of storing Data Source specific information
        ///     for a table.
        /// </summary>
        public IDictionary<string, object> ExtendedData { get; }

        /// <summary>
        ///     Gets or sets the <see cref="Type" /> associated with this <see cref="TableDescriptor" />,
        ///     if any. This property may be <c>null</c>.
        /// </summary>
        public Type Type { get; set; }

        /// <summary>
        ///     Gets or sets the pre-built configurations of this table.
        /// </summary>
        public TableConfigurations PrebuiltTableConfigurations { get; set; }

        /// <summary>
        ///     Gets or sets the <see cref="Type" /> representing the <see cref="ITableService"/>
        ///     used by this table, if any. This property may be <c>null</c>. See
        ///     <see cref="ITableBuilder.SetService(ITableService)"/>.
        /// </summary>
        public Type ServiceInterface { get; set; }

        /// <summary>
        ///     Identifiers for data cookers necessary to create this table.
        /// </summary>
        public IReadOnlyCollection<DataCookerPath> RequiredDataCookers => 
            new ReadOnlyCollection<DataCookerPath>(this.dataCookers.ToList());

        /// <summary>
        ///     Identifiers for data processors necessary to create this table.
        /// </summary>
        public IReadOnlyCollection<DataProcessorId> RequiredDataProcessors => 
            new ReadOnlyCollection<DataProcessorId>(this.dataProcessors.ToList());

        /// <inheritdoc />
        public bool Equals(TableDescriptor other)
        {
            return TableDescriptorEqualityComparer.Default.Equals(this, other);
        }

        /// <inheritdoc />
        public override bool Equals(object obj)
        {
            return obj != null &&
                this.Equals(obj as TableDescriptor);
        }

        /// <inheritdoc />
        public override int GetHashCode()
        {
            return TableDescriptorEqualityComparer.Default.GetHashCode(this);
        }

        /// <inheritdoc />
        public override string ToString()
        {
            return $"{this.Guid} - {this.Name}";
        }

        /// <summary>
        ///     Add a data cooker path to the local <see cref="dataCookers"/>
        /// </summary>
        /// <param name="cookerPath"></param>
        public void AddRequiredDataCooker(DataCookerPath cookerPath)
        {
            this.dataCookers.Add(cookerPath);
        }
    }
}
