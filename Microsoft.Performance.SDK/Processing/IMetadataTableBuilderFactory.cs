// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.

namespace Microsoft.Performance.SDK.Processing
{
    /// <summary>
    ///     Factory to create <see cref="ITableBuilder" />s that can
    ///     be used to create metadata tables.
    /// </summary>
    public interface IMetadataTableBuilderFactory
    {
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
        ITableBuilder Create(TableDescriptor tableDescriptor);
    }
}
