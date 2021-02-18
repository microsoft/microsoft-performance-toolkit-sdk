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
        ///     Determines whether the given path is a valid
        ///     assembly.
        /// </summary>
        /// <param name="path">
        ///     The path to check.
        /// </param>
        /// <returns>
        ///     <c>true</c> if the path is to an assembly;
        ///     <c>false</c> otherwise.
        /// </returns>
        bool IsAssembly(string path);

        /// <summary>
        ///     Loads the given assembly from the given path.
        /// </summary>
        /// <param name="assemblyPath">
        ///     The path of the assembly to load.
        /// </param>
        /// <param name="error">
        ///     Contains the error(s) that occurred, if any, while
        ///     loading the assembly. If the assembly loads,
        ///     then this parameter is set to <see cref="ErrorInfo.None"/>.
        /// </param>
        /// <returns>
        ///     The loaded assembly, if possible; null otherwise.
        ///     If this method returns <c>null</c>, then the
        ///     <paramref name="error"/> parameter will contain
        ///     details as to the failure.
        /// </returns>
        Assembly LoadAssembly(
            string assemblyPath,
            out ErrorInfo error);
    }
}
