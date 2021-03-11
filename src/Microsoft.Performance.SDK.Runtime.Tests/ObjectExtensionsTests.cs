// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.

using System;
using Microsoft.Performance.Testing;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace Microsoft.Performance.SDK.Runtime.Tests
{
    [TestClass]
    public class ObjectExtensionsTests
    {
        [TestMethod]
        [UnitTest]
        public void TryDispose_WhenNull_DoesNothing()
        {
            ObjectExtensions.TryDispose(null);
        }

        [TestMethod]
        [UnitTest]
        public void TryDispose_WhenNotDisposable_DoesNothing()
        {
            new object().TryDispose();
        }

        [TestMethod]
        [UnitTest]
        public void TryDispose_WhenDisposable_Disposes()
        {
            using (var d = new FakeDisposable())
            {
                d.TryDispose();
                Assert.AreEqual(1, d.DisposeCalls);
            }
        }

        [TestMethod]
        [UnitTest]
        public void TryDispose_WhenDisposableAndThrows_DoesNotThrow()
        {
            using (var d = new FakeDisposable())
            {
                d.Exception = new ArithmeticException();
                d.TryDispose();
            }
        }

        [TestMethod]
        [UnitTest]
        public void SafeDispose_WhenNull_DoesNothing()
        {
            ObjectExtensions.SafeDispose(null);
        }

        [TestMethod]
        [UnitTest]
        public void SafeDispose_WhenDisposable_Disposes()
        {
            using (var d = new FakeDisposable())
            {
                d.SafeDispose();
                Assert.AreEqual(1, d.DisposeCalls);
            }
        }

        [TestMethod]
        [UnitTest]
        public void SafeDispose_WhenDisposableAndThrows_DoesNotThrow()
        {
            using (var d = new FakeDisposable())
            {
                d.Exception = new ArithmeticException();
                d.SafeDispose();
            }
        }

        [TestMethod]
        [UnitTest]
        public void SafeDispose_WhenDisposableAndThrows_PassesExceptionOut()
        {
            using (var d = new FakeDisposable())
            {
                d.Exception = new ArithmeticException();
                d.SafeDispose(out var e);
                Assert.AreEqual(d.Exception, e);
            }
        }

        private sealed class FakeDisposable
            : IDisposable
        {
            public Exception Exception { get; set; }

            public int DisposeCalls { get; set; }

            public void Dispose()
            {
                ++this.DisposeCalls;
                if (this.DisposeCalls > 1)
                {
                    return;
                }

                if (this.Exception != null)
                {
                    throw this.Exception;
                }
            }
        }
    }
}
