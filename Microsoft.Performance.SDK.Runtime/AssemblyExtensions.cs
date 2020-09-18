// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.

using System;
using System.Reflection;

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
    }
}
