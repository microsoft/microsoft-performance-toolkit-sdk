// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.

using System;
using System.Collections.Generic;
using System.Linq;
using Microsoft.Performance.SDK.Processing;
using NuGet.Versioning;

namespace Microsoft.Performance.SDK.PlugInConfiguration
{
    /// <summary>
    ///     A plug-in configuration exposes a set of option and subscribers to those options.
    /// </summary>
    public class PlugInConfiguration
    {
        /// <summary>
        ///     The plug-in configuration is English and case-sensitive.
        /// </summary>
        public static readonly IEqualityComparer<string> Comparer = StringComparer.InvariantCulture;

        private readonly List<ConfigurationOption> options;

        /// <summary>
        ///     Create a plug-in configuration.
        /// </summary>
        /// <param name="name">
        ///     Name of the plug-in.
        /// </param>
        /// <param name="version">
        ///     Version of the configuration.
        /// </param>
        /// <param name="options">
        ///     A set of option to include in the configuration.
        /// </param>
        public PlugInConfiguration(string name, SemanticVersion version, ISet<ConfigurationOption> options)
        {
            Guard.NotNullOrWhiteSpace(name, nameof(name));
            Guard.NotNull(version, nameof(version));
            Guard.NotNull(options, nameof(options));

            if (!PlugInConfigurationValidation.ValidateElementName(name))
            {
                throw new ArgumentException(message:
                    "The name is invalid: " + PlugInConfigurationValidation.ValidCharactersMessage,
                    nameof(name));
            }

            this.PlugInName = name;
            this.Version = version;
            this.options = new List<ConfigurationOption>(options);
            this.Options = this.options.AsReadOnly();
        }

        /// <summary>
        ///     Gets the name of the plug-in to which this configuration belongs.
        /// </summary>
        public string PlugInName { get; internal set; }

        /// <summary>
        /// Gets the configuration file version.
        /// </summary>
        public SemanticVersion Version { get; internal set; }

        /// <summary>
        ///     Gets a set of options to opt into.
        /// </summary>
        public IReadOnlyList<ConfigurationOption> Options { get; internal set; }

        /// <summary>
        ///     Find an option with the given name.
        /// </summary>
        /// <param name="name">
        ///     Option name.
        /// </param>
        /// <returns>
        ///     An option if found, or <c>null</c>.
        /// </returns>
        public ConfigurationOption FindOption(string name)
        {
            if (!this.options.Any())
            {
                return null;
            }

            var option = this.options.FirstOrDefault(
                opt => Comparer.Equals(opt.Name, name));

            return option;
        }

        /// <summary>
        ///     Check if an option is enabled for an application or a runtime.
        /// </summary>
        /// <param name="optionName">
        ///     Name of the option.
        /// </param>
        /// <param name="applicationName">
        ///     Name of an application.
        /// </param>
        /// <param name="runtimeName">
        ///     Name of a runtime.
        /// </param>
        /// <returns>
        ///     true if the option is enabled; false otherwise.
        /// </returns>
        public bool OptionEnabled(string optionName, string applicationName, string runtimeName)
        {
            if (!PlugInConfigurationValidation.ValidateElementName(optionName))
            {
                return false;
            }

            var option = FindOption(optionName);
            if (option == null)
            {
                return false;
            }

            if (!string.IsNullOrWhiteSpace(applicationName) && 
                PlugInConfigurationValidation.ValidateElementName(applicationName))
            {
                if (option.Applications.Contains(applicationName, Comparer))
                {
                    return true;
                }
            }

            if (!string.IsNullOrWhiteSpace(runtimeName) &&
                PlugInConfigurationValidation.ValidateElementName(runtimeName))
            {
                return option.Runtimes.Contains(runtimeName, Comparer);
            }

            return false;
        }
    }

    /// <summary>
    ///     Some extensions for PlugInConfiguration objects.
    /// </summary>
    public static class PlugInConfigurationExtensions
    {
        /// <summary>
        ///     Check if an option is enabled for an application or a runtime.
        /// </summary>
        /// <param name="configuration">
        ///     The plug-in configuration.
        /// </param>
        /// <param name="optionName">
        ///     Name of the option.
        /// </param>
        /// <param name="environment">
        ///     The application environment contains the application and runtime names
        ///     for which to determine the options to apply (if any.)
        /// </param>
        /// <returns>
        ///     true if the option is enabled; false otherwise.
        /// </returns>
        public static bool OptionEnabled(
            this PlugInConfiguration configuration,
            string optionName,
            IApplicationEnvironment environment)
        {
            Guard.NotNull(configuration, nameof(configuration));

            return configuration.OptionEnabled(optionName, environment.ApplicationName, environment.RuntimeName);
        }
    }
}
