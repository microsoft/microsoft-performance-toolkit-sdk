// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.

using System.Collections.Generic;
using Microsoft.Performance.SDK;
using Microsoft.Performance.Toolkit.Plugins.Core.Metadata;

namespace Microsoft.Performance.Toolkit.Plugins.Runtime.Validation
{
    /// <summary>
    ///     An <see cref="IPluginValidator"/> that combines multiple <see cref="IPluginValidator"/>s.
    /// </summary>
    public class CompositePluginValidator
        : IPluginValidator
    {
        private readonly IReadOnlyCollection<IPluginValidator> validators;

        public CompositePluginValidator(IReadOnlyCollection<IPluginValidator> validators)
        {
            this.validators = validators;
        }

        /// <inheritdoc />
        public ErrorInfo[] GetValidationErrors(PluginMetadata pluginMetadata)
        {
            List<ErrorInfo> errors = new List<ErrorInfo>();

            foreach (var validator in this.validators)
            {
                ErrorInfo[] innerErrors = validator.GetValidationErrors(pluginMetadata);

                if (innerErrors != null)
                {
                    errors.AddRange(innerErrors);
                }
            }

            return errors.ToArray();
        }
    }
}
