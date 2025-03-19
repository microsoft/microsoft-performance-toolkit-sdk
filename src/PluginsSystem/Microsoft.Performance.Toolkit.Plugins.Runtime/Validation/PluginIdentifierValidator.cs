// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.

using System;
using Microsoft.Performance.SDK;
using Microsoft.Performance.SDK.Processing;
using Microsoft.Performance.Toolkit.Plugins.Core.Metadata;

namespace Microsoft.Performance.Toolkit.Plugins.Runtime.Validation
{
    /// <summary>
    ///     Validates a plugin's <see cref="PluginMetadata.Identity"/> has a valid identifier.
    /// </summary>
    public class PluginIdentifierValidator
        : IPluginValidator
    {
        private readonly ILogger logger;

        public PluginIdentifierValidator(ILogger logger)
        {
            this.logger = logger;
        }

        public ErrorInfo[] GetValidationErrors(PluginMetadata pluginMetadata)
        {
            bool valid = pluginMetadata.Identity.HasValidId(out string errorMessage);

            if (!valid)
            {
                this.logger.Error(errorMessage);
                return new []{ new ErrorInfo(ErrorCodes.PLUGINS_VALIDATION_InvalidPluginId, errorMessage) };
            }

            return Array.Empty<ErrorInfo>();
        }
    }
}
