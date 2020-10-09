// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.

namespace Microsoft.Performance.SDK.Processing
{
    // _CDS_
    // todo: are these specific to an implementation of an ICustomFormatter?

    /// <summary>
    ///     Provides constants for formatting column values as
    ///     <see cref="System.String"/>s. These are the format strings
    ///     as used in the .NET formatting infrastructure. See
    ///     <see cref="UIHints.CellFormat"/> and
    ///     <see cref="ColumnMetadata.FormatProvider"/>.
    /// </summary>
    public static class ColumnFormats
    {
        /// <summary>
        ///     Render the value as a percent to two decimal places.
        /// </summary>
        public const string PercentFormat = "N2";

        /// <summary>
        ///     Render the value as a hexadecimal value.
        /// </summary>
        public const string HexFormat = "x";

        /// <summary>
        ///     Render the value as a whole number with no decimal places.
        ///     todo: __CDS__
        ///     Document the rounding (if any)
        /// </summary>
        public const string NumberFormat = "N0";

        /// <summary>
        ///     Render the value as a whole number with no decimal places,
        ///     as appropriate for copying to the clipboard.
        ///     todo: __CDS__
        ///     Document the rounding (if any)
        /// </summary>
        public const string NumberClipboardFormat = "F0";
    }
}
