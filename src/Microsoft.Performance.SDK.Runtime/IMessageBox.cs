// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.

using System;
using Microsoft.Performance.SDK.Processing;

namespace Microsoft.Performance.SDK.Runtime
{
    /// <summary>
    ///     Used to break dependencies on platform specific code
    ///     in the runtime. It also allows for unit testing code 
    ///     that creates message boxes without locking up a UI.
    /// </summary>
    public interface IMessageBox
    {
        /// <summary>
        ///     Called when a message box needs to be shown the user.
        /// </summary>
        /// <param name="icon">
        ///     <see cref="MessageBoxIcon"/> to be used in the UI.
        /// </param>
        /// <param name="formatProvider">
        ///     <see cref="IFormatProvider"/> for the string message.
        /// </param>
        /// <param name="format">
        ///     Format of the string message, used with <paramref name="args"/>.
        /// </param>
        /// <param name="args">
        ///     Values to be displayed based on the string format: <paramref name="format"/>.
        /// </param>
        void Show(MessageBoxIcon icon, IFormatProvider formatProvider, string format, params object[] args);

        /// <summary>
        ///     Called when a message box needs to be shown to the user and receive a response of <see cref="ButtonResult"/>.
        /// </summary>
        /// <param name="icon">
        ///     <see cref="MessageBoxIcon"/> to be used in the UI.
        /// </param>
        /// <param name="formatProvider">
        ///     <see cref="IFormatProvider"/> for the string message.
        /// </param>
        /// <param name="buttons">
        ///     <see cref="Buttons"/> option to display to the user. 
        /// </param>
        /// <param name="caption">
        ///     Caption for the message box.
        /// </param>
        /// <param name="format">
        ///     Format of the string message, used with <paramref name="args"/>.
        /// </param>
        /// <param name="args">
        ///     Values to be displayed based on the string format: <paramref name="format"/>.
        /// </param>
        /// <returns></returns>
        ButtonResult Show(MessageBoxIcon icon, IFormatProvider formatProvider, Buttons buttons, string caption, string format, params object[] args);
    }
}
