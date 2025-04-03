// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.

using Microsoft.Performance.SDK;
using Microsoft.Performance.Toolkit.Plugins.Core;

namespace Microsoft.Performance.Toolkit.Plugins.Cli.Manifest
{
    /// <summary>
    ///     Contains information about the plugin owner in the manifest.
    /// </summary>
    public sealed class PluginOwnerInfoManifest
        : IEquatable<PluginOwnerInfoManifest>
    {
        /// <summary>
        ///     Initializes a new instance of the <see cref="PluginOwnerInfoManifest"/>
        /// </summary>
        /// <param name="name">
        ///     The name of the plugin owner.
        /// </param>
        /// <param name="address">
        ///     The address of the owner, if any.
        /// </param>
        /// <param name="emailAddresses">
        ///     The email addresses of the owner, if any.
        /// </param>
        /// <param name="phoneNumbers">
        ///     The phone numbers of the owner, if any.
        /// </param>
        public PluginOwnerInfoManifest(
            string name,
            string address,
            IEnumerable<string> emailAddresses,
            IEnumerable<string> phoneNumbers)
        {
            this.Name = name;
            this.Address = address;
            this.EmailAddresses = emailAddresses;
            this.PhoneNumbers = phoneNumbers;
        }

        /// <summary>
        ///     Gets the name of the plugin owner.
        /// </summary>
        public string Name { get; }

        /// <summary>
        ///     Gets the address of the owner, if any.
        /// </summary>
        public string Address { get; }

        /// <summary>
        ///     Gets the email addresses of the owner, if any.
        /// </summary>
        public IEnumerable<string> EmailAddresses { get; }

        /// <summary>
        ///     Gets the phone numbers of the owner, if any.
        /// </summary>
        public IEnumerable<string> PhoneNumbers { get; }

        /// <inheritdoc />
        public override bool Equals(object? obj)
        {
            return Equals(obj as PluginOwnerInfoManifest);
        }

        /// <inheritdoc/>
        public bool Equals(PluginOwnerInfoManifest? other)
        {
            if (other is null)
            {
                return false;
            }

            if (ReferenceEquals(this, other))
            {
                return true;
            }

            return string.Equals(this.Name, other.Name, StringComparison.Ordinal)
                && string.Equals(this.Address, other.Address, StringComparison.Ordinal)
                && this.EmailAddresses.EnumerableEqual(other.EmailAddresses, StringComparer.Ordinal)
                && this.PhoneNumbers.EnumerableEqual(other.PhoneNumbers, StringComparer.Ordinal);
        }

        /// <inheritdoc/>
        public override int GetHashCode()
        {
            int result = HashCodeUtils.CombineHashCodeValues(
                this.Name?.GetHashCode() ?? 0,
                this.Address?.GetHashCode() ?? 0);

            if (this.PhoneNumbers != null)
            {
                foreach (string phone in this.PhoneNumbers)
                {
                    HashCodeUtils.CombineHashCodeValues(result, phone?.GetHashCode() ?? 0);
                }
            }

            if (this.EmailAddresses != null)
            {
                foreach (string email in this.EmailAddresses)
                {
                    HashCodeUtils.CombineHashCodeValues(result, email?.GetHashCode() ?? 0);
                }
            }

            return result;
        }
    }
}
