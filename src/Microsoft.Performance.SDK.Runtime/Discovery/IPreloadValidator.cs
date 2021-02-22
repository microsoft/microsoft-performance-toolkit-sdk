// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.

using System;

namespace Microsoft.Performance.SDK.Runtime.Discovery
{
    /// <summary>
    ///     Performs checks against assemblies to determine
    ///     their eligibility as part of an extension.
    /// </summary>
    public interface IPreloadValidator
        : IDisposable
    {
        /// <summary>
        ///     Determines if the given path represents an assembly
        ///     that is valid to be loaded as part of an extension.
        /// </summary>
        /// <param name="fullPath">
        ///     The full path to the assembly to validate.
        /// </param>
        /// <param name="error">
        ///     If the assembly fails to validate, then this parameter
        ///     will contain the reasons. If the assembly is valid, then
        ///     this parameter will be set to <see cref="ErrorInfo.None"/>.
        /// </param>
        /// <returns>
        ///     <c>true</c> if the specified assembly is acceptable;
        ///     <c>false</c> otherwise.
        /// </returns>
        bool IsAssemblyAcceptable(string fullPath, out ErrorInfo error);
    }
}
