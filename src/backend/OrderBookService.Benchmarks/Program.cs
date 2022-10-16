// See https://aka.ms/new-console-template for more information

using BenchmarkDotNet.Reports;
using BenchmarkDotNet.Running;
using OrderBookService.Benchmarks;

Summary? summary = BenchmarkRunner.Run<GetPriceBenchmark>();

