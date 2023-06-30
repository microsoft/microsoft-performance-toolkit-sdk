// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.

using System;
using System.Linq;
using System.Text.Json.Serialization;
using Microsoft.Performance.SDK;

namespace Microsoft.Performance.Toolkit.Plugins.Core.Metadata
{
    /// <summary>
    ///     Represents contact information for an entity.
    /// </summary>
    public sealed class ContactInfo
        : IEquatable<ContactInfo>
    {
        /// <summary>
        ///    Initializes a new instance of the <see cref="ContactInfo"/> class with the specified parameters.
        /// </summary>
        [JsonConstructor]
        public ContactInfo(
            string name,
            string address,
            string[] emailAddresses,
            string[] phoneNumbers)
        {
            this.Name = name;
            this.Address = address;
            this.EmailAddresses = emailAddresses;
            this.PhoneNumbers = phoneNumbers;
        }

        /// <summary>
        ///     Initializes a new instance of the <see cref="ContactInfo"/> class from a <see cref="SDK.Processing.ContactInfo"/> instance.
        /// </summary>
        /// <param name="contactInfo">
        ///     The <see cref="SDK.Processing.ContactInfo"/> instance to copy.
        /// </param>
        public ContactInfo(SDK.Processing.ContactInfo contactInfo)
            : this(
                  contactInfo?.Name,
                  contactInfo?.Address,
                  contactInfo?.EmailAddresses?.ToArray(),
                  contactInfo?.PhoneNumbers?.ToArray())
        {
        }

        /// <summary>
        ///     Gets the name of the contact, if any.
        /// </summary>
        public string Name { get; }

        /// <summary>
        ///     Gets the address of the contact, if any.
        /// </summary>
        public string Address { get; }

        /// <summary>
        ///     Gets the email address of the contact,
        ///     if any.
        /// </summary>
        public string[] EmailAddresses { get; }

        /// <summary>
        ///     Gets the phone numbers of the contact, if any.
        /// </summary>
        public string[] PhoneNumbers { get; }

        /// <inheritdoc />
        public override bool Equals(object obj)
        {
            return Equals(obj as ContactInfo);
        }

        /// <inheritdoc />
        public bool Equals(ContactInfo other)
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
                && string.Equals(this.Address, other.Address, StringComparison.Ordinal)
                && this.EmailAddresses.EnumerableEqual(other.EmailAddresses, StringComparer.Ordinal)
                && this.PhoneNumbers.EnumerableEqual(other.PhoneNumbers, StringComparer.Ordinal);
        }

        /// <inheritdoc />
        public override int GetHashCode()
        {
            int result = HashCodeUtils.CombineHashCodeValues(
                this.Name?.GetHashCode() ?? 0,
                this.Address?.GetHashCode() ?? 0);

            if (this.EmailAddresses != null)
            {
                foreach (string email in this.EmailAddresses)
                {
                    result = HashCodeUtils.CombineHashCodeValues(result, email?.GetHashCode() ?? 0);
                }
            }

            if (this.PhoneNumbers != null)
            {
                foreach (string phone in this.PhoneNumbers)
                {
                    result = HashCodeUtils.CombineHashCodeValues(result, phone?.GetHashCode() ?? 0);
                }
            }

            return result;
        }
    }
}
