// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.

using Microsoft.Performance.SDK.Processing;

namespace Microsoft.Performance.SDK.Runtime
{
    /// <summary>
    ///     Extension methods for <see cref="CustomDataSourceReference"/>.
    /// </summary>
    public static class CustomDataSourceReferenceExtensions
    {
        /// <summary>
        ///     Extension method to get the canonical file extension for a <see cref="CustomDataSourceReference"/>.
        /// </summary>
        /// <returns>
        ///     <inheritdoc cref="FileExtensionUtils.CanonicalizeExtension(string)"/>
        /// </returns>
        public static string TryGetCanonicalFileExtension(this CustomDataSourceReference self)
        {
            var fileDataSource = self.DataSource as FileDataSourceAttribute;
            var fileDataSourceExtension = fileDataSource?.FileExtension;

            return FileExtensionUtils.CanonicalizeExtension(fileDataSourceExtension);
        }

        /// <summary>
        ///     Extension method to get the description for a <see cref="CustomDataSourceReference"/>.
        /// </summary>
        /// <returns>
        ///     <inheritdoc cref="CustomDataSourceExtensions.TryGetFileDescription(ICustomDataSource)"/>
        /// </returns>
        public static string TryGetFileDescription(this CustomDataSourceReference self)
        {
            var fileDataSource = self.DataSource as FileDataSourceAttribute;
            return fileDataSource?.Description;
        }

        /// <summary>
        ///     Extension method to check if the file is supported by the <see cref="CustomDataSourceReference"/>.
        /// </summary>
        /// <param name="filePath">
        ///     <inheritdoc cref="CustomDataSourceExtensions.Supports(ICustomDataSource, string)"/>
        /// </param>
        /// <returns>
        ///     <inheritdoc cref="CustomDataSourceExtensions.Supports(ICustomDataSource, string)"/>
        /// </returns>
        public static bool Supports(
            this CustomDataSourceReference self,
            string filePath)
        {
            return self.Instance.Supports(filePath);
        }
    }
}
