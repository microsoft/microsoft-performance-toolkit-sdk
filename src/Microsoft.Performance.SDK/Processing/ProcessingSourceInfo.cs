// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.

using System.Text.Json.Serialization;

namespace Microsoft.Performance.SDK.Processing
{
    /// <summary>
    ///     Represents information about an <see cref="IProcessingSource"/>.
    /// </summary>
    public sealed class ProcessingSourceInfo
    {
        /// <summary>
        ///    Initializes a new instance of the <see cref="ProcessingSourceInfo"/> class with the specified parameters.
        /// </summary>
        [JsonConstructor]
        public ProcessingSourceInfo(
            ContactInfo[] owners,
            ProjectInfo projectInfo,
            LicenseInfo licenseInfo,
            string copyrightNotice,
            string[] additionalInformation)
        {
            this.Owners = owners;
            this.ProjectInfo = projectInfo;
            this.LicenseInfo = licenseInfo;
            this.CopyrightNotice = copyrightNotice;
            this.AdditionalInformation = additionalInformation;
        }

        /// <summary>
        ///    Initializes a new instance of the <see cref="ProcessingSourceInfo"/> class.
        /// </summary>
        public ProcessingSourceInfo()
        {
        }

        /// <summary>
        ///     Gets or sets the contact information of the owners
        ///     of the <see cref="IProcessingSource"/>.
        /// </summary>
        public ContactInfo[] Owners { get; set; }

        /// <summary>
        ///     Gets or sets the project information of the <see cref="IProcessingSource"/>, if any.
        /// </summary>
        public ProjectInfo ProjectInfo { get; set; }

        /// <summary>
        ///     Gets or sets the license information of the <see cref="IProcessingSource"/>, if any.
        /// </summary>
        public LicenseInfo LicenseInfo { get; set; }

        /// <summary>
        ///     Gets or sets the copyright notice of the <see cref="IProcessingSource"/>, if any.
        /// </summary>
        public string CopyrightNotice { get; set; }

        /// <summary>
        ///     Gets or sets any additional information about the <see cref="IProcessingSource"/> to convey to the user. 
        ///     Each entry in the array is logically a new paragraph.
        /// </summary>
        public string[] AdditionalInformation { get; set; }
    }

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
        ///     Initializes a new instance of the <see cref="ContactInfo"/> class.
        /// </summary>
        public ContactInfo()
        {
        }

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
    ///     Represents the license information for a <see cref="IProcessingSource"/>.
    /// </summary>
    public sealed class LicenseInfo
    {
        /// <summary>
        ///   Initializes a new instance of the <see cref="LicenseInfo"/> class with the specified parameters.
        /// </summary>
        [JsonConstructor]
        public LicenseInfo(
            string name,
            string uri,
            string text)
        {
            this.Name = name;
            this.Uri = uri;
            this.Text = text;
        }

        /// <summary>
        ///    Initializes a new instance of the <see cref="LicenseInfo"/> class.
        /// </summary>
        public LicenseInfo()
        {
        }

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
    ///     Represents project information about the <see cref="IProcessingSource"/>.
    /// </summary>
    public sealed class ProjectInfo
    {
        /// <summary>
        ///   Initializes a new instance of the <see cref="ProjectInfo"/> class with the specified parameters.
        /// </summary>
        /// <param name="uri"></param>
        [JsonConstructor]
        public ProjectInfo(string uri)
        {
            this.Uri = uri;
        }

        /// <summary>
        ///   Initializes a new instance of the <see cref="ProjectInfo"/> class.
        /// </summary>
        public ProjectInfo()
        {
        } 

        /// <summary>
        ///     Gets or sets the URI to the page for this project.
        /// </summary>
        public string Uri { get; set; }
    }
}
