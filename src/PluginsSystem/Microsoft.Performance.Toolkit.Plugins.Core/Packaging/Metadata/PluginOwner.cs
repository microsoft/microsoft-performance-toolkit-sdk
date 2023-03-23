// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.

using System.Collections.Generic;
using System.Text.Json.Serialization;

namespace Microsoft.Performance.Toolkit.Plugins.Core.Packaging.Metadata
{
    /// <summary>
    ///     Gets or sets the information of a plugin owner.
    /// </summary>
    public sealed class PluginOwner
    {
        /// <summary>
        ///     Initializes a new instance of the <see cref="PluginOwner"/> class.
        /// </summary>
        [JsonConstructor]
        public PluginOwner(
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
    }
}
