// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.

using System;

namespace Microsoft.Performance.SDK.Processing
{
    /// <summary>
    ///     Base of all Data Source implementations.
    /// </summary>
    public abstract class DataSource
        : IDataSource
    {
        /// <summary>
        ///     Initializes a new instance of the <see cref="DataSource"/>
        ///     class with the given <see cref="Uri"/>.
        /// </summary>
        /// <param name="uri">
        ///     The URI of the data.
        /// </param>
        /// <exception cref="System.ArgumentNullException">
        ///     <paramref name="uri"/> is <c>null</c>.
        /// </exception>
        protected DataSource(Uri uri)
        {
            Guard.NotNull(uri, nameof(uri));

            this.Uri = uri;
        }

        /// <summary>
        ///     Gets the <see cref="Uri" /> of the data.
        /// </summary>
        /// <returns>
        ///     The URI of the data.
        /// </returns>
        public Uri Uri { get; }

        /// <inheritdoc />
        public override bool Equals(object obj)
        {
            if (ReferenceEquals(this, obj))
            {
                return true;
            }

            var other = obj as DataSource;
            return other != null &&
                this.Uri.Equals(other.Uri);
        }

        /// <inheritdoc />
        public override int GetHashCode()
        {
            return this.Uri.GetHashCode();
        }

        /// <inheritdoc />
        public override string ToString()
        {
            return this.Uri.ToString();
        }
    }
}
