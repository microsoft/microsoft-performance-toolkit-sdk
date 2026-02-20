// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.

using System;
using System.IO;
using System.Text;

namespace Microsoft.Performance.SDK.Processing
{
    /// <summary>
    ///     Attribute to mark an <see cref="IProcessingSource"/> implementation as processing
    ///     files of a specific type. This is used to instruct callers to route files
    ///     with a given extension to the decorated <see cref="IProcessingSource"/> for processing.
    /// </summary>
    [AttributeUsage(AttributeTargets.Class, AllowMultiple = true)]
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
            : base(typeof(FileDataSource), description)
        {
            Guard.NotNullOrWhiteSpace(fileExtension, nameof(fileExtension));

            this.FileExtension = fileExtension.TrimStart('.');
        }

        /// <summary>
        ///     Gets the file extension supported by the decorated class,
        ///     not including the leading period (".")
        /// </summary>
        public string FileExtension { get; }

        /// <inheritdoc />
        public bool Equals(FileDataSourceAttribute other)
        {
            return base.Equals(other) &&
                this.FileExtension.Equals(other.FileExtension);
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
                this.FileExtension.GetHashCode());
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

        /// <inheritdoc />
        protected override bool AcceptsCore(IDataSource dataSource)
        {
            Guard.NotNull(dataSource, nameof(dataSource));

            if (!(dataSource is FileDataSource fds))
            {
                return false;
            }

            var ext = '.' + this.FileExtension;

            return StringComparer.OrdinalIgnoreCase.Equals(
                ext,
                Path.GetExtension(fds.FullPath));
        }
    }
}
