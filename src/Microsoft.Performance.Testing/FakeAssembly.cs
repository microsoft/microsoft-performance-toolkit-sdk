// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.

using System;
using System.Reflection;

namespace Microsoft.Performance.Testing
{
    public sealed class FakeAssembly
        : Assembly
    {
        public FakeAssembly()
        {
            this.AssemblyName = new AssemblyName("MyFakeAssembly");
            this.CodeBaseToReturn = Guid.NewGuid().ToString();
            this.LocationToReturn = Guid.NewGuid().ToString();
            this.ReferencedAssemblies = Array.Empty<AssemblyName>();
            this.TypesToReturn = Type.EmptyTypes;
        }

        public AssemblyName AssemblyName { get; set; }
        public string CodeBaseToReturn { get; set; }
        public string LocationToReturn { get; set; }
        public AssemblyName[] ReferencedAssemblies { get; set; }
        public Type[] TypesToReturn { get; set; }

        public override string CodeBase => this.CodeBaseToReturn;
        public override string Location => this.LocationToReturn;

        public override AssemblyName GetName()
        {
            return this.AssemblyName;
        }

        public override AssemblyName[] GetReferencedAssemblies()
        {
            return this.ReferencedAssemblies ?? Array.Empty<AssemblyName>();
        }

        public override Type[] GetTypes()
        {
            return this.TypesToReturn ?? Array.Empty<Type>();
        }
    }
}
