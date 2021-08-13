// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.

using System.Collections.Generic;
using System.Reflection;
using Microsoft.Performance.SDK;
using Microsoft.Performance.SDK.Runtime.Discovery;

namespace Microsoft.Performance.Toolkit.Engine.Tests
{
    internal sealed class AssemblyLoaderSpy
        : IAssemblyLoader
    {
        private readonly IAssemblyLoader wrapped;

        public AssemblyLoaderSpy(IAssemblyLoader wrapped)
        {
            this.wrapped = wrapped;

            this.IsAssemblyCalls = new List<string>();
            this.LoadAssemblyCalls = new List<string>();
        }

        public bool SupportsIsolation => this.wrapped.SupportsIsolation;

        public List<string> IsAssemblyCalls { get; }

        public bool IsAssembly(string path)
        {
            this.IsAssemblyCalls.Add(path);

            return this.wrapped.IsAssembly(path);
        }

        public List<string> LoadAssemblyCalls { get; }

        public Assembly LoadAssembly(string assemblyPath, out ErrorInfo error)
        {
            this.LoadAssemblyCalls.Add(assemblyPath);

            return this.wrapped.LoadAssembly(assemblyPath, out error);
        }
    }
}
