// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.

using System;
using System.IO;
using System.Reflection;

namespace Microsoft.Performance.SDK.Runtime.Discovery
{
    /// <summary>
    ///     Loads assemblies into the default load context.
    /// </summary>
    public sealed class AssemblyLoader
        : IAssemblyLoader
    {
        /// <inheritdoc />
        public bool SupportsIsolation => false;

        /// <summary>
        ///     Loads the specified path as an assembly into the default load context.
        /// </summary>
        /// <param name="assemblyPath">
        ///     Path to an assembly.
        /// </param>
        public Assembly LoadAssembly(string assemblyPath, out ErrorInfo error)
        {
            if (!CliUtils.IsCliAssembly(assemblyPath))
            {
                error = new ErrorInfo(
                    ErrorCodes.AssemblyLoadFailed,
                    "The given file is not a managed assembly.")
                {
                    Target = assemblyPath,
                };

                return null;
            }

            Assembly loadedAssembly;
            try
            {
                loadedAssembly = Assembly.LoadFrom(assemblyPath);
                error = ErrorInfo.None;
                return loadedAssembly;
            }
            catch (BadImageFormatException)
            {
                error = new ErrorInfo(
                    ErrorCodes.AssemblyLoadFailed,
                    "The given file is not readable by the CLR.")
                {
                    Target = assemblyPath,
                };

                return null;
            }
            catch (FileLoadException e)
            {
                Console.Error.WriteLine(
                    "[warn]: managed assembly `{0}` cannot be loaded - {1}.",
                    assemblyPath,
                    e.FusionLog);

                error = new ErrorInfo(
                    ErrorCodes.AssemblyLoadFailed,
                    "An error occurred while loading the assembly")
                {
                    Target = assemblyPath,
                };

                return null;
            }
            catch (FileNotFoundException)
            {
                error = new ErrorInfo(
                    ErrorCodes.FileNotFound,
                    "The file could not be found.")
                {
                    Target = assemblyPath,
                };

                return null;
            }
        }
    }
}
