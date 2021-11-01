// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.

using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using Microsoft.Performance.SDK.Processing;
using Microsoft.Performance.SDK.Runtime.Discovery;
using Microsoft.Performance.Testing;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace Microsoft.Performance.SDK.Runtime.Tests.Discovery
{
    [TestClass]
    public class ReflectionProcessingSourceCatalogTests
    {
        private FakeReferenceFactory ReferenceFactory { get; set; }
        private ReflectionProcessingSourceCatalog Sut { get; set; }
        private TestExtensionProvider ExtensionProvider { get; set; }

        [TestInitialize]
        public void Initialize()
        {
            this.ReferenceFactory = new FakeReferenceFactory();
            this.ExtensionProvider = new TestExtensionProvider();
            this.Sut = new ReflectionProcessingSourceCatalog(
                this.ExtensionProvider,
                this.ReferenceFactory.TryCreateProcessingSourceReference);
        }

        [TestMethod]
        [UnitTest]
        public void AddInsEmptyOnConstruction()
        {
            Assert.IsFalse(this.Sut.ProcessingSources.Any());
        }

        [TestMethod]
        [UnitTest]
        public void SkipsInvalidType()
        {
            var testType = typeof(CdsWithoutCdsAttribute);
            this.Sut.ProcessType(testType, testType.FullName);
            Assert.IsFalse(this.Sut.ProcessingSources.Any());
        }

        [TestMethod]
        [UnitTest]
        public void AddsValidTypes()
        {
            var testType1 = typeof(CdsOne);
            var testType1Guid = testType1.GetCustomAttribute<ProcessingSourceAttribute>().Guid;

            var testType2 = typeof(CdsTwo);
            var testType2Guid = testType2.GetCustomAttribute<ProcessingSourceAttribute>().Guid;

            this.ReferenceFactory.SetupReference(testType1);
            this.ReferenceFactory.SetupReference(testType2);

            this.Sut.ProcessType(testType1, testType1.FullName);
            Assert.AreEqual(1, this.Sut.ProcessingSources.Count());

            this.Sut.ProcessType(testType2, testType2.FullName);
            Assert.AreEqual(2, this.Sut.ProcessingSources.Count());

            Assert.IsTrue(this.Sut.ProcessingSources.Any(cds => cds.Guid == testType1Guid));
            Assert.IsTrue(this.Sut.ProcessingSources.Any(cds => cds.Guid == testType2Guid));
        }

        private sealed class FakeReferenceFactory
        {
            public FakeReferenceFactory()
            {
                this.TypeToReference = new Dictionary<Type, ProcessingSourceReference>();
            }

            public IDictionary<Type, ProcessingSourceReference> TypeToReference { get; }

            public bool TryCreateProcessingSourceReference(
                Type type,
                out ProcessingSourceReference reference)
            {
                return this.TypeToReference.TryGetValue(type, out reference);
            }

            public void SetupReference(Type type)
            {
                if (type != null)
                {
                    if (ProcessingSourceReference.TryCreateReference(
                        type,
                        out ProcessingSourceReference reference))
                    {
                        this.TypeToReference[type] = reference;
                    }
                }
            }
        }
    }
}
