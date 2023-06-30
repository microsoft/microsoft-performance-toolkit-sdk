// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.

using System;
using System.Collections.Generic;
using System.Text.Json.Serialization;
using Microsoft.Performance.SDK;

namespace Microsoft.Performance.Toolkit.Plugins.Core.Metadata
{
    /// <summary>
    ///     Gets or sets the information of a plugin owner.
    /// </summary>
    public sealed class PluginOwnerInfo
        : IEquatable<PluginOwnerInfo>
    {
        /// <summary>
        ///     Initializes a new instance of the <see cref="PluginOwnerInfo"/> class.
        /// </summary>
        [JsonConstructor]
        public PluginOwnerInfo(
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
        ///     Gets or sets the name of the plugin owner.
        /// </summary>
        public string Name { get; }

        /// <summary>
        ///     Gets or sets the address of the owner, if any.
        /// </summary>
        public string Address { get; }

        /// <summary>
        ///     Gets or sets the email addresses of the owner, if any.
        /// </summary>
        public IEnumerable<string> EmailAddresses { get; }

        /// <summary>
        ///     Gets or sets the phone numbers of the owner, if any.
        /// </summary>
        public IEnumerable<string> PhoneNumbers { get; }

        /// <inheritdoc />
        public override bool Equals(object obj)
        {
            return Equals(obj as PluginOwnerInfo);
        }

        /// <inheritdoc/>
        public bool Equals(PluginOwnerInfo other)
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
