// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.

using System.Diagnostics;
using System.IO;

namespace Microsoft.Performance.SDK.Runtime
{
    /// <summary>
    ///     Contains utility methods for interacting with
    ///     file extensions.
    /// </summary>
    public static class FileExtensionUtils
    {
        /// <summary>
        ///     Gets the extension from the given path, and places
        ///     said extension into canonical form. This method
        ///     effectively canonicalizes the output of <see cref="Path.GetExtension(string)" />.
        /// </summary>
        /// <param name="path">
        ///     The path string from which to get the extension.
        /// </param>
        /// <returns>
        ///     The canonical extension of the specified <paramref name="path"/> 
        ///     (including the period "."), or <c>null</c>, or <see cref="string.Empty" />.
        ///     If <paramref name="path"/> is <c>null</c>, this method returns <c>null</c>.
        ///     If <paramref name="path"/> does not have extension information,
        ///     this method returns <see cref="string.Empty" />.
        /// </returns>
        public static string GetCanonicalExtension(string path)
        {
            var extension = Path.GetExtension(path);
            return CanonicalizeExtension(extension);
        }

        /// <summary>
        ///     Gets the canonical form of the given extension. The canonical
        ///     form is ".EXT" where EXT is the extension in upper case invariant.
        ///     If the given extension does not start with a '.', then one will
        ///     be prepended. This method assumes that <paramref name="extension"/>
        ///     is already a file extension, and has no concept of paths or directories,
        ///     etc. If you are trying to get the canonical extension from a path,
        ///     use <see cref="GetCanonicalExtension(string)" />.
        /// </summary>
        /// <param name="extension">
        ///     The file extension to put into canonical form.
        /// </param>
        /// <returns>
        ///     The canonical form of the extension.
        /// </returns>
        public static string CanonicalizeExtension(string extension)
        {
            if (extension is null)
            {
                return null;
            }

            if (string.IsNullOrWhiteSpace(extension))
            {
                return string.Empty;
            }

            Debug.Assert(extension.Length > 0);
            if (extension[0] != '.')
            {
                extension = "." + extension;
            }

            return extension.ToUpperInvariant();
        }
    }
}
