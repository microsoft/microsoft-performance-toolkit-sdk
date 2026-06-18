// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.

using Microsoft.VisualStudio.TestTools.UnitTesting;
using System.Collections.Generic;

namespace Microsoft.Performance.Testing
{
    public static class AssertEx
    {
        public static void AreEqual<T>(
            T expected,
            T actual,
            IEqualityComparer<T> comparer)
        {
            Assert.IsNotNull(comparer);
            var areEqual = comparer.Equals(expected, actual);
            Assert.IsTrue(
                areEqual,
                $"AssertEx.AreEqual failed. Expected:<{expected}>. Actual:<{actual}>.");
        }

        public static void IsInstanceOfType<T>(object obj)
        {
            Assert.IsInstanceOfType(obj, typeof(T));
        }

        public static void IsInstanceOfType<T>(object obj, string message)
        {
            Assert.IsInstanceOfType(obj, typeof(T), message);
        }
    }
}
