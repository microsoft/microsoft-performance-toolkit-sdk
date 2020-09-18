// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.

namespace Microsoft.Performance.SDK
{
    /// <summary>
    ///     Provides a means of formatting an instance of an
    ///     <see cref="object"/> for the system clipboard.
    /// </summary>
    public interface IFormatForClipboard
    {
        /// <summary>
        ///     Converts this instance into a string suitable for
        ///     the system clipboard.
        /// </summary>
        /// <param name="format">
        ///     A composite format string.
        /// </param>
        /// <param name="includeUnits">
        ///     Whether to include units in the string.
        /// </param>
        /// <returns>
        ///     The string representation of this instance, suitable for
        ///     use in the system clipboard.
        /// </returns>
        string ToClipboardString(string format, bool includeUnits);
    }
}
