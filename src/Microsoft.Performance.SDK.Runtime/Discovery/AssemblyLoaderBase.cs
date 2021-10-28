// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.

using System;
using System.IO;
using System.Reflection;
using Microsoft.Performance.SDK.Processing;

namespace Microsoft.Performance.SDK.Runtime.Discovery
{
    /// <summary>
    ///     Base class for assembly loaders.
    /// </summary>
    public abstract class AssemblyLoaderBase
        : IAssemblyLoader
    {
        /// <summary>
        ///     Initializes a new instance of the <see cref="AssemblyLoaderBase"/>
        ///     class.
        /// </summary>
        /// <param name="logger">
        ///     Logs messages during loading.
        /// </param>
        /// <exception cref="System.ArgumentNullException">
        ///     <paramref name="logger"/> is <c>null</c>.
        /// </exception>
        protected AssemblyLoaderBase(ILogger logger)
        {
            Guard.NotNull(logger, nameof(logger));

            this.Log = logger;
        }

        /// <inheritdoc />
        public abstract bool SupportsIsolation { get; }

        /// <summary>
        ///     Gets the interface for logging messages from this instance.
        /// </summary>
        protected ILogger Log { get; }

        /// <inheritdoc />
        public bool IsAssembly(string path)
        {
            return CliUtils.IsCliAssembly(path);
        }

        /// <summary>
        ///     Loads the specified path as an assembly.
        /// </summary>
        /// <param name="assemblyPath">
        ///     Path to an assembly.
        /// </param>
        /// <param name="error">
        ///     If this method fails, then this parameter receives information
        ///     about the error that occurred.
        /// </param>
        /// <returns>
        ///     The loaded assembly, or <c>null</c> if the path cannot be loaded.
        /// </returns>
        public Assembly LoadAssembly(string assemblyPath, out ErrorInfo error)
        {
            error = ErrorInfo.None;

            if (!this.IsAssembly(assemblyPath))
            {
                error = new ErrorInfo(
                   ErrorCodes.InvalidCliAssembly,
                   ErrorCodes.InvalidCliAssembly.Description)
                {
                    Target = assemblyPath,
                };

                return null;
            }

            try
            {
                return this.LoadFromPath(assemblyPath);
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
                this.Log.Warn(
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
            catch (Exception e)
            {
                error = new ErrorInfo(
                    ErrorCodes.AssemblyLoadFailed,
                    "The assembly cannot be loaded. See the details for more information.")
                {
                    Target = assemblyPath,
                    Details = new[]
                    {
                        new ErrorInfo(
                            ErrorCodes.Unexpected,
                            e.Message)
                        {
                            Target = assemblyPath,
                        },
                    },
                };

                return null;
            }
        }

        /// <summary>
        ///     When overridden in a derived class, performs the actual
        ///     load of the assembly from the specified path.
        /// </summary>
        /// <param name="assemblyPath">
        ///     Path to an assembly.
        /// </param>
        /// <returns>
        ///     The loaded assembly.
        /// </returns>
        protected abstract Assembly LoadFromPath(string path);
    }
}
