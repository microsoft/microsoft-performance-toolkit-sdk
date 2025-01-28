// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.

using System;
using System.Globalization;
using Microsoft.Performance.SDK.Auth;
using Microsoft.Performance.SDK.Options;

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

        /// <summary>
        ///     Attempts to get an <see cref="IAuthProvider{TAuth, TResult}"/> that can provide authentication
        ///     for <see cref="IAuthMethod{TResult}"/> of type <typeparamref name="TAuth"/>.
        /// </summary>
        /// <param name="self">
        ///     The <see cref="IApplicationEnvironment"/> instance.
        /// </param>
        /// <param name="provider">
        ///     The found provider, or <c>null</c> if no registered provider can provide authentication for
        ///     <typeparamref name="TAuth"/>.
        /// </param>
        /// <typeparam name="TAuth">
        ///     The type of the <see cref="IAuthMethod{TResult}"/> for which to attempt to get a provider.
        /// </typeparam>
        /// <typeparam name="TResult">
        ///     The type of the result of a successful authentication for <typeparamref name="TAuth"/>.
        /// </typeparam>
        /// <returns>
        ///     <c>true</c> if a provider was found; <c>false</c> otherwise. If <c>false</c> is returned,
        ///     <paramref name="provider"/> will be <c>null</c>.
        /// </returns>
        public static bool TryGetAuthProvider<TAuth, TResult>(
            this IApplicationEnvironment self,
            out IAuthProvider<TAuth, TResult> provider)
            where TAuth : IAuthMethod<TResult>
        {
            if (self is IApplicationEnvironmentV2 v2)
            {
                return v2.TryGetAuthProvider(out provider);
            }

            provider = null;
            return false;
        }

        /// <summary>
        ///     Attempts to get the plugin option of type <typeparamref name="T"/> with the given GUID.
        /// </summary>
        /// <param name="optionGuid">
        ///     The <see cref="Guid"/> of the option to get.
        /// </param>
        /// <param name="option">
        ///     The found option if this method returns <c>true</c>; <c>null</c> otherwise.
        /// </param>
        /// <typeparam name="T">
        ///     The concrete type of the <see cref="PluginOption"/> to get. Must be a subclass of <see cref="PluginOption"/>.
        /// </typeparam>
        /// <returns>
        ///     <c>true</c> if the option was found; <c>false</c> otherwise.
        /// </returns>
        public static bool TryGetPluginOption<T>(
            this IApplicationEnvironment self,
            Guid optionGuid,
            out T option)
            where T : PluginOption
        {
            if (self is IApplicationEnvironmentV3 v3)
            {
                return v3.TryGetPluginOption(optionGuid, out option);
            }

            option = null;
            return false;
        }
    }
}
