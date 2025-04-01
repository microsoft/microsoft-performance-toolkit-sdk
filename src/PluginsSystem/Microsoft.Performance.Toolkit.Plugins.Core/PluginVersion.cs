// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.

using Microsoft.Performance.SDK;
using NuGet.Versioning;
using System;

namespace Microsoft.Performance.Toolkit.Plugins.Core
{
    /// <summary>
    /// Represents a version of a plugin.
    /// </summary>
    public sealed class PluginVersion
        : IEquatable<PluginVersion>
    {
        private readonly SemanticVersion version;

        /// <summary>
        /// Parses the specified version string and returns a new instance of <see cref="PluginVersion"/>.
        /// </summary>
        /// <param name="versionString">The version string to parse.</param>
        /// <returns>A new instance of <see cref="PluginVersion"/>.</returns>
        /// <exception cref="ArgumentNullException">Thrown when <paramref name="versionString"/> is null.</exception>
        /// <exception cref="ArgumentException">Thrown when <paramref name="versionString"/> is not a valid version.</exception>
        public static PluginVersion Parse(string versionString)
        {
            Guard.NotNull(versionString, nameof(versionString));

            return new PluginVersion(SemanticVersion.Parse(versionString));
        }

        private PluginVersion(SemanticVersion version)
        {
            this.version = version;
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="PluginVersion"/> class.
        /// </summary>
        /// <param name="major">The major version number.</param>
        /// <param name="minor">The minor version number.</param>
        /// <param name="patch">The patch version number.</param>
        public PluginVersion(int major, int minor, int patch)
            : this(new SemanticVersion(major, minor, patch))
        {
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="PluginVersion"/> class.
        /// </summary>
        /// <param name="major">The major version number.</param>
        /// <param name="minor">The minor version number.</param>
        /// <param name="patch">The patch version number.</param>
        /// <param name="preReleaseTag">The pre-release tag.</param>
        public PluginVersion(int major, int minor, int patch, string preReleaseTag)
            : this(new SemanticVersion(major, minor, patch, preReleaseTag))
        {
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="PluginVersion"/> class.
        /// </summary>
        /// <param name="major">The major version number.</param>
        /// <param name="minor">The minor version number.</param>
        /// <param name="patch">The patch version number.</param>
        /// <param name="preReleaseTag">The pre-release tag.</param>
        /// <param name="buildMetadata">The build metadata.</param>
        public PluginVersion(int major, int minor, int patch, string preReleaseTag, string buildMetadata)
            : this(new SemanticVersion(major, minor, patch, preReleaseTag, buildMetadata))
        {
        }

        /// <summary>
        /// Gets the major version number.
        /// </summary>
        public int Major => version.Major;

        /// <summary>
        /// Gets the minor version number.
        /// </summary>
        public int Minor => version.Minor;

        /// <summary>
        /// Gets the patch version number.
        /// </summary>
        public int Patch => version.Patch;

        /// <summary>
        /// Gets the pre-release tag.
        /// </summary>
        public string PreReleaseTag => version.Release;

        /// <summary>
        /// Returns the string representation of the version.
        /// </summary>
        /// <returns>The string representation of the version.</returns>
        public override string ToString()
        {
            return version.ToString();
        }

        public static bool operator ==(PluginVersion left, PluginVersion right)
        {
            if (ReferenceEquals(left, right))
            {
                return true;
            }

            if (left is null || right is null)
            {
                return false;
            }

            return left.Equals(right);
        }

        public static bool operator !=(PluginVersion left, PluginVersion right)
        {
            return !(left == right);
        }

        public static bool operator <(PluginVersion left, PluginVersion right)
        {
            return left.version < right.version;
        }

        public static bool operator <=(PluginVersion left, PluginVersion right)
        {
            return left.version <= right.version;
        }

        public static bool operator >(PluginVersion left, PluginVersion right)
        {
            return left.version > right.version;
        }

        public static bool operator >=(PluginVersion left, PluginVersion right)
        {
            return left.version >= right.version;
        }

        /// <summary>
        /// Determines whether the specified object is equal to the current object.
        /// </summary>
        /// <param name="obj">The object to compare with the current object.</param>
        /// <returns>true if the specified object is equal to the current object; otherwise, false.</returns>
        public override bool Equals(object obj)
        {
            return this.Equals(obj as PluginVersion);
        }

        /// <summary>
        /// Determines whether the specified <see cref="PluginVersion"/> is equal to the current <see cref="PluginVersion"/>.
        /// </summary>
        /// <param name="other">The <see cref="PluginVersion"/> to compare with the current <see cref="PluginVersion"/>.</param>
        /// <returns>true if the specified <see cref="PluginVersion"/> is equal to the current <see cref="PluginVersion"/>; otherwise, false.</returns>
        public bool Equals(PluginVersion other)
        {
            return SemanticVersion.Equals(this.version, other.version);
        }

        /// <summary>
        /// Serves as the default hash function.
        /// </summary>
        /// <returns>A hash code for the current object.</returns>
        public override int GetHashCode()
        {
            return this.version.GetHashCode();
        }
    }
}
