// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.

using System;

namespace Microsoft.Performance.SDK.Processing
{
    /// <summary>
    ///     Provides a means of logging information.
    /// </summary>
    public interface ILogger
    {
        /// <summary>
        ///     Logs a verbose message.
        /// </summary>
        /// <param name="fmt">
        ///     A composite format string.
        /// </param>
        /// <param name="args">
        ///     An array of objects to be formatted.
        /// </param>
        void Verbose(string fmt, params object[] args);

        /// <summary>
        ///     Logs a verbose message with an <see cref="Exception"/>.
        /// </summary>
        /// <param name="e">
        ///     The <see cref="Exception"/> to log.
        /// </param>
        /// <param name="fmt">
        ///     A composite format string.
        /// </param>
        /// <param name="args">
        ///     An array of objects to be formatted.
        /// </param>
        void Verbose(Exception e, string fmt, params object[] args);

        /// <summary>
        ///     Logs an informational message with an <see cref="Exception"/>.
        /// </summary>
        /// <param name="fmt">
        ///     A composite format string.
        /// </param>
        /// <param name="args">
        ///     An array of objects to be formatted.
        /// </param>
        void Info(string fmt, params object[] args);

        /// <summary>
        ///     Logs an informational message with an <see cref="Exception"/>.
        /// </summary>
        /// <param name="e">
        ///     The <see cref="Exception"/> to log.
        /// </param>
        /// <param name="fmt">
        ///     A composite format string.
        /// </param>
        /// <param name="args">
        ///     An array of objects to be formatted.
        /// </param>
        void Info(Exception e, string fmt, params object[] args);

        /// <summary>
        ///     Logs a warning message with an <see cref="Exception"/>.
        /// </summary>
        /// <param name="fmt">
        ///     A composite format string.
        /// </param>
        /// <param name="args">
        ///     An array of objects to be formatted.
        /// </param>
        void Warn(string fmt, params object[] args);

        /// <summary>
        ///     Logs a warning message with an <see cref="Exception"/>.
        /// </summary>
        /// <param name="e">
        ///     The <see cref="Exception"/> to log.
        /// </param>
        /// <param name="fmt">
        ///     A composite format string.
        /// </param>
        /// <param name="args">
        ///     An array of objects to be formatted.
        /// </param>
        void Warn(Exception e, string fmt, params object[] args);

        /// <summary>
        ///     Logs an error message with an <see cref="Exception"/>.
        /// </summary>
        /// <param name="fmt">
        ///     A composite format string.
        /// </param>
        /// <param name="args">
        ///     An array of objects to be formatted.
        /// </param>
        void Error(string fmt, params object[] args);

        /// <summary>
        ///     Logs an error message with an <see cref="Exception"/>.
        /// </summary>
        /// <param name="e">
        ///     The <see cref="Exception"/> to log.
        /// </param>
        /// <param name="fmt">
        ///     A composite format string.
        /// </param>
        /// <param name="args">
        ///     An array of objects to be formatted.
        /// </param>
        void Error(Exception e, string fmt, params object[] args);

        /// <summary>
        ///     Logs a fatal message with an <see cref="Exception"/>.
        /// </summary>
        /// <param name="fmt">
        ///     A composite format string.
        /// </param>
        /// <param name="args">
        ///     An array of objects to be formatted.
        /// </param>
        void Fatal(string fmt, params object[] args);

        /// <summary>
        ///     Logs a fatal message with an <see cref="Exception"/>.
        /// </summary>
        /// <param name="e">
        ///     The <see cref="Exception"/> to log.
        /// </param>
        /// <param name="fmt">
        ///     A composite format string.
        /// </param>
        /// <param name="args">
        ///     An array of objects to be formatted.
        /// </param>
        void Fatal(Exception e, string fmt, params object[] args);
    }
}
