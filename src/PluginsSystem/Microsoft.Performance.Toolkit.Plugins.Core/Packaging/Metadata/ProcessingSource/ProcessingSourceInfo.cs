// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.

using System;
using System.Linq;
using System.Text.Json.Serialization;
using Microsoft.Performance.SDK;

namespace Microsoft.Performance.Toolkit.Plugins.Core.Packaging.Metadata
{
    /// <summary>
    ///     Represents information about a processing source.
    /// </summary>
    public sealed class ProcessingSourceInfo
        : IEquatable<ProcessingSourceInfo>
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

        /// <inheritdoc />
        public override bool Equals(object obj)
        {
            return Equals(obj as ProcessingSourceInfo);
        }

        /// <inheritdoc />
        public bool Equals(ProcessingSourceInfo other)
        {
            if (other == null)
            {
                return false;
            }

            if (ReferenceEquals(this, other))
            {
                return true;
            }

            return this.Owners.EnumerableEqual(other.Owners)
                && this.ProjectInfo?.Equals(other.ProjectInfo) == true
                && this.LicenseInfo?.Equals(other.LicenseInfo) == true
                && string.Equals(this.CopyrightNotice, other.CopyrightNotice)
                && this.AdditionalInformation.SequenceEqual(other.AdditionalInformation);
        }

        /// <inheritdoc />
        public override int GetHashCode()
        {
            int result = HashCodeUtils.CombineHashCodeValues(
                this.ProjectInfo?.GetHashCode() ?? 0,
                this.LicenseInfo?.GetHashCode() ?? 0,
                this.CopyrightNotice?.GetHashCode() ?? 0);


            if (this.Owners != null)
            {
                foreach (ContactInfo owner in this.Owners)
                {
                    result = HashCodeUtils.CombineHashCodeValues(result, owner?.GetHashCode() ?? 0);
                }
            }

            if (this.AdditionalInformation != null)
            {
                foreach (string additionalInfo in this.AdditionalInformation)
                {
                    result = HashCodeUtils.CombineHashCodeValues(result, additionalInfo?.GetHashCode() ?? 0);
                }
            }

            return result;
        }
    }
}
