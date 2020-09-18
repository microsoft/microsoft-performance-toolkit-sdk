// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.

using System;
using Microsoft.Performance.Testing;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace Microsoft.Performance.SDK.Tests
{
    [TestClass]
    public class TypeExtensionTests
    {
        [TestMethod]
        [UnitTest]
        public void IsStaticReturnsTrueForStaticTypes()
        {
            Assert.IsTrue(typeof(StaticType).IsStatic());
        }

        [TestMethod]
        [UnitTest]
        public void IsStaticReturnsFalseForNonStaticTypes()
        {
            Assert.IsFalse(typeof(TypeExtensionTests).IsStatic());
        }

        [TestMethod]
        [UnitTest]
        public void IsStaticThrowsOnNull()
        {
            Assert.ThrowsException<ArgumentNullException>(
                () => TypeExtensions.IsStatic(null));
        }

        [TestMethod]
        [UnitTest]
        public void IsConcreteReturnsFalseForStaticTypes()
        {
            Assert.IsFalse(typeof(StaticType).IsConcrete());
        }

        [TestMethod]
        [UnitTest]
        public void IsConcreteReturnsFalseForInterfaceTypes()
        {
            Assert.IsFalse(typeof(InterfaceType).IsConcrete());
        }

        [TestMethod]
        [UnitTest]
        public void IsConcreteReturnsFalseForAbstractTypes()
        {
            Assert.IsFalse(typeof(AbstractType).IsConcrete());
        }

        [TestMethod]
        [UnitTest]
        public void IsConcreteReturnsTrueForATypeThatCanBeConstructed()
        {
            Assert.IsTrue(typeof(ConcreteType).IsConcrete());
        }

        [TestMethod]
        [UnitTest]
        public void IsConcreteThrowsOnNull()
        {
            Assert.ThrowsException<ArgumentNullException>(
                () => TypeExtensions.IsConcrete(null));
        }

        [TestMethod]
        [UnitTest]
        public void IsReturnsTrueWhenTypesAreTheSame()
        {
            Assert.IsTrue(typeof(ConcreteType).Is(typeof(ConcreteType)));
        }

        [TestMethod]
        [UnitTest]
        public void IsThrowsOnNull()
        {
            Assert.ThrowsException<ArgumentNullException>(
                () => TypeExtensions.Is(null, typeof(ConcreteType)));
            Assert.ThrowsException<ArgumentNullException>(
                () => TypeExtensions.Is(typeof(ConcreteType), null));
        }

        [TestMethod]
        [UnitTest]
        public void IsReturnsFalseWhenNotInHeirarchy()
        {
            Assert.IsFalse(typeof(ConcreteType).Is(typeof(StaticType)));
        }

        [TestMethod]
        [UnitTest]
        public void IsReturnsTrueWhenInherits()
        {
            Assert.IsTrue(typeof(InheritsType).Is(typeof(AbstractType)));
        }

        [TestMethod]
        [UnitTest]
        public void IsReturnsTrueWhenImplements()
        {
            Assert.IsTrue(typeof(ImplementsType).Is(typeof(InterfaceType)));
        }

        [TestMethod]
        [UnitTest]
        public void SuperTypeIsNotSubType()
        {
            Assert.IsFalse(typeof(AbstractType).Is(typeof(ConcreteType)));
        }

        [TestMethod]
        [UnitTest]
        public void IsGenericReturnsTrueWhenTypesAreTheSame()
        {
            Assert.IsTrue(typeof(ConcreteType).Is<ConcreteType>());
        }

        [TestMethod]
        [UnitTest]
        public void IsGenericThrowsOnNull()
        {
            Assert.ThrowsException<ArgumentNullException>(
                () => TypeExtensions.Is<ConcreteType>(null));
        }

        [TestMethod]
        [UnitTest]
        public void IsGenericReturnsFalseWhenNotInHeirarchy()
        {
            Assert.IsFalse(typeof(ConcreteType).Is<InterfaceType>());
        }

        [TestMethod]
        [UnitTest]
        public void IsGenericReturnsTrueWhenInherits()
        {
            Assert.IsTrue(typeof(InheritsType).Is<AbstractType>());
        }

        [TestMethod]
        [UnitTest]
        public void IsGenericReturnsTrueWhenImplements()
        {
            Assert.IsTrue(typeof(ImplementsType).Is<InterfaceType>());
        }

        [TestMethod]
        [UnitTest]
        public void GenericSuperTypeIsNotSubType()
        {
            Assert.IsFalse(typeof(AbstractType).Is<ConcreteType>());
        }

        private static class StaticType
        {
        }

        private interface InterfaceType
        {
        }

        private abstract class AbstractType
        {
        }

        private class ConcreteType
        {
        }

        private class InheritsType : AbstractType
        {
        }

        private class ImplementsType : InterfaceType
        {
        }
    }
}
