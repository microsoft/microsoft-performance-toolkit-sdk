// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.

using System;

namespace Microsoft.Performance.SDK.Runtime
{
    /// <summary>
    ///     Denotes an icon to display in a message box.
    /// </summary>
    /// <remarks>
    ///     This enum is to break the dependency on the PresentationFramework.
    /// </remarks>
    public enum MessageBoxIcon
    {
        /// <summary>
        ///     No icon is to be displayed.
        /// </summary>
        None = 0,

        /// <summary>
        ///     An icon that is a lower case 'i' in a circle.
        /// </summary>
        Asterisk,
        
        /// <summary>
        ///     An icon that is a white X in a circle with a red background.
        /// </summary>
        Error,

        /// <summary>
        ///     An icon that is a white exclamation mark in a triangle with a yellow background.
        /// </summary>
        Exclamation,

        /// <summary>
        ///     An icon that is a white X in a circle with a red background.
        /// </summary>
        Hand,

        /// <summary>
        ///     An icon that is a lower case 'i' in a circle.
        /// </summary>
        Information,

        /// <summary>
        ///     An icon that is a question mark in a circle.
        ///     Do not use this value. It is included for backwards
        ///     compatibility.
        /// </summary>
        [Obsolete("This value is included for backwards compatibility only.")]
        Question,

        /// <summary>
        ///     An icon that is a white X in a circle with a red background.
        /// </summary>
        Stop,

        /// <summary>
        ///     An icon that is a white exclamation mark in a triangle with a yellow background.
        /// </summary>
        Warning,
    }
}
