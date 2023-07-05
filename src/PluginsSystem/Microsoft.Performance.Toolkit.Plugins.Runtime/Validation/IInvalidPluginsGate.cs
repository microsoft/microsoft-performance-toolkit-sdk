// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.

using System.Collections.Generic;
using System.Threading.Tasks;

namespace Microsoft.Performance.Toolkit.Plugins.Runtime.Validation
{
    /// <summary>
    ///     A gate to allow a plugins system operation despite plugin validation failures.
    /// </summary>
    public interface IInvalidPluginsGate
    {
        /// <summary>
        ///     Determines whether a plugins system operation should continue despite one or more
        ///     plugins having validation failures.
        /// </summary>
        /// <param name="operation">
        ///     The operation that was being performed when validation failures were encountered.
        ///     Returning <c>true</c> will continue this operation.
        /// </param>
        /// <param name="validationFailures">
        ///     The failures that were encountered.
        /// </param>
        /// <returns>
        ///     Whether or not the <paramref name="operation"/> should continue.
        /// </returns>
        Task<bool> ShouldProceedDespiteFailures(
            PluginsSystemOperation operation,
            IReadOnlyCollection<PluginValidationFailures> validationFailures);
    }
}