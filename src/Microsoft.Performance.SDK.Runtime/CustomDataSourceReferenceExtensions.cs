// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.

using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using Microsoft.Performance.SDK.Processing;

namespace Microsoft.Performance.SDK.Runtime
{
    /// <summary>
    ///     Extension methods for <see cref="CustomDataSourceReference"/>.
    /// </summary>
    public static class CustomDataSourceReferenceExtensions
    {
        /// <summary>
        ///     Extension method to get the file extensions for a <see cref="CustomDataSourceReference"/>.
        /// </summary>
        /// <returns>
        ///     All file extensions supported by the referenced Custom Data Source.
        /// </returns>
        public static IEnumerable<string> TryGetFileExtensions(this CustomDataSourceReference self)
        {
            return self.DataSources
                .OfType<FileDataSourceAttribute>()
                .Select(x => x.FileExtension);
        }

        /// <summary>
        ///     Extension method to get the canonical file extensions for a <see cref="CustomDataSourceReference"/>.
        /// </summary>
        /// <returns>
        ///     The canonical forms of the file extensions supported by the referenced
        ///     Custom Data Source.
        /// </returns>
        public static IEnumerable<string> TryGetCanonicalFileExtensions(this CustomDataSourceReference self)
        {
            return self.TryGetFileExtensions()
                .Select(FileExtensionUtils.CanonicalizeExtension);
        }

        /// <summary>
        ///     Extension method to get the description for a <see cref="CustomDataSourceReference"/>.
        /// </summary>
        /// <param name="extension">
        ///     The extension of the file whose description is to be retreived. Specify
        ///     <c>null</c> or whitespace to retrieve the description of extensionless files.
        /// </param>
        /// <returns>
        ///     The description of the file specified by the given exteions; <c>null</c> if not found.
        /// </returns>
        public static string TryGetFileDescription(this CustomDataSourceReference self, string extension)
        {
            if (string.IsNullOrWhiteSpace(extension))
            {
                var attr = self.DataSources.SingleOrDefault(x => x is ExtensionlessFileDataSourceAttribute);
                return attr?.Description;
            }

            var canonical = FileExtensionUtils.CanonicalizeExtension(extension);
            foreach (var attr in self.DataSources.OfType<FileDataSourceAttribute>())
            {
                if (FileExtensionUtils.CanonicalizeExtension(attr.FileExtension) == canonical)
                {
                    return attr.Description;
                }
            }

            return null;
        }

        /// <summary>
        ///     Gets the description, if it exists, of directory support for the referenced
        ///     Custom Data Source 
        /// </summary>
        /// <returns>
        ///     The description of directories supported by the referenced Custom Data Source,
        ///     if any are supported; <c>null</c> otherwise.
        /// </returns>
        public static string TryGetDirectoryDescription(this CustomDataSourceReference self)
        {
            return self.DataSources
                .OfType<DirectoryDataSourceAttribute>()
                .SingleOrDefault()
                ?.Description;
        }

        /// <summary>
        ///     Gets the description, if it exists, of extensionless file support for the referenced
        ///     Custom Data Source 
        /// </summary>
        /// <returns>
        ///     The description of extensionless files supported by the referenced Custom Data Source,
        ///     if any are supported; <c>null</c> otherwise.
        /// </returns>
        public static string TryGetExtensionlessFileDescription(this CustomDataSourceReference self)
        {
            return self.DataSources
                .OfType<ExtensionlessFileDataSourceAttribute>()
                .SingleOrDefault()
                ?.Description;
        }

        /// <summary>
        ///     Gets a value indicating whether the referenced Custom Data Source
        ///     can process directories.
        /// </summary>
        /// <returns>
        ///     <c>true</c> if the referenced Custom Data Source can process directories;
        ///     <c>false</c> otherwise,
        /// </returns>
        public static bool AreDirectoriesSupported(this CustomDataSourceReference self)
        {
            return self.DataSources.Any(x => x is DirectoryDataSourceAttribute);
        }

        /// <summary>
        ///     Gets a value indicating whether the referenced Custom Data Source
        ///     can process files without extensions.
        /// </summary>
        /// <returns>
        ///     <c>true</c> if the referenced Custom Data Source can process files
        ///     without extensions; <c>false</c> otherwise,
        /// </returns>
        public static bool AreExtensionlessFilesSupported(this CustomDataSourceReference self)
        {
            return self.DataSources.Any(x => x is ExtensionlessFileDataSourceAttribute);
        }
    }
}
