using FlurinBruehwiler.Helpers;
using FuzzySearch.Tests.Performance.V3;

// BenchmarkRunner.Run<FuzzySearchBenchmarks>();

var sut = new FuzzySearcher<string>(new[] { "Firefox", "Chrome", "Spotify" });

var result = sut.Search("fi");

result.Dump();
