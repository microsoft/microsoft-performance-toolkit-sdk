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

                Assert.ThrowsExactly<ObjectDisposedException>(() => sut.Availability);
                Assert.ThrowsExactly<ObjectDisposedException>(() => sut.BuildTableAction);
                Assert.ThrowsExactly<ObjectDisposedException>(() => sut.DependencyReferences);
                Assert.ThrowsExactly<ObjectDisposedException>(() => sut.IsDataAvailableFunc);
                Assert.ThrowsExactly<ObjectDisposedException>(() => sut.TableDescriptor);

                Assert.ThrowsExactly<ObjectDisposedException>(() => sut.ProcessDependencies(new TestDataExtensionRepository()));
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
