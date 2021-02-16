// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.

using System;
using System.IO;

namespace Microsoft.Performance.SDK.Processing
{
    /// <summary>
    ///     Attribute to mark a custom data source implementation as processing
    ///     files that do not have extensions.
    /// </summary>
    [AttributeUsage(AttributeTargets.Class, AllowMultiple = false)]
    public sealed class ExtensionlessFileDataSourceAttribute
        : DataSourceAttribute,
          IEquatable<ExtensionlessFileDataSourceAttribute>
    {
        /// <summary>
        ///     Initializes a new instance of the <see cref="ExtensionlessFileDataSourceAttribute"/>
        ///     class.
        /// </summary>
        /// <param name="description">
        ///     A description of the file type exposed by this attribute.
        /// </param>
        /// <exception cref="System.ArgumentException">
        ///     <paramref name="description"/> is whitespace.
        /// </exception>
        /// <exception cref="System.ArgumentNullException">
        ///     <paramref name="description"/> is null.
        /// </exception>
        public ExtensionlessFileDataSourceAttribute(string description)
            : base(typeof(FileDataSource), description)
        {
        }

        /// <inheritdoc />
        public override bool Accepts(IDataSource dataSource)
        {
            Guard.NotNull(dataSource, nameof(dataSource));

            if (!(dataSource is FileDataSource fds))
            {
                return false;
            }

            return Path.GetExtension(fds.FullPath) == string.Empty;
        }

        /// <inheritdoc />
        public bool Equals(ExtensionlessFileDataSourceAttribute other)
        {
            return base.Equals(other);
        }

        /// <inheritdoc />
        public override bool Equals(object obj)
        {
            return this.Equals(obj as ExtensionlessFileDataSourceAttribute);
        }

        /// <inheritdoc />
        public override int GetHashCode()
        {
            return base.GetHashCode();
        }

        /// <inheritdoc />
        public override string ToString()
        {
            return this.Description;
        }
    }
}
