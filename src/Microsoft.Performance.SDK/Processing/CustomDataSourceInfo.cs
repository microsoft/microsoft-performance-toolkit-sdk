// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.

namespace Microsoft.Performance.SDK.Processing
{
    /// <summary>
    ///     Represents information about a Custom Data Source. This
    ///     exposes authorship information, licensing information, and
    ///     any other information about your data source.
    /// </summary>
    public sealed class CustomDataSourceInfo
    {
        /// <summary>
        ///     Gets or sets the contact information of the owners
        ///     of this data source.
        /// </summary>
        public ContactInfo[] Owners { get; set; }

        /// <summary>
        ///     Gets or sets the project information, if any.
        /// </summary>
        public ProjectInfo ProjectInfo { get; set; }

        /// <summary>
        ///     Gets or sets the license information, if any.
        /// </summary>
        public LicenseInfo LicenseInfo { get; set; }

        /// <summary>
        ///     Gets or sets the copyright notice, if any.
        /// </summary>
        public string CopyrightNotice { get; set; }

        /// <summary>
        ///     Gets or sets any additional information you wish to
        ///     convey to the user. Each entry in the array is logically
        ///     a new paragraph.
        /// </summary>
        public string[] AdditionalInformation { get; set; }
    }

    /// <summary>
    ///     Represents contact information for an entity.
    /// </summary>
    public sealed class ContactInfo
    {
        /// <summary>
        ///     Gets or sets the name of the contact, if any.
        /// </summary>
        public string Name { get; set; }

        /// <summary>
        ///     Gets or sets the address of the contact, if any.
        /// </summary>
        public string Address { get; set; }

        /// <summary>
        ///     Gets or sets the email address of the contact,
        ///     if any.
        /// </summary>
        public string[] EmailAddresses { get; set; }

        /// <summary>
        ///     Gets or sets the phone numbers of the contact, if any.
        /// </summary>
        public string[] PhoneNumbers { get; set; }
    }

    /// <summary>
    ///     Represents the license information for a data source.
    /// </summary>
    public sealed class LicenseInfo
    {
        /// <summary>
        ///     Gets or sets the name of the license.
        /// </summary>
        public string Name { get; set; }

        /// <summary>
        ///     Gets or sets the URI where the license text may be found.
        /// </summary>
        public string Uri { get; set; }

        /// <summary>
        ///     Gets or sets the full text of the license, if desired.
        ///     This property may be <c>null</c>.
        /// </summary>
        public string Text { get; set; }
    }

    /// <summary>
    ///     Represents project information about a data source.
    /// </summary>
    public sealed class ProjectInfo
    {
        /// <summary>
        ///     Gets or sets the URI to the page for this project.
        /// </summary>
        public string Uri { get; set; }
    }
}
