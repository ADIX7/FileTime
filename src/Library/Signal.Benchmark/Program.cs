using BenchmarkDotNet.Running;
using Signal.Benchmark;

var summary = BenchmarkRunner.Run<MapBenchmark>();