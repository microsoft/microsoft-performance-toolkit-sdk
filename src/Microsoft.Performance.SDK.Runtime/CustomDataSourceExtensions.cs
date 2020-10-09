// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.

using System;
using System.IO;
using System.Reflection;
using Microsoft.Performance.SDK.Processing;

namespace Microsoft.Performance.SDK.Runtime
{
    /// <summary>
    ///     Extension methods for <see cref="ICustomDataSource"/>.
    /// </summary>
    public static class CustomDataSourceExtensions
    {
        /// <summary>
        ///     Extension method to return the supported file extension for a <see cref="ICustomDataSource"/>.
        /// </summary>
        /// <returns>
        ///     Returns the file extension; <c>null</c> if not defined.
        /// </returns>
        public static string TryGetFileExtension(this ICustomDataSource self)
        {
            var dataSource = self.GetType().GetCustomAttribute<DataSourceAttribute>();
            var fileDataSource = dataSource as FileDataSourceAttribute;
            return fileDataSource?.FileExtension;
        }

        /// <summary>
        ///     Extension method to return the file extension description for a <see cref="ICustomDataSource"/>.
        /// </summary>
        /// <returns>
        ///     Returns the file extension description; <c>null</c> if not defined.
        /// </returns>
        public static string TryGetFileDescription(this ICustomDataSource self)
        {
            var dataSource = self.GetType().GetCustomAttribute<DataSourceAttribute>();
            var fileDataSource = dataSource as FileDataSourceAttribute;
            return fileDataSource?.Description;
        }

        /// <summary>
        ///     Extension method to return the name for a <see cref="ICustomDataSource"/>.
        /// </summary>
        /// <returns>
        ///     Returns the name of the <see cref="ICustomDataSource"/>; <c>null</c> if not defined.
        /// </returns>
        public static string TryGetName(this ICustomDataSource self)
        {
            return self.GetType().GetCustomAttribute<CustomDataSourceAttribute>()?.Name;
        }

        /// <summary>
        ///     Extension method to return the description for a <see cref="ICustomDataSource"/>.
        /// </summary>
        /// <returns>
        ///     Returns the description of the <see cref="ICustomDataSource"/>; <c>null</c> if not defined.
        /// </returns>
        public static string TryGetDescription(this ICustomDataSource self)
        {
            return self.GetType().GetCustomAttribute<CustomDataSourceAttribute>()?.Description;
        }

        /// <summary>
        ///     Extension method to check if the file is supported by the <see cref="ICustomDataSource"/>.
        /// </summary>
        /// <param name="filePath">
        ///     Full path to the file being checked.
        /// </param>
        /// <returns>
        ///     <inheritdoc cref="ICustomDataSource.IsFileSupported(string)"/>
        /// </returns>
        public static bool Supports(this ICustomDataSource self, string filePath)
        {
            var supportedExtension = FileExtensionUtils.CanonicalizeExtension(self.TryGetFileExtension());
            var fileExtension = FileExtensionUtils.CanonicalizeExtension(Path.GetExtension(filePath));

            // Should this be invariant culture?
            return StringComparer.CurrentCultureIgnoreCase.Equals(fileExtension, supportedExtension) &&
                self.IsFileSupported(filePath);
        }

        /// <summary>
        ///     Extension method to return the <see cref="Guid"/> for a <see cref="ICustomDataSource"/>.
        /// </summary>
        /// <returns>
        ///     Returns the <see cref="Guid"/> of the <see cref="ICustomDataSource"/>; <see cref="Guid.Empty"/> if not defined.
        /// </returns>
        public static Guid TryGetGuid(this ICustomDataSource self)
        {
            var dataSourceId = self.GetType().GetCustomAttribute<CustomDataSourceAttribute>()?.Guid;
            return dataSourceId.HasValue ? dataSourceId.Value : Guid.Empty;
        }
    }
}
