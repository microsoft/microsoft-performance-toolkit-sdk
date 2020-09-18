// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.

using System;
using System.Text;

namespace Microsoft.Performance.SDK.Processing
{
    /// <summary>
    ///     Attribute to mark a custom data source implementation as processing
    ///     files of a specific type. This is used to instruct callers to route files
    ///     with a given extension to the decorated custom data source for processing.
    /// </summary>
    [AttributeUsage(AttributeTargets.Class, AllowMultiple = false, Inherited = false)]
    public sealed class FileDataSourceAttribute
        : DataSourceAttribute,
          IEquatable<FileDataSourceAttribute>
    {
        /// <summary>
        ///     Initializes a new instance of the <see cref="FileDataSourceAttribute"/>
        ///     class.
        /// </summary>
        /// <param name="fileExtension">
        ///     The extension of files that should be processed by this data source.
        /// </param>
        /// <exception cref="System.ArgumentException">
        ///     <paramref name="fileExtension"/> is whitespace.
        /// </exception>
        /// <exception cref="System.ArgumentNullException">
        ///     <paramref name="fileExtension"/> is <c>null</c>.
        /// </exception>
        public FileDataSourceAttribute(string fileExtension)
            : this(fileExtension, fileExtension)
        {
        }

        /// <summary>
        ///     Initializes a new instance of the <see cref="FileDataSourceAttribute"/>
        ///     class.
        /// </summary>
        /// <param name="fileExtension">
        ///     The extension of files that should be processed by this data source.
        /// </param>
        /// <param name="description">
        ///     A description of the file type exposed by this attribute.
        /// </param>
        /// <exception cref="System.ArgumentException">
        ///     <paramref name="fileExtension"/> is whitespace.
        ///     - or -
        ///     <paramref name="description"/> is whitespace.
        /// </exception>
        /// <exception cref="System.ArgumentNullException">
        ///     <paramref name="fileExtension"/> is <c>null</c>.
        ///     - or -
        ///     <paramref name="description"/> is whitespace.
        /// </exception>
        public FileDataSourceAttribute(string fileExtension, string description)
        {
            // todo: do we want to support files that do not
            // have file extensions? How would these be denoted?
            Guard.NotNullOrWhiteSpace(fileExtension, nameof(fileExtension));
            Guard.NotNullOrWhiteSpace(description, nameof(description));

            this.FileExtension = fileExtension.TrimStart('.');
            this.Description = description;
        }

        /// <summary>
        ///     Gets the file extension supported by the decorated class.
        /// </summary>
        public string FileExtension { get; }

        /// <summary>
        ///     Gets the description of the file type.
        /// </summary>
        public string Description { get; }

        /// <inheritdoc />
        public bool Equals(FileDataSourceAttribute other)
        {
            return base.Equals(other ) &&
                !ReferenceEquals(other, null) &&
                this.FileExtension.Equals(other.FileExtension) &&
                this.Description.Equals(other.Description);
        }

        /// <inheritdoc />
        public override bool Equals(object obj)
        {
            return this.Equals(obj as FileDataSourceAttribute);
        }

        /// <inheritdoc />
        public override int GetHashCode()
        {
            return HashCodeUtils.CombineHashCodeValues(
                base.GetHashCode(),
                this.FileExtension.GetHashCode(),
                this.Description.GetHashCode());
        }

        /// <inheritdoc />
        public override string ToString()
        {
            var toString = new StringBuilder();
            toString.Append("File: ").Append(this.FileExtension);
            if (this.Description != this.FileExtension)
            {
                toString.Append(" (").Append(this.Description).Append(")");
            }

            return toString.ToString();
        }
    }
}
