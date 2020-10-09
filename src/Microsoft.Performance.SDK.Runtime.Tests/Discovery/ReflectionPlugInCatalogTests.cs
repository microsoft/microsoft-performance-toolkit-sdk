// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.

using Microsoft.Performance.SDK.Processing;
using Microsoft.Performance.SDK.Runtime.Discovery;
using Microsoft.Performance.Testing;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;

namespace Microsoft.Performance.SDK.Runtime.Tests.Discovery
{
    [TestClass]
    public class ReflectionPlugInCatalogTests
    {
        private FakeReferenceFactory ReferenceFactory { get; set; }
        private ReflectionPlugInCatalog Sut { get; set; }
        private TestExtensionProvider ExtensionProvider { get; set; }

        [TestInitialize]
        public void Initialize()
        {
            this.ReferenceFactory = new FakeReferenceFactory();
            this.ExtensionProvider = new TestExtensionProvider();
            this.Sut = new ReflectionPlugInCatalog(
                this.ExtensionProvider,
                this.ReferenceFactory.TryCreateCustomDataSourceReference);
        }

        [TestMethod]
        [UnitTest]
        public void AddInsEmptyOnConstruction()
        {
            Assert.IsFalse(this.Sut.PlugIns.Any());
        }

        [TestMethod]
        [UnitTest]
        public void SkipsInvalidType()
        {
            var testType = typeof(CdsWithoutCdsAttribute);
            this.Sut.ProcessType(testType, testType.FullName);
            Assert.IsFalse(this.Sut.PlugIns.Any());
        }

        [TestMethod]
        [UnitTest]
        public void AddsValidTypes()
        {
            var testType1 = typeof(CdsOne);
            var testType1Guid = testType1.GetCustomAttribute<CustomDataSourceAttribute>().Guid;

            var testType2 = typeof(CdsTwo);
            var testType2Guid = testType2.GetCustomAttribute<CustomDataSourceAttribute>().Guid;

            this.ReferenceFactory.SetupReference(testType1);
            this.ReferenceFactory.SetupReference(testType2);

            this.Sut.ProcessType(testType1, testType1.FullName);
            Assert.AreEqual(1, this.Sut.PlugIns.Count());

            this.Sut.ProcessType(testType2, testType2.FullName);
            Assert.AreEqual(2, this.Sut.PlugIns.Count());

            Assert.IsTrue(this.Sut.PlugIns.Any(cds => cds.Guid == testType1Guid));
            Assert.IsTrue(this.Sut.PlugIns.Any(cds => cds.Guid == testType2Guid));
        }

        private sealed class FakeReferenceFactory
        {
            public FakeReferenceFactory()
            {
                this.TypeToReference = new Dictionary<Type, CustomDataSourceReference>();
            }

            public IDictionary<Type, CustomDataSourceReference> TypeToReference { get; }

            public bool TryCreateCustomDataSourceReference(
                Type type,
                out CustomDataSourceReference reference)
            {
                return this.TypeToReference.TryGetValue(type, out reference);
            }

            public void SetupReference(Type type)
            {
                if (type != null)
                {
                    if (CustomDataSourceReference.TryCreateReference(
                        type,
                        out CustomDataSourceReference reference))
                    {
                        this.TypeToReference[type] = reference;
                    }
                }
            }
        }
    }
}
