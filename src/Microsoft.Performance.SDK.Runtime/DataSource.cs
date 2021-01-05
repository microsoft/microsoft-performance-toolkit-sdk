// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.

using System;
using Microsoft.Performance.SDK.Processing;

namespace Microsoft.Performance.SDK.Runtime
{
    /// <summary>
    ///     Base of all Data Source implementations.
    /// </summary>
    public abstract class DataSource
        : IDataSource
    {
        private readonly Uri uri;

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

            this.uri = uri;
        }

        /// <summary>
        ///     Returns the <see cref="Uri" /> of the data.
        /// </summary>
        /// <returns>
        ///     The URI of the data.
        /// </returns>
        public Uri GetUri()
        {
            return this.uri;
        }

        /// <inheritdoc />
        public override bool Equals(object obj)
        {
            if (ReferenceEquals(this, obj))
            {
                return true;
            }

            var other = obj as DataSource;
            return other != null &&
                this.uri.Equals(other.uri);
        }

        /// <inheritdoc />
        public override int GetHashCode()
        {
            return this.uri.GetHashCode();
        }

        /// <inheritdoc />
        public override string ToString()
        {
            return this.uri.ToString();
        }
    }
}
