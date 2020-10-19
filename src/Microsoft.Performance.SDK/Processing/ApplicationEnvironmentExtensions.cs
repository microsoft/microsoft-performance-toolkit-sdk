// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.

using System;
using System.Globalization;

namespace Microsoft.Performance.SDK.Processing
{
    /// <summary>
    ///     Contains static (Shared in Visual Basic) methods for interacting
    ///     with <see cref="IApplicationEnvironment"/> instances.
    /// </summary>
    public static class ApplicationEnvironmentExtensions
    {
        /// <summary>
        ///     Displays an informational message box with the formatted text and objects.
        /// </summary>
        /// <param name="self">
        ///     The <see cref="IApplicationEnvironment"/> instance.
        /// </param>
        /// <param name="format">
        ///     A composite format string.
        /// </param>
        /// <param name="args">
        ///     The objects to format using <paramref name="format"/>.
        /// </param>
        public static void Show(
            this IApplicationEnvironment self, 
            string format,
            params object[] args)
        {
            self.Show(CultureInfo.CurrentCulture, format, args);
        }

        /// <summary>
        ///     Displays an informational message box with the formatted text and objects.
        /// </summary>
        /// <param name="self">
        ///     The <see cref="IApplicationEnvironment"/> instance.
        /// </param>
        /// <param name="formatProvider">
        ///     An object that supplies culture-specific formatting information.
        /// </param>
        /// <param name="format">
        ///     A composite format string.
        /// </param>
        /// <param name="args">
        ///     The objects to format using <paramref name="format"/>.
        /// </param>
        public static void Show(
            this IApplicationEnvironment self, 
            IFormatProvider formatProvider,
            string format,
            params object[] args)
        {
            self.DisplayMessage(MessageType.Information, formatProvider, format, args);
        }

        /// <summary>
        ///     Displays a warning message box with the formatted text and objects.
        /// </summary>
        /// <param name="self">
        ///     The <see cref="IApplicationEnvironment"/> instance.
        /// </param>
        /// <param name="format">
        ///     A composite format string.
        /// </param>
        /// <param name="args">
        ///     The objects to format using <paramref name="format"/>.
        /// </param>
        public static void ShowWarning(
            this IApplicationEnvironment self,
            string format,
            params object[] args)
        {
            self.ShowWarning(CultureInfo.CurrentCulture, format, args);
        }

        /// <summary>
        ///     Displays a warning message box with the formatted text and objects.
        /// </summary>
        /// <param name="self">
        ///     The <see cref="IApplicationEnvironment"/> instance.
        /// </param>
        /// <param name="formatProvider">
        ///     An object that supplies culture-specific formatting information.
        /// </param>
        /// <param name="format">
        ///     A composite format string.
        /// </param>
        /// <param name="args">
        ///     The objects to format using <paramref name="format"/>.
        /// </param>
        public static void ShowWarning(
            this IApplicationEnvironment self,
            IFormatProvider formatProvider,
            string format,
            params object[] args)
        {
            self.DisplayMessage(MessageType.Warning, formatProvider, format, args);
        }

        /// <summary>
        ///     Displays an error message box with the formatted text and objects.
        /// </summary>
        /// <param name="self">
        ///     The <see cref="IApplicationEnvironment"/> instance.
        /// </param>
        /// <param name="format">
        ///     A composite format string.
        /// </param>
        /// <param name="args">
        ///     The objects to format using <paramref name="format"/>.
        /// </param>
        public static void ShowError(
            this IApplicationEnvironment self, 
            string format, 
            params object[] args)
        {
            self.ShowError(CultureInfo.CurrentCulture, format, args);
        }

        /// <summary>
        ///     Displays an error message box with the formatted text and objects.
        /// </summary>
        /// <param name="self">
        ///     The <see cref="IApplicationEnvironment"/> instance.
        /// </param>
        /// <param name="formatProvider">
        ///     An object that supplies culture-specific formatting information.
        /// </param>
        /// <param name="format">
        ///     A composite format string.
        /// </param>
        /// <param name="args">
        ///     The objects to format using <paramref name="format"/>.
        /// </param>
        public static void ShowError(
            this IApplicationEnvironment self,
            IFormatProvider formatProvider,
            string format,
            params object[] args)
        {
            self.DisplayMessage(MessageType.Error, formatProvider, format, args);
        }
    }
}
