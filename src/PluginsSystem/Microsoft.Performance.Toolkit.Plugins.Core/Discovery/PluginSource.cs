// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.

using System;

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
            this.Uri = uri;
        }

        /// <summary>
        ///     Gets the URI to discover plugins from.
        /// </summary>
        public Uri Uri { get; }

        /// <inheritdoc />
        public bool Equals(PluginSource other)
        {
            return Equals(this, other);
        }

        /// <summary>
        ///     Determines whether the specified <see cref="PluginSource"/> instances are considered equal.
        /// </summary>
        /// <param name="a">
        ///     The first plugin source to compare.  
        /// </param>
        /// <param name="b">
        ///     The second plugin source to compare.
        /// </param>
        /// <returns>
        ///     <c>true</c> if a and b are considered equal; <c>false</c> otherwise.
        ///     If a or b is null, the method returns <c>false</c>.
        /// </returns>
        public static bool Equals(PluginSource a, PluginSource b)
        {
            if (ReferenceEquals(a, b))
            {
                return true;
            }

            if (a is null || b is null)
            {
                return false;
            }

            return a.Uri == b.Uri;
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
