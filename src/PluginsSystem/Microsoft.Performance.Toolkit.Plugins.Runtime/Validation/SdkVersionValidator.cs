// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.

using System;
using Microsoft.Performance.SDK;
using Microsoft.Performance.SDK.Processing;
using Microsoft.Performance.SDK.Runtime;
using Microsoft.Performance.Toolkit.Plugins.Core.Metadata;
using NuGet.Versioning;

namespace Microsoft.Performance.Toolkit.Plugins.Runtime.Validation
{
    /// <summary>
    ///     Validates a plugin's <see cref="PluginMetadata.SdkVersion"/> is compatible with
    ///     the currently running's SDK version.
    /// </summary>
    public class SdkVersionValidator
        : IPluginValidator
    {
        private readonly VersionChecker versionChecker = VersionChecker.Create();
        private readonly ILogger logger;

        public SdkVersionValidator(ILogger logger)
        {
            this.logger = logger;
        }

        public ErrorInfo[] GetValidationErrors(PluginMetadata pluginMetadata)
        {
            SemanticVersion pluginSdkVersion = new SemanticVersion(
                pluginMetadata.SdkVersion.Major,
                pluginMetadata.SdkVersion.Minor,
                pluginMetadata.SdkVersion.Build);

            bool valid = versionChecker.IsVersionSupported(pluginSdkVersion);

            if (!valid)
            {
                string errorMessage =
                    $"This plugin depends on SDK version {pluginSdkVersion}, which is incompatible with the current SDK version ({versionChecker.Sdk})";

                this.logger.Error(errorMessage);

                return new []{ new ErrorInfo(ErrorCodes.PLUGINS_VALIDATION_UnsupportedSdkVersion, errorMessage) };
            }
            return Array.Empty<ErrorInfo>();
        }
    }
}
