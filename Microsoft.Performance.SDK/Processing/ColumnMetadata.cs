// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.

using System;

namespace Microsoft.Performance.SDK.Processing
{
    /// <summary>
    ///     Represents the metadata about a table column.
    /// </summary>
    public class ColumnMetadata
        : ICloneable<ColumnMetadata>
    {
        /// <summary>
        ///     Initializes a new instance of the <see cref="ColumnMetadata"/>
        ///     class.
        /// </summary>
        /// <param name="guid">
        ///     The unique identifier for this column. This MAY NOT be
        ///     the default (empty) <see cref="Guid"/>.
        /// </param>
        /// <param name="name">
        ///     The name of this column.
        /// </param>
        /// <exception cref="System.ArgumentException">
        ///     <paramref name="guid"/> is equal to <c>default(Guid)</c>.
        ///     - or -
        ///     <paramref name="name"/> is whitespace.
        /// </exception>
        /// <exception cref="System.ArgumentNullException">
        ///     <paramref name="name"/> is <c>null</c>.
        /// </exception>
        public ColumnMetadata(Guid guid, string name)
            : this(guid, name, name)
        {
        }

        /// <summary>
        ///     Initializes a new instance of the <see cref="ColumnMetadata"/>
        ///     class.
        /// </summary>
        /// <param name="guid">
        ///     The unique identifier for this column. This MAY NOT be
        ///     the default (empty) <see cref="Guid"/>.
        /// </param>
        /// <param name="name">
        ///     The name of this column.
        /// </param>
        /// <param name="description">
        ///     A user friendly description of this column.
        /// </param>
        /// <exception cref="System.ArgumentException">
        ///     <paramref name="guid"/> is equal to <c>default(Guid)</c>.
        ///     - or -
        ///     <paramref name="name"/> is whitespace.
        ///     - or -
        ///     <paramref name="description"/> is whitespace.
        /// </exception>
        /// <exception cref="System.ArgumentNullException">
        ///     <paramref name="name"/> is <c>null</c>.
        ///     - or -
        ///     <paramref name="description"/> is <c>null</c>.
        /// </exception>
        public ColumnMetadata(Guid guid, string name, string description)
            : this(guid, name, Projection.Constant(name), description)
        {
        }

        /// <summary>
        ///     Initializes a new instance of the <see cref="ColumnMetadata"/>
        ///     class.
        /// </summary>
        /// <param name="guid">
        ///     The unique identifier for this column. This MAY NOT be
        ///     the default (empty) <see cref="Guid"/>.
        /// </param>
        /// <param name="defaultName">
        ///     The name to use on the column when there are multiple candidates
        ///     in the projection.
        /// </param>
        /// <param name="nameProjection">
        ///     The projection that allows the name of the column to change
        ///     based on the data in a row.
        /// </param>
        /// <param name="description">
        ///     A user friendly description of this column.
        /// </param>
        /// <exception cref="System.ArgumentException">
        ///     <paramref name="guid"/> is equal to <c>default(Guid)</c>.
        ///     - or -
        ///     <paramref name="defaultName"/> is whitespace.
        ///     - or -
        ///     <paramref name="description"/> is whitespace.
        /// </exception>
        /// <exception cref="System.ArgumentNullException">
        ///     <paramref name="defaultName"/> is <c>null</c>.
        ///     - or -
        ///     <paramref name="nameProjection"/> is <c>null</c>.
        ///     - or -
        ///     <paramref name="description"/> is <c>null</c>.
        /// </exception>
        public ColumnMetadata(
            Guid guid,
            string defaultName,
            IProjection<int, string> nameProjection,
            string description)
        {
            Guard.NotDefault(guid, nameof(guid));
            Guard.NotNullOrWhiteSpace(defaultName, nameof(defaultName));
            Guard.NotNull(nameProjection, nameof(nameProjection));

            if (string.IsNullOrWhiteSpace(description))
            {
                description = defaultName;
            }

            this.Guid = guid;
            this.Description = description;
            this.IsNameConstant = Projection.IsConstant(nameProjection);

            this.NameProjection = nameProjection;
            this.IsNameConstant = Projection.IsConstant(this.NameProjection);
            this.Name = defaultName;
        }

        /// <summary>
        ///     Initializes a new instance of the <see cref="ColumnMetadata"/>
        ///     class from another instance.
        /// </summary>
        /// <param name="other">
        ///     The instance from which to make a copy.
        /// </param>
        /// <exception cref="System.ArgumentNullException">
        ///     <paramref name="other"/> is <c>null</c>.
        /// </exception>
        public ColumnMetadata(ColumnMetadata other)
        {
            Guard.NotNull(other, nameof(other));

            this.Name = other.Name;
            this.NameProjection = other.NameProjection;

            this.Guid = other.Guid;
            this.ShortDescription = other.ShortDescription;
            this.Description = other.Description;
            this.IsNameConstant = other.IsNameConstant;
            this.IsPercent = other.IsPercent;
            this.IsDynamic = other.IsDynamic;
            this.FormatProvider = other.FormatProvider;
        }

        /// <summary>
        ///     Gets the unique identifier for this column.
        /// </summary>
        public Guid Guid { get; }

        /// <summary>
        ///     Gets the name of this column. If this column has a dynamic
        ///     name, then this property always returns the name as based
        ///     on the data in the first row. Use <see cref="NameProjection"/>
        ///     for dynamic names.
        /// </summary>
        public string Name { get; }

        /// <summary>
        ///     Gets a value indicating whether the name of this column is
        ///     constant. If this property returns <c>false</c>, then the
        ///     <see cref="NameProjection"/> can be used to get the dynamic
        ///     name.
        /// </summary>
        public bool IsNameConstant { get; }

        /// <summary>
        ///     Gets the name projector. This projector is only
        ///     useful if <see cref="IsNameConstant"/> is <c>false</c>.
        /// </summary>
        public IProjection<int, string> NameProjection { get; }

        /// <summary>
        /// Gets the user friendly short description of this column.
        /// </summary>
        public string ShortDescription { get; set; }

        /// <summary>
        ///     Gets the user friendly description of this column.
        /// </summary>
        public string Description { get; }

        /// <summary>
        ///     Gets or sets a value indicating whether this column
        ///     represents a percent value.
        /// </summary>
        public bool IsPercent { get; set; }

        /// <summary>
        ///     Gets or sets a value indicating whether the column has
        ///     dynamic content. This will instruct callers to always use the
        ///     column if it is available.
        /// </summary>
        public bool IsDynamic { get; set; }

        /// <summary>
        ///     Gets or sets the format provider to use to format the
        ///     data in the cell. This property may be <c>null</c>.
        /// </summary>
        public IFormatProvider FormatProvider { get; set; }

        /// <inheritdoc />
        public object Clone()
        {
            return new ColumnMetadata(this);
        }

        /// <inheritdoc />
        public ColumnMetadata CloneT()
        {
            return new ColumnMetadata(this);
        }
    }
}
