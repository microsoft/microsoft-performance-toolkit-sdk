// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.

namespace Microsoft.Performance.SDK.Processing
{
    /// <summary>
    ///     Represents the button pressed by a user
    ///     on a message box.
    /// </summary>
    public enum ButtonResult
    {
        /// <summary>
        ///     No result is specified.
        /// </summary>
        None = 0,

        /// <summary>
        ///     User chose OK.
        /// </summary>
        OK = 1,

        /// <summary>
        ///     User chose Cancel.
        /// </summary>
        Cancel = 2,

        /// <summary>
        ///     User chose Yes.
        /// </summary>
        Yes = 6,

        /// <summary>
        ///     User chose No.
        /// </summary>
        No = 7,
    }
}
