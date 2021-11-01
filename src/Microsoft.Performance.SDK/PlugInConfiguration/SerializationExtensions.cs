// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.

using System.Collections.Generic;
using System.Linq;
using Microsoft.Performance.SDK.Processing;
using NuGet.Versioning;

namespace Microsoft.Performance.SDK.PluginConfiguration
{
    internal static class SerializationExtensions
    {
        internal static ConfigurationOption ConfigurationOptionFromDTO(this ConfigurationOptionDTO sourceOption)
        {
            var result = new ConfigurationOption(sourceOption.Name, sourceOption.Description);
            result.AddApplications(sourceOption.Applications);
            result.AddRuntimes(sourceOption.Runtimes);

            return result;
        }

        internal static ConfigurationOptionDTO ConfigurationOptionsToDTO(this ConfigurationOption sourceOption)
        {
            var result = new ConfigurationOptionDTO
            {
                Name = sourceOption.Name,
                Description = sourceOption.Description,
                Applications = sourceOption.Applications.ToArray(),
                Runtimes = sourceOption.Runtimes.ToArray(),
            };

            return result;
        }

        internal static PluginConfiguration ConfigurationFromDTO(this PluginConfigurationDTO source, ILogger logger)
        {
            var options = new HashSet<ConfigurationOption>();
            foreach (var configurationOption in source.Options)
            {
                options.Add(configurationOption.ConfigurationOptionFromDTO());
            }

            if (!SemanticVersion.TryParse(source.Version, out var version))
            {
                logger?.Error("Unable to parse {0} version: {1}", nameof(PluginConfiguration), source.Version);
                return null;
            }

            var result = new PluginConfiguration(source.PluginName, version, options);

            return result;
        }

        internal static PluginConfigurationDTO ConfigurationToDTO(this PluginConfiguration source)
        {
            var options = new List<ConfigurationOptionDTO>();
            foreach (var configurationOption in source.Options)
            {
                options.Add(configurationOption.ConfigurationOptionsToDTO());
            }

            var result = new PluginConfigurationDTO
            {
                PluginName = source.PluginName,
                Version = source.Version.ToString(),
                Options = options.ToArray(),
            };

            return result;
        }
    }
}
