// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.

using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Reflection;

namespace Microsoft.Performance.SDK.Runtime.Extensibility
{
    /// <summary>
    /// Exposes the types from an assembly.
    /// </summary>
    [DebuggerDisplay("{DebuggerDisplay}")]
    internal class AssemblyTypeSource
        : ITypeSource
    {
        private readonly Assembly assembly;

        public AssemblyTypeSource(Assembly assembly)
        {
            Guard.NotNull(assembly, nameof(assembly));

            this.assembly = assembly;
        }

        public string Name => this.assembly.GetName().ToString();

        public IEnumerable<Type> Types => this.assembly.GetTypes();

        private string DebuggerDisplay => this.assembly.GetCodeBaseAsLocalPath();
    }
}
