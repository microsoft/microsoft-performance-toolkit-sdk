// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.

using System.Reflection;
using Microsoft.Performance.SDK.Processing;

namespace Microsoft.Performance.SDK.Runtime.Discovery
{
    /// <summary>
    ///     Loads assemblies into the default load context.
    /// </summary>
    public sealed class AssemblyLoader
        : AssemblyLoaderBase
    {
        /// <summary>
        ///     Initializes a new instance of the <see cref="AssemblyLoader"/>
        ///     class.
        /// </summary>
        public AssemblyLoader()
            : this(Logger.Create<AssemblyLoader>())
        {
        }

        /// <summary>
        ///     Initializes a new instance of the <see cref="AssemblyLoader"/>
        ///     class.
        /// </summary>
        /// <param name="logger">
        ///     Logs messages during loading.
        /// </param>
        /// <exception cref="System.ArgumentNullException">
        ///     <paramref name="logger"/> is <c>null</c>.
        /// </exception>
        public AssemblyLoader(ILogger logger)
            : base(logger)
        {
        }

        /// <inheritdoc />
        public override bool SupportsIsolation => false;

        /// <inheritdoc />
        protected override Assembly LoadFromPath(string path)
        {
            return Assembly.LoadFrom(path);
        }
    }
}
