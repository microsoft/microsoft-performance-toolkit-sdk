// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.

using System;

namespace Microsoft.Performance.SDK.Processing
{
    /// <summary>
    ///     Represents a column of data in a table. This class is usable
    ///     on its own, or new columns may be derived.
    /// </summary>
    /// <typeparam name="T">
    ///     The <see cref="Type"/> of data projected by this column.
    /// </typeparam>
    public class BaseDataColumn<T>
        : IDataColumn<T>
    {
        /// <summary>
        ///     Initializes a new instance of the <see cref="BaseDataColumn{T}" />
        ///     class.
        /// </summary>
        /// <param name="metadata">
        ///     The metadata about the column.
        /// </param>
        /// <param name="displayHints">
        ///     Hints to give callers on how to render this column. This parameter
        ///     may be <c>null</c>.
        /// </param>
        /// <param name="projection">
        ///     The projection that projects the data in the column.
        /// </param>
        /// <exception cref="System.ArgumentNullException">
        ///     <paramref name="metadata"/> is <c>null</c>.
        ///     - or -
        ///     <paramref name="projection"/> is <c>null</c>.
        /// </exception>
        public BaseDataColumn(
            ColumnMetadata metadata,
            UIHints displayHints,
            IProjection<int, T> projection)
            : this(new ColumnConfiguration(metadata, displayHints), projection)
        {
        }

        /// <summary>
        ///     Initializes a new instance of the <see cref="BaseDataColumn{T}" />
        ///     class.
        /// </summary>
        /// <param name="configuration">
        ///     The configuration of this column.
        /// </param>
        /// <param name="projection">
        ///     The projection that projects the data in the column.
        /// </param>
        /// <exception cref="System.ArgumentNullException">
        ///     <paramref name="configuration"/> is <c>null</c>.
        ///     - or -
        ///     <paramref name="projection"/> is <c>null</c>.
        /// </exception>
        public BaseDataColumn(
            ColumnConfiguration configuration,
            IProjection<int, T> projection)
        {
            Guard.NotNull(configuration, nameof(configuration));
            Guard.NotNull(projection, nameof(projection));

            this.Configuration = configuration;
            this.ProjectorInterface = projection.GetType();
            this.Projector = projection;
        }

        /// <inheritdoc />
        public ColumnConfiguration Configuration { get; }

        /// <inheritdoc />
        public Type DataType => typeof(T);

        /// <inheritdoc />
        public Type ProjectorInterface { get; }

        /// <inheritdoc />
        public IProjection<int, T> Projector { get; }

        /// <summary>
        ///     Projects the data in this column for the given row.
        /// </summary>
        /// <param name="index">
        ///     The row whose value is to be retrieved.
        /// </param>
        /// <returns>
        ///     The projected value at the given row.
        /// </returns>
        public T this[int index] => this.Projector[index];

        /// <inheritdoc />
        public object Project(int index)
        {
            return this.ProjectTyped(index);
        }

        /// <inheritdoc />
        public T ProjectTyped(int index)
        {
            return this[index];
        }

        /// <inheritdoc />
        public override string ToString()
        {
            return $"{Configuration.Metadata.Name} {{{Configuration.Metadata.Guid}}}";
        }
    }
}
