// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.

using System.Reflection;

namespace Microsoft.Performance.SDK.Runtime.Discovery
{
    /// <summary>
    ///     Loads assemblies.
    /// </summary>
    public interface IAssemblyLoader
    {
        /// <summary>
        ///     Gets a value indicating whether this loader supports
        ///     loading assemblies into an isolated context.
        /// </summary>
        bool SupportsIsolation { get; }

        /// <summary>
        ///     Loads the given assembly from the given path.
        /// </summary>
        /// <param name="assemblyPath">
        ///     The path of the assembly to load.
        /// </param>
        /// <returns>
        ///     The loaded assembly, if possible; null otherwise.
        /// </returns>
        Assembly LoadAssembly(
            string assemblyPath);
    }
}
