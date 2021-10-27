// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.

using System;
using Microsoft.Performance.SDK.Runtime.Extensibility.DataExtensions.Tables;
using Microsoft.Performance.SDK.Runtime.Tests.Extensibility.TestClasses;
using Microsoft.Performance.SDK.Tests.TestClasses;
using Microsoft.Performance.Testing;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace Microsoft.Performance.SDK.Runtime.Tests.Extensibility
{
    [TestClass]
    public class TableExtensionReferenceTests
    {
        [TestMethod]
        [UnitTest]
        public void WhenDisposed_EverthingThrows()
        {
            ITableExtensionReference sut = null;
            try
            {
                var result = TableExtensionReference.TryCreateReference(
                    typeof(TestTableExtension),
                    out sut);

                Assert.IsTrue(result);

                sut.Dispose();

                Assert.ThrowsException<ObjectDisposedException>(() => sut.Availability);
                Assert.ThrowsException<ObjectDisposedException>(() => sut.BuildTableAction);
                Assert.ThrowsException<ObjectDisposedException>(() => sut.DependencyReferences);
                Assert.ThrowsException<ObjectDisposedException>(() => sut.IsDataAvailableFunc);
                Assert.ThrowsException<ObjectDisposedException>(() => sut.TableDescriptor);

                Assert.ThrowsException<ObjectDisposedException>(() => sut.ProcessDependencies(new TestDataExtensionRepository()));
            }
            finally
            {
                sut?.Dispose();
            }
        }

        [TestMethod]
        [UnitTest]
        public void CanDisposeMultipleTimes()
        {
            ITableExtensionReference sut = null;
            try
            {
                var result = TableExtensionReference.TryCreateReference(
                    typeof(TestTableExtension),
                    out sut);

                Assert.IsTrue(result);

                sut.Dispose();
                sut.Dispose();
                sut.Dispose();
            }
            finally
            {
                sut?.Dispose();
            }
        }
    }
}
