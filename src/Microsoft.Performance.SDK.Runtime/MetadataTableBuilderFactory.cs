// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.

using System.Collections.Generic;
using Microsoft.Performance.SDK.Processing;

namespace Microsoft.Performance.SDK.Runtime
{
    /// <summary>
    ///     Provides a means of creating metadata tables.
    /// </summary>
    public sealed class MetadataTableBuilderFactory
        : IMetadataTableBuilderFactory
    {
        private readonly List<MetadataTableBuilder> createdTables;
        private readonly IReadOnlyList<MetadataTableBuilder> createdTablesRO;

        /// <summary>
        ///     Initializes a new instance of the <see cref="MetadataTableBuilderFactory"/>
        ///     class.
        /// </summary>
        public MetadataTableBuilderFactory()
        {
            this.createdTables = new List<MetadataTableBuilder>();
            this.createdTablesRO = this.createdTables.AsReadOnly();
        }

        /// <summary>
        ///     Gets the collection of all metadata tables that have been
        ///     created by this instance of the factory.
        /// </summary>
        public IEnumerable<MetadataTableBuilder> CreatedTables => this.createdTablesRO;

        /// <summary>
        ///     Creates a new <see cref="ITableBuilder"/> to build
        ///     the metadata table represented by the given descriptor.
        /// </summary>
        /// <param name="tableDescriptor">
        ///     The <see cref="TableDescriptor"/> defining the metadata
        ///     table being built.
        /// </param>
        /// <returns>
        ///     A new <see cref="ITableBuilder"/> instance that can be
        ///     used to build a metadata table.
        /// </returns>
        /// <exception cref="System.ArgumentNullException">
        ///     <paramref name="tableDescriptor"/> is <c>null</c>.
        /// </exception>
        public ITableBuilder Create(TableDescriptor tableDescriptor)
        {
            var builder = new MetadataTableBuilder(tableDescriptor);
            this.createdTables.Add(builder);
            return builder;
        }
    }
}
