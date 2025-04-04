// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.

using Microsoft.Performance.SDK;
using Microsoft.Performance.Toolkit.Plugins.Core;

namespace Microsoft.Performance.Toolkit.Plugins.Cli.Manifest
{
    /// <summary>
    ///     Represents the identity of a plugin in a manifest.
    /// </summary>
    public sealed class PluginIdentityManifest
        : IEquatable<PluginIdentityManifest>
    {
        /// <summary>
        ///     Initializes a new instance of the <see cref="PluginIdentityManifest"/>
        /// </summary>
        /// <param name="id">
        ///     The identifier of this plugin.
        /// </param>
        /// <param name="version">
        ///     The version of this plugin.
        /// </param>
        public PluginIdentityManifest(
            string id,
            PluginVersion version)
        {
            Guard.NotNull(id, nameof(id));
            Guard.NotNull(version, nameof(version));

            this.Id = id;
            this.Version = version;
        }

        /// <summary>
        ///     Gets or sets the identifier of this plugin.
        /// </summary>
        public string Id { get; }

        /// <summary>
        ///     Gets or sets the version of this plugin.
        /// </summary>
        public PluginVersion Version { get; }

        /// <inheritdoc />
        public override bool Equals(object? obj)
        {
            return Equals(obj as PluginIdentityManifest);
        }

        /// <inheritdoc />
        public bool Equals(PluginIdentityManifest? other)
        {
            return Equals(this, other);
        }

        /// <summary>
        ///     Determines whether the specified <see cref="PluginIdentityManifest"/> instances are considered equal.
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
        public static bool Equals(PluginIdentityManifest? a, PluginIdentityManifest? b)
        {
            if (ReferenceEquals(a, b))
            {
                return true;
            }

            if (a is null || b is null)
            {
                return false;
            }

            return string.Equals(a.Id, b.Id, StringComparison.Ordinal) &&
                PluginVersion.Equals(a.Version, b.Version);
        }

        /// <inheritdoc />
        public override int GetHashCode()
        {
            return HashCodeUtils.CombineHashCodeValues(
               this.Id.GetHashCode(),
               this.Version.GetHashCode());
        }
    }
}
