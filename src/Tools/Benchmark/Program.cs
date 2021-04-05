// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.

namespace BenchmarkProjections
{
    class Program
    {
        static void Main(string[] args)
        {
            BenchmarkDotNet.Running.BenchmarkRunner.Run<ProjectionBenchmark>();
        }
    }
}
