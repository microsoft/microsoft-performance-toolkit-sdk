// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.

using System;
using System.Reflection;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace Microsoft.Performance.Testing
{
    public sealed class FakeAssembly
        : Assembly
    {
        public FakeAssembly()
        {
            this.TypesToReturn = Type.EmptyTypes;
            this.CodeBaseToReturn = Guid.NewGuid().ToString();
            this.LocationToReturn = Guid.NewGuid().ToString();
        }

        public Type[] TypesToReturn { get; set; }
        public string CodeBaseToReturn { get; set; }
        public string LocationToReturn { get; set; }

        public override string CodeBase => this.CodeBaseToReturn;
        public override string Location => this.LocationToReturn;

        public override Type[] GetTypes()
        {
            Assert.IsNotNull(this.TypesToReturn);

            return this.TypesToReturn;
        }
    }
}
