// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.


namespace Microsoft.Performance.SDK
{
    /// <summary>
    ///     Represents units measuring quantities of bytes.
    /// </summary>
    public enum BytesUnits
    {
        /// <summary>
        ///     Represents bytes.
        /// </summary>
        Bytes,

        /// <summary>
        ///     Represents thousands of bytes.
        ///     Specifically, a multiple of 1024.
        /// </summary>
        Kilobytes,

        /// <summary>
        ///     Represents millions of bytes.
        ///     Specifically, a multiple of (1024 * 1024).
        /// </summary>
        Megabytes,

        /// <summary>
        ///     Represents billions of bytes.
        ///     Specifically, a multiple of (1024 * 1024 * 1024).
        /// </summary>
        Gigabytes,
    }
}
