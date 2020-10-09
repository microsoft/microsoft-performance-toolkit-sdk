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
        public Assembly LoadAssembly(string assemblyPath)
        {
            if (!CliUtils.IsCliAssembly(assemblyPath))
            {
                return null;
            }

            Assembly loadedAssembly;

            try
            {
                loadedAssembly = Assembly.LoadFrom(assemblyPath);
            }
            catch (BadImageFormatException)
            {
                //
                // this means it is native code or otherwise
                // not readable by the CLR.
                //

                loadedAssembly = null;
            }
            catch (FileLoadException e)
            {
                Console.Error.WriteLine(
                    "[warn]: managed assembly `{0}` cannot be loaded - {1}.",
                    assemblyPath,
                    e.FusionLog);
                loadedAssembly = null;
            }
            catch (FileNotFoundException)
            {
                loadedAssembly = null;
            }

            return loadedAssembly;
        }
    }
}
