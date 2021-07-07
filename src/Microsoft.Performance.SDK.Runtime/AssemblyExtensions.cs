// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.

using System;
using System.Linq;
using System.Reflection;
using NuGet.Versioning;

namespace Microsoft.Performance.SDK.Runtime
{
    /// <summary>
    ///     Encapsulates functionality used to extend the Assembly implementation.
    /// </summary>
    public static class AssemblyExtensions
    {
        /// <summary>
        ///     Gets a local path to the given <see cref="Assembly"/>.
        /// </summary>
        /// <param name="self">
        ///     The target <see cref="Assembly"/>.
        /// </param>
        /// <returns>
        ///     The local operating-system representation of a file path to the <see cref="Assembly"/>.
        /// </returns>
        public static string GetCodeBaseAsLocalPath(this Assembly self)
        {
            Guard.NotNull(self, nameof(self));

            var codeBaseUri = new Uri(self.CodeBase);
            return codeBaseUri.LocalPath;
        }

        /// <summary>
        ///     Gets the <see cref="SemanticVersion"/> of the given assembly.
        ///     Calling this method for assemblies that use schemes other than
        ///     Semantic Versioning is undefined, and may produce erroneous
        ///     results.
        /// </summary>
        /// <param name="assembly">
        ///     The <see cref="Assembly"/> whose version is to be returned as
        ///     a <see cref="SemanticVersion"/>.
        /// </param>
        /// <returns>
        ///     The <see cref="SemanticVersion"/> of the given <paramref name="assembly"/>.
        /// </returns>
        public static SemanticVersion GetSemanticVersion(this Assembly assembly)
        {
            return assembly.GetName().GetSemanticVersion();
        }

        /// <summary>
        ///     Gets the <see cref="SemanticVersion"/> of the given assembly.
        ///     Calling this method for assemblies that use schemes other than
        ///     Semantic Versioning is undefined, and may produce erroneous
        ///     results.
        /// </summary>
        /// <param name="assemblyName">
        ///     The <see cref="AssemblyName"/> of the assembly whose version is to
        ///     be returned as a <see cref="SemanticVersion"/>.
        /// </param>
        /// <returns>
        ///     The <see cref="SemanticVersion"/> from the given <paramref name="assemblyName"/>.
        /// </returns>
        public static SemanticVersion GetSemanticVersion(this AssemblyName assemblyName)
        {
            return new SemanticVersion(
                assemblyName.Version.Major,
                assemblyName.Version.Minor,
                assemblyName.Version.Build);
        }

        /// <summary>
        ///     Determines whether the given <see cref="Assembly"/> references the
        ///     SDK.
        /// </summary>
        /// <param name="self">
        ///     The <see cref="Assembly"/> to check.
        /// </param>
        /// <returns>
        ///     <c>true</c> if <paramref name="self" /> references the SDK;
        ///     <c>false</c> otherwise.
        /// </returns>
        public static bool ReferencesSdk(this Assembly self)
        {
            return self.GetReferencedAssemblies()
                .Any(x => x.Name.Equals(SdkAssembly.Assembly.GetName().Name));
        }
    }
}
