// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.

using System;
using System.Text.Json.Serialization;
using Microsoft.Performance.SDK;

namespace Microsoft.Performance.Toolkit.Plugins.Core.Metadata
{
    /// <summary>
    ///     Represents the license information for a processing source.
    /// </summary>
    public sealed class LicenseInfo
        : IEquatable<LicenseInfo>
    {
        /// <summary>
        ///   Initializes a new instance of the <see cref="LicenseInfo"/> class with the specified parameters.
        /// </summary>
        [JsonConstructor]
        public LicenseInfo(
            string name,
            Uri uri,
            string text)
        {
            this.Name = name;
            this.Uri = uri;
            this.Text = text;
        }

        /// <summary>
        ///     Gets the name of the license.
        /// </summary>
        public string Name { get; }

        /// <summary>
        ///     Gets the URI where the license text may be found.
        /// </summary>
        public Uri Uri { get; }

        /// <summary>
        ///     Gets the full text of the license, if desired.
        ///     This property may be <c>null</c>.
        /// </summary>
        public string Text { get; }

        /// <inheritdoc/>
        public override bool Equals(object obj)
        {
            return Equals(obj as LicenseInfo);
        }

        /// <inheritdoc/>
        public bool Equals(LicenseInfo other)
        {
            if (other == null)
            {
                return false;
            }

            if (ReferenceEquals(this, other))
            {
                return true;
            }

            return string.Equals(this.Name, other.Name, StringComparison.Ordinal)
                && string.Equals(this.Uri.OriginalString, other.Uri.OriginalString, StringComparison.Ordinal)
                && string.Equals(this.Text, other.Text, StringComparison.Ordinal);
        }

        /// <inheritdoc/>
        public override int GetHashCode()
        {
            return HashCodeUtils.CombineHashCodeValues(
                this.Name?.GetHashCode() ?? 0,
                this.Uri?.GetHashCode() ?? 0,
                this.Text?.GetHashCode() ?? 0);
        }
    }
}
