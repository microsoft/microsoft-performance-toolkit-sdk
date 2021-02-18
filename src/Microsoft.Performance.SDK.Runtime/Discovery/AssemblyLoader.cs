// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.

using System.Reflection;

namespace Microsoft.Performance.SDK.Runtime.Discovery
{
    /// <summary>
    ///     Loads assemblies into the default load context.
    /// </summary>
    public sealed class AssemblyLoader
        : AssemblyLoaderBase
    {
        /// <inheritdoc />
        public override bool SupportsIsolation => false;

        /// <inheritdoc />
        protected override Assembly LoadFromPath(string path)
        {
            return Assembly.LoadFrom(path);
        }
    }
}
