// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.

using System;
using Microsoft.Performance.SDK.Processing;

namespace Microsoft.Performance.SDK.Runtime
{
    /// <summary>
    ///     A table builder that is used to build
    ///     metadata tables.
    ///     <para/>
    ///     Within the SDK, the runtime will pass a processor an
    ///     <see cref="IMetadataTableBuilderFactory"/> via the <see cref="ICustomDataProcessor.BuildMetadataTables(IMetadataTableBuilderFactory)"/>
    ///     method, so there is usually no need to interact with this class directly. Instead, just use the
    ///     factory to get builders as necessary.
    /// </summary>
    public sealed class MetadataTableBuilder
        : TableBuilder
    {
        /// <summary>
        ///     Initializes a new instance of the <see cref="MetadataTableBuilder"/>
        ///     class for the given table.
        /// </summary>
        /// <param name="tableDescriptor">
        ///     A <see cref="TableDescriptor"/> describing the metadata table being
        ///     built.
        /// </param>
        /// <exception cref="System.ArgumentNullException">
        ///     <paramref name="tableDescriptor"/> is <c>null</c>.
        /// </exception>
        public MetadataTableBuilder(
            TableDescriptor tableDescriptor)
        {
            Guard.NotNull(tableDescriptor, nameof(tableDescriptor));

            this.TableDescriptor = tableDescriptor;
        }

        /// <summary>
        ///     Gets the description of this metadata table.
        /// </summary>
        public string Description => this.TableDescriptor.Description;

        /// <summary>
        ///     Gets the <see cref="Guid"/> for this metadata table.
        /// </summary>
        public Guid Guid => this.TableDescriptor.Guid;

        /// <summary>
        ///     Gets the name of this metadata table.
        /// </summary>
        public string Name => this.TableDescriptor.Name;

        /// <summary>
        ///     Gets the <see cref="TableDescriptor"/> for this metadata table.
        /// </summary>
        public TableDescriptor TableDescriptor { get; }
    }
}
