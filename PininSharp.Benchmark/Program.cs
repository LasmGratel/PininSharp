using BenchmarkDotNet.Running;
using System;
using BenchmarkDotNet.Configs;

namespace PininSharp.Benchmark
{
    internal class Program
    {
        static void Main(string[] args)
        {
            var summary = BenchmarkRunner.Run<PinInBenchmark>();
        }
    }
}
