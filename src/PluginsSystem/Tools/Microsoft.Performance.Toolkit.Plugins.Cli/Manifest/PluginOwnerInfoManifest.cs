// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.

namespace Microsoft.Performance.Toolkit.Plugins.Cli.Manifest
{
    /// <summary>
    ///     Contains information about the plugin owner in the manifest.
    /// </summary>
    public sealed class PluginOwnerInfoManifest
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
    }
}
