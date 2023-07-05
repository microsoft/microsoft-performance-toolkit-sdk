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

        public bool IsValid(PluginMetadata pluginMetadata, out ErrorInfo[] errorInfos)
        {
            List<ErrorInfo> errors = new List<ErrorInfo>();
            bool isValid = true;

            foreach (var validator in this.validators)
            {
                if (!validator.IsValid(pluginMetadata, out ErrorInfo[] innerErrors))
                {
                    isValid = false;
                    errors.AddRange(innerErrors);
                }
            }

            errorInfos = errors.ToArray();
            return isValid;
        }
    }
}