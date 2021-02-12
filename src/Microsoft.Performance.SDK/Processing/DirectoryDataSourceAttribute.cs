// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.

using System;

namespace Microsoft.Performance.SDK.Processing
{
    /// <summary>
    ///     Attribute to mark a Custom Data Source implementation as processing
    ///     directories.
    /// </summary>
    public sealed class DirectoryDataSourceAttribute
        : DataSourceAttribute,
          IEquatable<DirectoryDataSourceAttribute>
    {
        /// <summary>
        ///     Initializes a new instance of the <see cref="DirectoryDataSourceAttribute"/>
        ///     class.
        /// </summary>
        public DirectoryDataSourceAttribute()
            : this("No description.")
        {
        }

        /// <summary>
        ///     Initializes a new instance of the <see cref="DirectoryDataSourceAttribute"/>
        ///     class, with a description of directories that would be acceptable.
        /// </summary>
        /// <param name="description">
        ///     A description of the directories accepted by the data source
        ///     decorated with this attribute.
        /// </param>
        /// <exception cref="System.ArgumentException">
        ///     <paramref name="description"/> is whitespace.
        /// </exception>
        /// <exception cref="System.ArgumentNullException">
        ///     <paramref name="description"/> is null.
        /// </exception>
        public DirectoryDataSourceAttribute(string description)
            : base(typeof(DirectoryDataSource), description)
        {
        }

        /// <inheritdoc />
        public bool Equals(DirectoryDataSourceAttribute other)
        {
            return base.Equals(other);
        }

        /// <inheritdoc />
        public override bool Equals(object obj)
        {
            return this.Equals(obj as DirectoryDataSourceAttribute);
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
