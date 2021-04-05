// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.

using BenchmarkDotNet.Attributes;
using Microsoft.Performance.SDK.Processing;
using System;
using System.Collections.Generic;

namespace BenchmarkProjections
{
    [MemoryDiagnoser]
    public class ProjectionBenchmark
    {
        [Params(100, 100000, 100000000)]
        public int Size { get; set; }

        private static Random rand = new Random();

        private IProjection<int, int> intProj;
        private IProjection<int, ClassWithProperty> classProj;
        private IProjection<int, ReadOnlyStructWithProperty> structProj;

        [GlobalSetup]
        public void Setup()
        {
            intProj = GenerateProjection<int>(Size);
            classProj = GenerateProjection(Size, () => new ClassWithProperty(rand.Next()));
            structProj = GenerateProjection(Size, () => new ReadOnlyStructWithProperty(rand.Next()));
        }

        #region Int

        [Benchmark]
        public void Compose()
        {
            var projection = intProj;

            var test = projection.Compose(x => x);

            LoopExecuter(test, Size);
        }

        [Benchmark]
        public void Func()
        {            
            var projection = intProj;

            var test = Projection.Select(projection, Func);

            LoopExecuter(test, Size);
        }
       
        public static int Func(int index, int value)
        {
            return value;
        }

        #endregion Simple

        #region Class

        [Benchmark]
        public void ComposeClass()
        {
            var projection = classProj;

            var test = projection.Compose(x => x.Value);

            LoopExecuter(test, Size);
        }

        [Benchmark]
        public void FuncClass()
        {
            var projection = classProj;

            var test = Projection.Select(projection, FuncIReturnInt);

            LoopExecuter(test, Size);
        }

        #endregion Class

        #region Struct

        [Benchmark]
        public void ComposeStruct()
        {
            var projection = structProj;

            var test = projection.Compose(x => x.Value);

            LoopExecuter(test, Size);
        }

        [Benchmark]
        public void FuncStruct()
        {
            var projection = structProj;

            var test = Projection.Select(projection, FuncIReturnInt);

            LoopExecuter(test, Size);
        }

        #endregion Struct

        #region Helpers

        public static int FuncIReturnInt<T>(int index, T value)
            where T : IReturnInt
        {
            return value.Value;
        }

        public interface IReturnInt
        {
            int Value { get; }
        }

        public class ClassWithProperty
            : IReturnInt
        {
            public ClassWithProperty(int value)
            {
                this.Value = value;
            }

            public int Value { get; }
        }

        public readonly struct ReadOnlyStructWithProperty
            : IReturnInt
        {
            public ReadOnlyStructWithProperty(int value)
            {
                this.Value = value;
            }

            public int Value { get; }
        }

        private static IProjection<int, T> GenerateProjection<T>(int size, Func<T> factory = null)
        {
            return Projection.Index(CreateList<T>(size, factory));
        }

        private static List<T> CreateList<T>(int size, Func<T> factory = null)
        {
            var list = new List<T>(size);

            for (int i = 0; i < size; ++i)
            {
                list.Add((factory != null) ? factory() : default);
            }

            return list;
        }

        private void LoopExecuter<T>(IProjection<int, T> projection, int length)
        {
            for (int i = 0; i < length; ++i)
            {
                var value = projection[i];
            }
        }

        #endregion Helpers
    }
}
