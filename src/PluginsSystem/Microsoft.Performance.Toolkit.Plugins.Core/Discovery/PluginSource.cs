// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.

using System;
using Microsoft.Performance.SDK;

namespace Microsoft.Performance.Toolkit.Plugins.Core.Discovery
{
    /// <summary>
    ///     A URI source endpoint for discovering plugins.
    /// </summary>
    public sealed class PluginSource
        : IEquatable<PluginSource>
    {
        /// <summary>
        ///     Initializes a <see cref="PluginSource"/.
        /// </summary>
        /// <param name="uri">
        ///     A URI that can be used to discover plugins.
        /// </param>
        public PluginSource(Uri uri)
        {
            Guard.NotNull(uri, nameof(uri));
            
            this.Uri = uri;
        }

        /// <summary>
        ///     Gets the URI to discover plugins from.
        /// </summary>
        public Uri Uri { get; }

        /// <inheritdoc />
        public override bool Equals(object obj)
        {
            return Equals(this, obj as PluginSource);
        }   

        /// <inheritdoc />
        public bool Equals(PluginSource other)
        {
            if (other is null)
            {
                return false;
            }

            if (ReferenceEquals(this, other))
            {
                return true;
            }

            return this.Uri == other.Uri;
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
