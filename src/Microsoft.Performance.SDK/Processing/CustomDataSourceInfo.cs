// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.

using System;

namespace Microsoft.Performance.SDK.Processing
{
    /// <summary>
    ///     Represents information about a Custom Data Source. This
    ///     exposes authorship information, licensing information, and
    ///     any other information about your data source.
    /// </summary>
    [Obsolete("CustomDataSourceInfo will be renamed to ProcessingSourceInfo")]
    public class CustomDataSourceInfo
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
}
