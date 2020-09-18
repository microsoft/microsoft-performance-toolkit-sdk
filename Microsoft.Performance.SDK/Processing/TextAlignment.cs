// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.

namespace Microsoft.Performance.SDK.Processing
{
    /// <summary>
    ///     Enumerates the different ways text can be aligned in a
    ///     data cell.
    /// </summary>
    public enum TextAlignment
    {
        /// <summary>
        ///     Default. Text is aligned to the left.
        /// </summary>
        Left,

        /// <summary>
        ///     Text is aligned to the right.
        /// </summary>
        Right,

        /// <summary>
        ///     Text is centered.
        /// </summary>
        Center,

        /// <summary>
        ///     Text is justified.
        /// </summary>
        Justify,
    }
}
