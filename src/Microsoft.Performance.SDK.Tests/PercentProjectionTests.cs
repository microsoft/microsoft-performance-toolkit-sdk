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
    public class PercentProjectionTests
    {
        private const int dataLength = 500;
        private const int maxValue = 500;

        [TestMethod]
        [UnitTest]
        public void DoublesProjectionTest()
        {
            var (numeratorsInt, denominatorsInt) = GenerateRandomNumbers(42);
            var numerators = numeratorsInt.Select(n => (double)n).ToList();
            var denominators = denominatorsInt.Select(d => (double)d).ToList();

            var projection = Projection.Percent.Create(Projection.Index(numerators), Projection.Index(denominators));

            double identitySelector(double value) => value;
            var expected = GenerateExpectedValues(numerators, denominators, identitySelector, identitySelector);
            AssertExpectedProjectionValues(expected, projection);
        }

        [TestMethod]
        [UnitTest]
        public void TimestampDeltaProjectionTest()
        {
            var (numeratorsInt, denominatorsInt) = GenerateRandomNumbers(43);
            var numerators = numeratorsInt.Select(n => new TimestampDelta(n)).ToList();
            var denominators = denominatorsInt.Select(d => new TimestampDelta(d)).ToList();

            var projection = Projection.Percent.Create(Projection.Index(numerators), Projection.Index(denominators));

            double toNanosecondsSelector(TimestampDelta value) => value.ToNanoseconds;
            var expected = GenerateExpectedValues(numerators, denominators, toNanosecondsSelector, toNanosecondsSelector);
            AssertExpectedProjectionValues(expected, projection);
        }

        [TestMethod]
        [UnitTest]
        public void UlongProjectionTest()
        {
            var (numeratorsInt, denominatorsInt) = GenerateRandomNumbers(44);
            var numerators = numeratorsInt.Select(n => (ulong)n).ToList();
            var denominators = denominatorsInt.Select(d => (ulong)d).ToList();

            var projection = Projection.Percent.Create(Projection.Index(numerators), Projection.Index(denominators));

            var expected = GenerateExpectedValues(numerators, denominators, (val) => val, (val) => val);
            AssertExpectedProjectionValues(expected, projection);
        }

        [TestMethod]
        [UnitTest]
        public void IntUlongProjectionTest()
        {
            var (numerators, denominatorsInt) = GenerateRandomNumbers(45);
            var denominators = denominatorsInt.Select(d => (ulong)d).ToList();

            var projection = Projection.Percent.Create(Projection.Index(numerators), Projection.Index(denominators));

            var expected = GenerateExpectedValues(numerators, denominators, (val) => val, (val) => val);
            AssertExpectedProjectionValues(expected, projection);
        }

        private static void AssertExpectedProjectionValues(List<double> expected, IProjection<int, double> projection)
        {
            for (int i = 0; i < expected.Count; i++)
            {
                Assert.IsTrue(Math.Abs(expected[i] - projection[i]) < double.Epsilon);
            }
        }

        private static (List<int> first, List<int> second) GenerateRandomNumbers(int seed)
        {
            var random = new Random(seed);

            var first = new List<int>();
            var second = new List<int>();

            for (int i = 0; i < dataLength; i++)
            {
                first.Add(random.Next(maxValue));
                second.Add(random.Next(maxValue));
            }

            return (first, second);
        }

        private static List<double> GenerateExpectedValues<T, G>(List<T> numerators, List<G> denominators, Func<T, double> nToDouble, Func<G, double> dToDouble)
        {
            var expected = new List<double>();
            for (int i = 0; i < numerators.Count; i++)
            {
                var n = nToDouble(numerators[i]);
                var d = dToDouble(denominators[i]);

                double toAdd;
                if (d > 0.0)
                {
                    toAdd = n / d * 100.0;
                }
                else
                {
                    toAdd = 0.0;
                }

                expected.Add(toAdd);
            }

            return expected;
        }
    }
}
