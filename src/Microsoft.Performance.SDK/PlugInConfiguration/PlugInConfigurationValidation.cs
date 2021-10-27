// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.

using System.Text.RegularExpressions;

namespace Microsoft.Performance.SDK.PluginConfiguration
{
    /// <summary>
    ///     Provides helper methods for configuration files.
    /// </summary>
    public static class PluginConfigurationValidation
    {
        private static readonly Regex ElementValidationExpression = new Regex(@"[^\w\d\._]");

        /// <summary>
        ///     A string that describes the allowable characters in a plugin configuration value.
        /// </summary>
        public static string ValidCharactersMessage =>
            "valid characters include: letters, digits, underscores, and periods.";

        /// <summary>
        ///     Check that the string value matches criteria of a plugin configuration.
        /// </summary>
        /// <param name="name">
        ///     A string value to store in a plugin configuration file.
        /// </param>
        /// <returns>
        ///     true if the value is valid; otherwise false.
        /// </returns>
        public static bool ValidateElementName(string name)
        {
            return !ElementValidationExpression.IsMatch(name);
        }
    }
}
