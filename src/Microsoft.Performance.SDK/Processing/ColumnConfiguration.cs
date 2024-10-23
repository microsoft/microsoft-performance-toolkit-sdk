// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.

using System;

namespace Microsoft.Performance.SDK.Processing
{
    /// <summary>
    ///     Defines the information for how to configure a column in a table.
    /// </summary>
    public sealed class ColumnConfiguration
    {
        /// <summary>
        ///     Initializes a new instance of the <see cref="ColumnConfiguration"/>
        ///     class.
        /// </summary>
        /// <param name="metadata">
        ///     The metadata about the column.
        /// </param>
        /// <param name="hints">
        ///     Optional hints about displaying this column in the UI. This parameter
        ///     may be <c>null</c>. If this parameter is <c>null</c>, then
        ///     <see cref="UIHints.Default"/> will be used for this instance.
        /// </param>
        /// <param name="variantGuid">
        ///     Optional unique identifier of the column variant to use.
        /// </param>
        /// <exception cref="System.ArgumentNullException">
        ///     <paramref name="metadata"/> is <c>null</c>.
        /// </exception>
        public ColumnConfiguration(
            ColumnMetadata metadata,
            UIHints hints = null,
            Guid? variantGuid = null)
        {
            Guard.NotNull(metadata, nameof(metadata));

            this.Metadata = metadata;
            this.VariantGuid = variantGuid;
            this.DisplayHints = hints ?? UIHints.Default();
        }

        /// <summary>
        ///     Initializes a new instance of the <see cref="ColumnConfiguration"/>
        ///     class from an existing instance.
        /// </summary>
        /// <param name="other">
        ///     The instance from which to make a copy.
        /// </param>
        /// <exception cref="System.ArgumentNullException">
        ///     <paramref name="other"/> is <c>null</c>.
        /// </exception>
        public ColumnConfiguration(ColumnConfiguration other)
            : this(
                other.Metadata.CloneT(),
                other.DisplayHints.CloneT(),
                other.VariantGuid)
        {
        }

        /// <summary>
        ///     Gets the metadata for this instance.
        /// </summary>
        public ColumnMetadata Metadata { get; }

        /// <summary>
        ///     Gets the unique identifier of the column variant to use.

        /// </summary>
        public Guid? VariantGuid { get; }

        /// <summary>
        ///     Gets any hints from the addin on how to render the column.
        /// </summary>
        /// <remarks>
        ///     todo: __CDS__ sensible defaults in the application layer.
        /// </remarks>
        public UIHints DisplayHints { get; }

        /// <summary>
        ///     Gets the <see cref="System.String"/> representation of this instance.
        /// </summary>
        /// <returns>
        ///     The <see cref="System.String"/> representation of this instance.
        /// </returns>
        public override string ToString()
        {
            if (this.VariantGuid == null)
            {
                return $"{this.Metadata.Guid} - {this.Metadata.Name}";
            }
            else
            {
                return $"{this.Metadata.Guid} - {this.Metadata.Name} (variant {this.VariantGuid})";
            }
        }
    }
}
