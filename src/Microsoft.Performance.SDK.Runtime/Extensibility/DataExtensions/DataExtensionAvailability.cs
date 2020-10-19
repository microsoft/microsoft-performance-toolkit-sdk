// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.

namespace Microsoft.Performance.SDK.Runtime.Extensibility.DataExtensions
{
    /// <summary>
    ///     Defines the availability of a given data extension.
    /// </summary>
    public enum DataExtensionAvailability
    {
        /// <summary>
        ///     Availability of the data extension has not been determined.
        /// </summary>
        Undetermined,

        /// <summary>
        ///     The data extension is available for use
        /// </summary>
        Available,

        /// <summary>
        ///     The data extension is not available for use because it is missing a direct requirement.
        /// </summary>
        MissingRequirement,

        /// <summary>
        ///     The data extension is not available for use because a data extension in its dependency chain is missing.
        /// </summary>
        MissingIndirectRequirement,

        /// <summary>
        ///     The data extension is unavailable because there was an error loading it or an extension in its dependency chain.
        /// </summary>
        Error,

        /// <summary>
        ///     An error exists in a dependency of this extension.
        /// </summary>
        IndirectError,
    }
}
