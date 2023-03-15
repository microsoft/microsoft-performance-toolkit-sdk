// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.

using System.Linq;
using System.Text.Json.Serialization;

namespace Microsoft.Performance.Toolkit.Plugins.Core.Packaging.Metadata
{
    /// <summary>
    ///     Represents contact information for an entity.
    /// </summary>
    public sealed class ContactInfo
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
    }
}
