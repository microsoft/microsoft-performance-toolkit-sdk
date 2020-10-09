// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.

namespace Microsoft.Performance.SDK.Processing
{
    /// <summary>
    ///     Represents buttons on a message box.
    /// </summary>
    public enum Buttons
    {
        /// <summary>
        ///     The box will have the OK button.
        /// </summary>
        OK = 0,

        /// <summary>
        ///     The box will have OK and Cancel buttons.
        /// </summary>
        OKCancel = 1,

        /// <summary>
        ///     The box will have Yes, No, and Cancel buttons.
        /// </summary>
        YesNoCancel = 3,

        /// <summary>
        ///     The box will have Yes and No buttons.
        /// </summary>
        YesNo = 4,
    }
}
