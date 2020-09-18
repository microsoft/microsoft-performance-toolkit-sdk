// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.

using System;
using System.Collections.Generic;
using System.Linq;
using Microsoft.Performance.SDK.Processing;
using Microsoft.Performance.Testing;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace Microsoft.Performance.SDK.Tests
{
    [TestClass]
    public class ProjectionTests
    {
        [TestMethod]
        [UnitTest]
        public void CacheThrowsWithNullArgument()
        {
            Assert.ThrowsException<ArgumentNullException>(
                () => Projection.CacheOnFirstUse<object>(1, null));
        }

        [TestMethod]
        [UnitTest]
        public void CacheThrowsWithNegativeArgument()
        {
            Assert.ThrowsException<ArgumentOutOfRangeException>(
                () => Projection.CacheOnFirstUse(-1, Projection.Identity<int>()));
        }

        [TestMethod]
        [UnitTest]
        public void CacheDoesNotInvokeProjectionOnCreation()
        {
            var invoked = false;
            object factory(int value)
            {
                invoked = true;
                return value;
            };

            var projection = Projection.CacheOnFirstUse(
                3,
                Projection.CreateUsingFuncAdaptor(factory));

            Assert.IsFalse(invoked);
        }

        [TestMethod]
        [UnitTest]
        public void CacheUsesCachedValuesAfterTheyAreComputed()
        {
            var computeCounts = new Dictionary<int, int>();
            object factory(int value)
            {
                if (!computeCounts.ContainsKey(value))
                {
                    computeCounts[value] = 0;
                }

                ++computeCounts[value];
                return computeCounts[value];
            }

            var projection = Projection.CacheOnFirstUse(3, Projection.CreateUsingFuncAdaptor(factory));

            Assert.AreEqual(1, projection[0]);
            Assert.AreEqual(1, projection[0]);
            Assert.AreEqual(1, projection[0]);

            Assert.AreEqual(1, projection[1]);
            Assert.AreEqual(1, projection[1]);
            Assert.AreEqual(1, projection[1]);

            Assert.AreEqual(1, projection[2]);
            Assert.AreEqual(1, projection[2]);
            Assert.AreEqual(1, projection[2]);
        }

        [TestMethod]
        [UnitTest]
        public void ComposeThrowsWithNullArguments()
        {
            Assert.ThrowsException<ArgumentNullException>(
                () => Projection.Compose<int, object, object>(null, i => i));

            Assert.ThrowsException<ArgumentNullException>(
                () => Projection.Compose(
                    Projection.Identity<int>(),
                    (Func<int, object>)null));

            Assert.ThrowsException<ArgumentNullException>(
                () => Projection.Compose(
                    Projection.Identity<int>(),
                    (IProjection<int, object>)null));
        }

        [TestMethod]
        [UnitTest]
        public void ComposeWithFuncComposesCorrectly()
        {
            var f = Projection.CreateUsingFuncAdaptor<int, DateTime>(i => DateTime.FromFileTime(i));
            var g = new Func<DateTime, string>(x => x.ToString());

            var argument = 23;
            var expected = g(f[argument]);

            var sut = f.Compose(g);
            var actual = sut[argument];

            Assert.AreEqual(expected, actual);
        }

        [TestMethod]
        [UnitTest]
        public void ComposeWithProjectionComposesCorrectly()
        {
            var f = Projection.CreateUsingFuncAdaptor<int, DateTime>(i => DateTime.FromFileTime(i));
            var g = Projection.CreateUsingFuncAdaptor<DateTime, string>(x => x.ToString());

            var argument = 23;
            var expected = g[f[argument]];

            var sut = f.Compose(g);
            var actual = sut[argument];

            Assert.AreEqual(expected, actual);
        }

        [TestMethod]
        [UnitTest]
        public void ConstantProjectionAlwaysReturnsTheSameValue()
        {
            var value = Guid.NewGuid();

            var projection = Projection.Constant(value);

            for (var i = 0; i < 100; ++i)
            {
                Assert.AreEqual(value, projection[i]);
            }
        }

        [TestMethod]
        [UnitTest]
        public void IsConstant_ReturnsTrueForConstantProject()
        {
            var projection = Projection.Constant(1);

            Assert.IsTrue(Projection.IsConstant(projection));
            Assert.IsTrue(projection.IsConstant());
        }

        [TestMethod]
        [UnitTest]
        public void IsConstant_ReturnsFalseForOtherProjections()
        {
            var projection = Projection.Identity<int>();

            Assert.IsFalse(Projection.IsConstant(projection));
            Assert.IsFalse(projection.IsConstant());
        }

        [TestMethod]
        [UnitTest]
        public void IsConstant_ReturnsFalseForNull()
        {
            Assert.IsFalse(Projection.IsConstant<int, int>(null));
        }

        [TestMethod]
        [UnitTest]
        public void CreateThrowsForNull()
        {
            Assert.ThrowsException<ArgumentNullException>(
                () => Projection.Create<object>((Func<int, object>)null));

            Assert.ThrowsException<ArgumentNullException>(
                () => Projection.Create<object, object>((Func<object, object>)null));
        }

        [TestMethod]
        [UnitTest]
        public void CreateRejectsNonPublicMethods()
        {
            // int, T

            Assert.ThrowsException<InvalidOperationException>(
                () => Projection.Create(MyPrivateMethodIntToObject));

            Assert.ThrowsException<InvalidOperationException>(
                () => Projection.Create(MyPrivateMethodIntToObjectStatic));

            Assert.ThrowsException<InvalidOperationException>(
                () => Projection.Create(MyProtectedMethodIntToObject));

            Assert.ThrowsException<InvalidOperationException>(
                () => Projection.Create(MyProtectedMethodIntToObjectStatic));

            Assert.ThrowsException<InvalidOperationException>(
                () => Projection.Create(MyInternalMethodIntToObject));

            Assert.ThrowsException<InvalidOperationException>(
                () => Projection.Create(MyInternalMethodIntToObjectStatic));

            // T1,T2

            Assert.ThrowsException<InvalidOperationException>(
                () => Projection.Create(MyGenericPrivateMethod<int, object>));

            Assert.ThrowsException<InvalidOperationException>(
                () => Projection.Create(MyGenericPrivateMethodStatic<int, object>));

            Assert.ThrowsException<InvalidOperationException>(
                () => Projection.Create(MyGenericProtectedMethod<int, object>));

            Assert.ThrowsException<InvalidOperationException>(
                () => Projection.Create(MyGenericProtectedMethodStatic<int, object>));

            Assert.ThrowsException<InvalidOperationException>(
                () => Projection.Create(MyGenericInternalMethod<int, object>));

            Assert.ThrowsException<InvalidOperationException>(
                () => Projection.Create(MyGenericInternalMethodStatic<int, object>));

            // non-public nested

            var internalInstance = new InternalNestedType();

            Assert.ThrowsException<InvalidOperationException>(
                () => Projection.Create(internalInstance.Method));
            Assert.ThrowsException<InvalidOperationException>(
                () => Projection.Create(InternalNestedType.MethodStatic));
            Assert.ThrowsException<InvalidOperationException>(
                () => Projection.Create(internalInstance.MethodGeneric<int, object>));
            Assert.ThrowsException<InvalidOperationException>(
                () => Projection.Create(InternalNestedType.MethodGenericStatic<int, object>));

            var protectedInstance = new ProtectedNestedType();

            Assert.ThrowsException<InvalidOperationException>(
                () => Projection.Create(protectedInstance.Method));
            Assert.ThrowsException<InvalidOperationException>(
                () => Projection.Create(ProtectedNestedType.MethodStatic));
            Assert.ThrowsException<InvalidOperationException>(
                () => Projection.Create(protectedInstance.MethodGeneric<int, object>));
            Assert.ThrowsException<InvalidOperationException>(
                () => Projection.Create(ProtectedNestedType.MethodGenericStatic<int, object>));

            var privateInstance = new PrivateNestedType();

            Assert.ThrowsException<InvalidOperationException>(
                () => Projection.Create(privateInstance.Method));
            Assert.ThrowsException<InvalidOperationException>(
                () => Projection.Create(PrivateNestedType.MethodStatic));
            Assert.ThrowsException<InvalidOperationException>(
                () => Projection.Create(privateInstance.MethodGeneric<int, object>));
            Assert.ThrowsException<InvalidOperationException>(
                () => Projection.Create(PrivateNestedType.MethodGenericStatic<int, object>));

            // anonymous

            Assert.ThrowsException<InvalidOperationException>(
                () => Projection.Create<int, object>(i => i));

            Assert.ThrowsException<InvalidOperationException>(
                () => Projection.Create<DateTime, string>(x => x.ToString()));
        }

        [TestMethod]
        [UnitTest]
        public void CreateCreatesFromPublicTypes()
        {
            var created = Projection.Create(MyPublicMethodIntToObject);
            Assert.AreEqual(23, created[23]);

            created = Projection.Create(MyPublicMethodIntToObjectStatic);
            Assert.AreEqual(23, created[23]);

            var createdGeneric = Projection.Create(MyGenericPublicMethod<int, MyGenericPublicMethodReturnValue<int>>);
            Assert.AreEqual(23, createdGeneric[23].Argument);

            createdGeneric = Projection.Create(MyGenericPublicMethodStatic<int, MyGenericPublicMethodReturnValue<int>>);
            Assert.AreEqual(23, createdGeneric[23].Argument);
        }

        [TestMethod]
        [UnitTest]
        public void CreateUsingFuncAdaptorCreates()
        {
            var data = Enumerable.Range(0, 10).ToDictionary(
                key => key,
                value => value.ToString());

            var anonympusFunction = new Func<int, string>(x => data[x]);
            string function(int value) => data[value];

            var sut = Projection.CreateUsingFuncAdaptor(anonympusFunction);

            foreach (var kvp in data)
            {
                Assert.AreEqual(kvp.Value, sut[kvp.Key]);
            }

            sut = Projection.CreateUsingFuncAdaptor(function);

            foreach (var kvp in data)
            {
                Assert.AreEqual(kvp.Value, sut[kvp.Key]);
            }
        }

        [TestMethod]
        [UnitTest]
        public void IdentityCreatesProjectionThatReturnsArgument()
        {
            var argument = new object();

            var projection = Projection.Identity<object>();

            Assert.AreEqual(argument, projection[argument]);
        }

        [TestMethod]
        [UnitTest]
        public void IndexThrowsOnNull()
        {
            Assert.ThrowsException<ArgumentNullException>(
                () => Projection.Index((IReadOnlyList<object>)null));
        }

        [TestMethod]
        [UnitTest]
        public void IndexCreatesIndexIntoTypeImplementingIReadOnlyList()
        {
            var list = Enumerable.Range(0, 10).Select(x => x + 10).ToList();

            var sut = Projection.Index(list);

            Assert.AreEqual(10, sut[0]);
            Assert.AreEqual(11, sut[1]);
            Assert.AreEqual(12, sut[2]);
            Assert.AreEqual(13, sut[3]);
            Assert.AreEqual(14, sut[4]);
            Assert.AreEqual(15, sut[5]);
            Assert.AreEqual(16, sut[6]);
            Assert.AreEqual(17, sut[7]);
            Assert.AreEqual(18, sut[8]);
            Assert.AreEqual(19, sut[9]);
        }

        [TestMethod]
        [UnitTest]
        public void IndexCreatesIndexIntoReadOnlyList()
        {
            var list = Enumerable.Range(0, 10).Select(x => x + 10).ToList().AsReadOnly();

            var sut = Projection.Index(list);

            Assert.AreEqual(10, sut[0]);
            Assert.AreEqual(11, sut[1]);
            Assert.AreEqual(12, sut[2]);
            Assert.AreEqual(13, sut[3]);
            Assert.AreEqual(14, sut[4]);
            Assert.AreEqual(15, sut[5]);
            Assert.AreEqual(16, sut[6]);
            Assert.AreEqual(17, sut[7]);
            Assert.AreEqual(18, sut[8]);
            Assert.AreEqual(19, sut[9]);
        }

        [TestMethod]
        [UnitTest]
        public void PrepopulatedCacheThrowsWithNullArgument()
        {
            Assert.ThrowsException<ArgumentNullException>(
                () => Projection.PrepopulatedCache<object>(1, null));
        }

        [TestMethod]
        [UnitTest]
        public void PrepopulatedCacheThrowsWithNegativeArgument()
        {
            Assert.ThrowsException<ArgumentOutOfRangeException>(
                () => Projection.PrepopulatedCache(-1, Projection.Identity<int>()));
        }

        [TestMethod]
        [UnitTest]
        public void PrepopulatedCacheInvokesUpfrontForEachValue()
        {
            var invokedWith = new List<int>();
            object factory(int value)
            {
                invokedWith.Add(value);
                return value;
            };

            var projection = Projection.PrepopulatedCache(
                3,
                Projection.CreateUsingFuncAdaptor(factory));

            Assert.AreEqual(3, invokedWith.Count);
            Assert.IsTrue(invokedWith.Contains(0));
            Assert.IsTrue(invokedWith.Contains(1));
            Assert.IsTrue(invokedWith.Contains(2));
        }

        [TestMethod]
        [UnitTest]
        public void PrepopulatedCacheUsesCachedValuesAfterTheyAreComputed()
        {
            var computeCounts = new Dictionary<int, int>();
            object factory(int value)
            {
                if (!computeCounts.ContainsKey(value))
                {
                    computeCounts[value] = 0;
                }

                ++computeCounts[value];
                return computeCounts[value];
            }

            var projection = Projection.PrepopulatedCache(3, Projection.CreateUsingFuncAdaptor(factory));

            Assert.AreEqual(1, projection[0]);
            Assert.AreEqual(1, projection[0]);
            Assert.AreEqual(1, projection[0]);

            Assert.AreEqual(1, projection[1]);
            Assert.AreEqual(1, projection[1]);
            Assert.AreEqual(1, projection[1]);

            Assert.AreEqual(1, projection[2]);
            Assert.AreEqual(1, projection[2]);
            Assert.AreEqual(1, projection[2]);
        }

        private static string Project_1_Projector(
              DateTime x)
        {
            return x.ToLongDateString();
        }

        [TestMethod]
        [UnitTest]
        public void Project_1_ThrowsForNull()
        {
            Assert.ThrowsException<ArgumentNullException>(
                () => Projection.Project<int, DateTime, string>(
                    null,
                    Project_1_Projector));

            Assert.ThrowsException<ArgumentNullException>(
                () => Projection.Project<int, DateTime, string>(
                    Projection.Constant(DateTime.UtcNow),
                    null));
        }

        [TestMethod]
        [UnitTest]
        public void Project_1_ThrowsIfNonStaticFuncGiven()
        {
            Assert.ThrowsException<InvalidOperationException>(
                () => Projection.Project<int, DateTime, string>(
                    Projection.Constant(DateTime.UtcNow),
                    x => x.ToString()));
        }

        [TestMethod]
        [UnitTest]
        public void Project_1_CombinesProperly()
        {
            var dates = Enumerable.Range(0, 100).Select(x => DateTime.FromFileTimeUtc(x)).ToArray();
            var argument = 23;
            var expected = Project_1_Projector(dates[argument]);

            var sut = Projection.Project<int, DateTime, string>(
                Projection.Index(dates),
                Project_1_Projector);

            Assert.AreEqual(expected, sut[argument]);
        }

        private static string Project_2_Projector(
              DateTime x,
              Guid y)
        {
            return x.ToLongDateString() + "_" + y.ToString();
        }

        [TestMethod]
        [UnitTest]
        public void Project_2_ThrowsForNull()
        {
            Assert.ThrowsException<ArgumentNullException>(
                () => Projection.Project<int, DateTime, Guid, string>(
                    null,
                    Projection.Constant(Guid.NewGuid()),
                    Project_2_Projector));

            Assert.ThrowsException<ArgumentNullException>(
                () => Projection.Project<int, DateTime, Guid, string>(
                    Projection.Constant(DateTime.UtcNow),
                    null,
                    Project_2_Projector));

            Assert.ThrowsException<ArgumentNullException>(
                () => Projection.Project<int, DateTime, Guid, string>(
                    Projection.Constant(DateTime.UtcNow),
                    Projection.Constant(Guid.NewGuid()),
                    null));
        }

        [TestMethod]
        [UnitTest]
        public void Project_2_ThrowsIfNonStaticFuncGiven()
        {
            Assert.ThrowsException<InvalidOperationException>(
                () => Projection.Project<int, DateTime, Guid, string>(
                    Projection.Constant(DateTime.UtcNow),
                    Projection.Constant(Guid.NewGuid()),
                    (x, y) => (x.ToString() + y.ToString())));
        }

        [TestMethod]
        [UnitTest]
        public void Project_2_CombinesProperly()
        {
            var dates = Enumerable.Range(0, 100).Select(x => DateTime.FromFileTimeUtc(x)).ToArray();
            var guids = Enumerable.Range(0, 100).Select(_ => Guid.NewGuid()).ToArray();
            var argument = 23;
            var expected = Project_2_Projector(dates[argument], guids[argument]);

            var sut = Projection.Project<int, DateTime, Guid, string>(
                Projection.Index(dates),
                Projection.Index(guids),
                Project_2_Projector);

            Assert.AreEqual(expected, sut[argument]);
        }

        private static string Project_3_Projector(
              DateTime x,
              Guid y,
              TimeSpan z)
        {
            return x.ToLongDateString() + "_" + y.ToString() + "_" + z.ToString();
        }

        [TestMethod]
        [UnitTest]
        public void Project_3_ThrowsForNull()
        {
            Assert.ThrowsException<ArgumentNullException>(
                () => Projection.Project<int, DateTime, Guid, TimeSpan, string>(
                    null,
                    Projection.Constant(Guid.NewGuid()),
                    Projection.Constant(TimeSpan.Zero),
                    Project_3_Projector));

            Assert.ThrowsException<ArgumentNullException>(
                () => Projection.Project<int, DateTime, Guid, TimeSpan, string>(
                    Projection.Constant(DateTime.UtcNow),
                    null,
                    Projection.Constant(TimeSpan.Zero),
                    Project_3_Projector));

            Assert.ThrowsException<ArgumentNullException>(
                () => Projection.Project<int, DateTime, Guid, TimeSpan, string>(
                    Projection.Constant(DateTime.UtcNow),
                    Projection.Constant(Guid.NewGuid()),
                    null,
                    Project_3_Projector));

            Assert.ThrowsException<ArgumentNullException>(
                () => Projection.Project<int, DateTime, Guid, TimeSpan, string>(
                    Projection.Constant(DateTime.UtcNow),
                    Projection.Constant(Guid.NewGuid()),
                    Projection.Constant(TimeSpan.Zero),
                    null));
        }

        [TestMethod]
        [UnitTest]
        public void Project_3_ThrowsIfNonStaticFuncGiven()
        {
            Assert.ThrowsException<InvalidOperationException>(
                () => Projection.Project<int, DateTime, Guid, TimeSpan, string>(
                    Projection.Constant(DateTime.UtcNow),
                    Projection.Constant(Guid.NewGuid()),
                    Projection.Constant(TimeSpan.Zero),
                    (x, y, z) => (x.ToString() + y.ToString() + z.ToString())));
        }

        [TestMethod]
        [UnitTest]
        public void Project_3_CombinesProperly()
        {
            var dates = Enumerable.Range(0, 100).Select(x => DateTime.FromFileTimeUtc(x)).ToArray();
            var guids = Enumerable.Range(0, 100).Select(_ => Guid.NewGuid()).ToArray();
            var timespans = Enumerable.Range(0, 100).Select(x => TimeSpan.FromTicks(x)).ToArray();
            var argument = 23;
            var expected = Project_3_Projector(dates[argument], guids[argument], timespans[argument]);

            var sut = Projection.Project<int, DateTime, Guid, TimeSpan, string>(
                Projection.Index(dates),
                Projection.Index(guids),
                Projection.Index(timespans),
                Project_3_Projector);

            Assert.AreEqual(expected, sut[argument]);
        }

        private static string Project_4_Projector(
              DateTime x,
              Guid y,
              TimeSpan z,
              DateTimeOffset a)
        {
            return x.ToLongDateString() + "_" + y.ToString() + "_" + z.ToString() + "_" + a.ToString();
        }

        [TestMethod]
        [UnitTest]
        public void Project_4_ThrowsForNull()
        {
            Assert.ThrowsException<ArgumentNullException>(
                () => Projection.Project<int, DateTime, Guid, TimeSpan, DateTimeOffset, string>(
                    null,
                    Projection.Constant(Guid.NewGuid()),
                    Projection.Constant(TimeSpan.Zero),
                    Projection.Constant(DateTimeOffset.MaxValue),
                    Project_4_Projector));

            Assert.ThrowsException<ArgumentNullException>(
                () => Projection.Project<int, DateTime, Guid, TimeSpan, DateTimeOffset, string>(
                    Projection.Constant(DateTime.UtcNow),
                    null,
                    Projection.Constant(TimeSpan.Zero),
                    Projection.Constant(DateTimeOffset.MaxValue),
                    Project_4_Projector));

            Assert.ThrowsException<ArgumentNullException>(
                () => Projection.Project<int, DateTime, Guid, TimeSpan, DateTimeOffset, string>(
                    Projection.Constant(DateTime.UtcNow),
                    Projection.Constant(Guid.NewGuid()),
                    null,
                    Projection.Constant(DateTimeOffset.MaxValue),
                    Project_4_Projector));

            Assert.ThrowsException<ArgumentNullException>(
                () => Projection.Project<int, DateTime, Guid, TimeSpan, DateTimeOffset, string>(
                    Projection.Constant(DateTime.UtcNow),
                    Projection.Constant(Guid.NewGuid()),
                    Projection.Constant(TimeSpan.Zero),
                    null,
                    Project_4_Projector));

            Assert.ThrowsException<ArgumentNullException>(
                () => Projection.Project<int, DateTime, Guid, TimeSpan, DateTimeOffset, string>(
                    Projection.Constant(DateTime.UtcNow),
                    Projection.Constant(Guid.NewGuid()),
                    Projection.Constant(TimeSpan.Zero),
                    Projection.Constant(DateTimeOffset.MaxValue),
                    null));
        }

        [TestMethod]
        [UnitTest]
        public void Project_4_ThrowsIfNonStaticFuncGiven()
        {
            Assert.ThrowsException<InvalidOperationException>(
                () => Projection.Project<int, DateTime, Guid, TimeSpan, DateTimeOffset, string>(
                    Projection.Constant(DateTime.UtcNow),
                    Projection.Constant(Guid.NewGuid()),
                    Projection.Constant(TimeSpan.Zero),
                    Projection.Constant(DateTimeOffset.MaxValue),
                    (x, y, z, a) => (x.ToString() + y.ToString() + z.ToString() + a.ToString())));
        }

        [TestMethod]
        [UnitTest]
        public void Project_4_CombinesProperly()
        {
            var dates = Enumerable.Range(0, 100).Select(x => DateTime.FromFileTimeUtc(x)).ToArray();
            var guids = Enumerable.Range(0, 100).Select(_ => Guid.NewGuid()).ToArray();
            var timespans = Enumerable.Range(0, 100).Select(x => TimeSpan.FromTicks(x)).ToArray();
            var offsets = Enumerable.Range(0, 100).Select(x => DateTimeOffset.FromUnixTimeSeconds(x)).ToArray();
            var argument = 23;
            var expected = Project_4_Projector(dates[argument], guids[argument], timespans[argument], offsets[argument]);

            var sut = Projection.Project<int, DateTime, Guid, TimeSpan, DateTimeOffset, string>(
                Projection.Index(dates),
                Projection.Index(guids),
                Projection.Index(timespans),
                Projection.Index(offsets),
                Project_4_Projector);

            Assert.AreEqual(expected, sut[argument]);
        }

        public object MyPublicMethodIntToObject(int intArgument)
        {
            return intArgument;
        }

        public static object MyPublicMethodIntToObjectStatic(int intArgument)
        {
            return intArgument;
        }

        public sealed class MyGenericPublicMethodReturnValue<T1>
        {
            public T1 Argument { get; set; }
        }

        public T2 MyGenericPublicMethod<T1, T2>(T1 intArgument)
            where T2 : class
        {
            return new MyGenericPublicMethodReturnValue<T1>
            {
                Argument = intArgument
            } as T2;
        }

        public static T2 MyGenericPublicMethodStatic<T1, T2>(T1 intArgument)
            where T2 : class
        {
            return new MyGenericPublicMethodReturnValue<T1>
            {
                Argument = intArgument
            } as T2;
        }

        private static object MyPrivateMethodIntToObjectStatic(int intArgument)
        {
            return intArgument;
        }

        private object MyPrivateMethodIntToObject(int intArgument)
        {
            return intArgument;
        }

        protected static object MyProtectedMethodIntToObjectStatic(int intArgument)
        {
            return intArgument;
        }

        protected object MyProtectedMethodIntToObject(int intArgument)
        {
            return intArgument;
        }

        internal static object MyInternalMethodIntToObjectStatic(int intArgument)
        {
            return intArgument;
        }

        internal object MyInternalMethodIntToObject(int intArgument)
        {
            return intArgument;
        }

        private static T2 MyGenericPrivateMethodStatic<T1, T2>(T1 argument)
        {
            return Activator.CreateInstance<T2>();
        }

        private T2 MyGenericPrivateMethod<T1, T2>(T1 argument)
        {
            return Activator.CreateInstance<T2>();
        }

        protected static T2 MyGenericProtectedMethodStatic<T1, T2>(T1 argument)
        {
            return Activator.CreateInstance<T2>();
        }

        protected T2 MyGenericProtectedMethod<T1, T2>(T1 argument)
        {
            return Activator.CreateInstance<T2>();
        }

        internal static T2 MyGenericInternalMethodStatic<T1, T2>(T1 argument)
        {
            return Activator.CreateInstance<T2>();
        }

        internal T2 MyGenericInternalMethod<T1, T2>(T1 argument)
        {
            return Activator.CreateInstance<T2>();
        }

        public sealed class PublicNestedType
        {
            public object Method(int value)
            {
                return value;
            }

            public static object MethodStatic(int value)
            {
                return value;
            }

            public T2 MethodGeneric<T1, T2>(T1 value)
            {
                return Activator.CreateInstance<T2>();
            }

            public static T2 MethodGenericStatic<T1, T2>(T1 value)
            {
                return Activator.CreateInstance<T2>();
            }
        }

        internal sealed class InternalNestedType
        {
            public object Method(int value)
            {
                return value;
            }

            public static object MethodStatic(int value)
            {
                return value;
            }

            public T2 MethodGeneric<T1, T2>(T1 value)
            {
                return Activator.CreateInstance<T2>();
            }

            public static T2 MethodGenericStatic<T1, T2>(T1 value)
            {
                return Activator.CreateInstance<T2>();
            }
        }

        protected sealed class ProtectedNestedType
        {
            public object Method(int value)
            {
                return value;
            }

            public static object MethodStatic(int value)
            {
                return value;
            }

            public T2 MethodGeneric<T1, T2>(T1 value)
            {
                return Activator.CreateInstance<T2>();
            }

            public static T2 MethodGenericStatic<T1, T2>(T1 value)
            {
                return Activator.CreateInstance<T2>();
            }
        }

        private sealed class PrivateNestedType
        {
            public object Method(int value)
            {
                return value;
            }

            public static object MethodStatic(int value)
            {
                return value;
            }

            public T2 MethodGeneric<T1, T2>(T1 value)
            {
                return Activator.CreateInstance<T2>();
            }

            public static T2 MethodGenericStatic<T1, T2>(T1 value)
            {
                return Activator.CreateInstance<T2>();
            }
        }
    }
}
