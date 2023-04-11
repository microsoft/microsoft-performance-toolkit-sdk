// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.

using System;
using Microsoft.Performance.SDK;

namespace Microsoft.Performance.Toolkit.Plugins.Core
{
    /// <summary>
    ///     Represents the core identity of a plugin.
    /// </summary>
    public sealed class PluginIdentity
        : IEquatable<PluginIdentity>
    {
        /// <summary>
        ///     Creates an instance of <see cref="PluginIdentity"/>.
        /// </summary>
        /// <param name="id">
        ///     The identifier of this plugin.
        /// </param>
        /// <param name="version">
        ///     The version of this plugin.
        /// </param>
        public PluginIdentity(string id, Version version)
        {
            Guard.NotNull(id, nameof(id));
            Guard.NotNull(version, nameof(version));

            this.Id = id;
            this.Version = version;
        }

        /// <summary>
        ///     Gets the identifer of this plugin.
        /// </summary>
        public string Id { get; }

        /// <summary>
        ///     Gets the version of this plugin.
        /// </summary>
        public Version Version { get; }

        /// <inheritdoc />
        public bool Equals(PluginIdentity other)
        {
            return Equals(this, other);
        }

        /// <summary>
        ///     Determines whether the specified <see cref="PluginIdentity"/> instances are considered equal.
        /// </summary>
        /// <param name="a">
        ///     The first plugin identity to compare.  
        /// </param>
        /// <param name="b">
        ///     The second plugin identity to compare.
        /// </param>
        /// <returns>
        ///     <c>true</c> if a and b are considered equal; <c>false</c> otherwise.
        ///     If a or b is null, the method returns <c>false</c>.
        /// </returns>
        public static bool Equals(PluginIdentity a, PluginIdentity b)
        {
            if (ReferenceEquals(a, b))
            {
                return true;
            }

            if (a is null || b is null)
            {
                return false;
            }

            return string.Equals(a.Id, b.Id, StringComparison.OrdinalIgnoreCase) &&
                Version.Equals(a.Version, b.Version);
        }

        /// <inheritdoc />
        public override int GetHashCode()
        {
            return HashCodeUtils.CombineHashCodeValues(
               this.Id.GetHashCode(),
               this.Version.GetHashCode());
        }

        /// <inheritdoc />
        public override string ToString()
        {
            return $"{this.Id} - {this.Version}";
        }
    }
}
