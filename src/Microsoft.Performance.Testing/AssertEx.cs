// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.

using System.Collections.Generic;
using Microsoft.VisualStudio.TestTools.UnitTesting;

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

        public static void IsInstanceOfType<T>(
            object obj, 
            string message, 
            params object[] paramters)
        {
            Assert.IsInstanceOfType(obj, typeof(T), string.Format(message, paramters));
        }
    }
}
