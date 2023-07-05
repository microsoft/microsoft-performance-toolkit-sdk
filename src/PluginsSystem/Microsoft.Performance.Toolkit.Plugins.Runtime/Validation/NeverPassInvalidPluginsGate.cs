// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.

using System.Collections.Generic;
using System.Threading.Tasks;

namespace Microsoft.Performance.Toolkit.Plugins.Runtime.Validation
{
    /// <summary>
    ///     An <see cref="IInvalidPluginsGate"/> that never passes.
    /// </summary>
    internal class NeverPassInvalidPluginsGate
        : IInvalidPluginsGate
    {
        /// <inheritdoc />
        public Task<bool> ShouldProceedDespiteFailures(PluginsSystemOperation operation, IReadOnlyCollection<PluginValidationFailures> validationFailures)
        {
            return Task.FromResult(false);
        }
    }
}
