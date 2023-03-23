// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.

using System.Linq;
using System.Text.Json.Serialization;

namespace Microsoft.Performance.Toolkit.Plugins.Core.Packaging.Metadata
{
    /// <summary>
    ///     Represents information about a processing source.
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
        ///     Initializes a new instance of the <see cref="ProcessingSourceInfo"/> class from a <see cref="SDK.Processing.ProcessingSourceInfo"/> instance.
        /// </summary>
        /// <param name="other"></param>
        public ProcessingSourceInfo(SDK.Processing.ProcessingSourceInfo other)
            : this(
                  other?.Owners?.Select(x => new ContactInfo(x)).ToArray(),
                  new ProjectInfo(other?.ProjectInfo),
                  new LicenseInfo(other?.LicenseInfo),
                  other?.CopyrightNotice,
                  other?.AdditionalInformation?.ToArray())
        {
        }

        /// <summary>
        ///     Gets the contact information of the owners
        ///     of the <see cref="IProcessingSource"/>.
        /// </summary>
        public ContactInfo[] Owners { get; }

        /// <summary>
        ///     Gets the project information of the <see cref="IProcessingSource"/>, if any.
        /// </summary>
        public ProjectInfo ProjectInfo { get; }

        /// <summary>
        ///     Gets the license information of the <see cref="IProcessingSource"/>, if any.
        /// </summary>
        public LicenseInfo LicenseInfo { get; }

        /// <summary>
        ///     Gets the copyright notice of the <see cref="IProcessingSource"/>, if any.
        /// </summary>
        public string CopyrightNotice { get; }

        /// <summary>
        ///     Gets any additional information about the <see cref="IProcessingSource"/> to convey to the user. 
        ///     Each entry in the array is logically a new paragraph.
        /// </summary>
        public string[] AdditionalInformation { get; }
    }
}
