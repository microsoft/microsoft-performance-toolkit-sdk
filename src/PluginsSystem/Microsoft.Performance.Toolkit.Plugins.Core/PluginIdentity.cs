// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.

using System;
using Microsoft.Performance.SDK;
using NuGet.Versioning;

namespace Microsoft.Performance.Toolkit.Plugins.Core
{
    /// <summary>
    ///     Represents the core identity of a plugin.
    /// </summary>
    public sealed class PluginIdentity
        : IEquatable<PluginIdentity>
    {
        /// <summary>
        ///     The maximum length <see cref="Id"/> may be for it to be considered valid.
        /// </summary>
        public const int MaxIdLength = 256;

        /// <summary>
        ///     Creates an instance of <see cref="PluginIdentity"/>.
        /// </summary>
        /// <param name="id">
        ///     The identifier of this plugin.
        /// </param>
        /// <param name="version">
        ///     The version of this plugin.
        /// </param>
        public PluginIdentity(string id, SemanticVersion version)
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
        public SemanticVersion Version { get; }

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

            return string.Equals(a.Id, b.Id, StringComparison.Ordinal) &&
                SemanticVersion.Equals(a.Version, b.Version);
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
            return $"{this.Id}-{this.Version}";
        }

        /// <summary>
        ///     Determines whether the <see cref="Id"/> conforms to the following specifications:
        ///     <list type="bullet">
        ///         <item>
        ///             <see cref="Id"/> is not null or whitespace.
        ///         </item>
        ///         <item>
        ///             <see cref="Id"/> is not more than <see cref="MaxIdLength"/> characters.
        ///         </item>
        ///         <item>
        ///             <see cref="Id"/> contains only alphanumeric characters, underscores, dots, and dashes.
        ///         </item>
        ///     </list>
        ///     This specification ensures that IDs can safely be used in file and directory names.
        /// </summary>
        /// <param name="errorMessage">
        ///     An error message that describes the reason why <see cref="Id"/> is invalid, if applicable. This
        ///     value is <c>null</c> if the ID is valid.
        /// </param>
        /// <returns>
        ///     Whether <see cref="Id"/> is valid.
        /// </returns>
        public bool HasValidId(out string errorMessage)
        {
            errorMessage = null;

            if (string.IsNullOrWhiteSpace(this.Id))
            {
                errorMessage = "Plugin Id is null or whitespace.";
                return false;
            }

            if (this.Id.Length > MaxIdLength)
            {
                errorMessage = $"Plugin Id is more than {MaxIdLength} characters.";
                return false;
            }

            // Only allow alphanumeric characters, underscores, dots, and dashes.
            foreach (var c in this.Id)
            {
                if (!char.IsLetterOrDigit(c) && c != '_' && c != '-' && c != '.')
                {
                    errorMessage = "Plugin Id contains invalid characters. Only alphanumeric characters, underscores, dots, and dashes are allowed.";
                    return false;
                }
            }

            return true;
        }
    }
}
