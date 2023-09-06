// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.

namespace Microsoft.Performance.Toolkit.Plugins.Cli.Manifest
{
    /// <summary>
    ///     Validates a plugin manifest file.
    /// </summary>
    internal interface IManifestFileValidator
    {
        /// <summary>
        ///     Validates the given manifest file.
        /// </summary>
        /// <param name="manifestFilePath">
        ///     The path to the manifest file to validate.
        /// </param>
        /// <param name="errorMessages">
        ///     Any error messages encountered during validation.
        /// </param>
        /// <returns>
        ///     <c>true</c> if the manifest file is valid; <c>false</c> otherwise.
        /// </returns>
        bool IsValid(string manifestFilePath, out List<string> errorMessages);
    }
}
