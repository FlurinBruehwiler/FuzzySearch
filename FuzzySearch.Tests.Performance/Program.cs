using BenchmarkDotNet.Attributes;
using BenchmarkDotNet.Running;
using Bogus;
using FuzzySearch.Tests.Performance;

BenchmarkRunner.Run<FuzzySearchBenchmarks>();

namespace FuzzySearch.Tests.Performance
{
    [MemoryDiagnoser]
    public class FuzzySearchBenchmarks
    {
        private V1.FuzzySearcher<string> _v1Small = null!;
        private V1.FuzzySearcher<string> _v1Middle = null!;
        private V1.FuzzySearcher<string> _v1Large = null!;
        
        private V2.FuzzySearcher<string> _v2Small = null!;
        private V2.FuzzySearcher<string> _v2Middle = null!;
        private V2.FuzzySearcher<string> _v2Large = null!;
        
        private V3.FuzzySearcher<string> _v3Small = null!;
        private V3.FuzzySearcher<string> _v3Middle = null!;
        private V3.FuzzySearcher<string> _v3Large = null!;
        
        [Params("a", "iure", "fjlkadsruieon")]
        public string Pattern;

        [GlobalSetup]
        public void Setup()
        {
            var faker = new Faker();

            _v1Small = new V1.FuzzySearcher<string>(faker.Random.WordsArray(100));
            _v1Large = new V1.FuzzySearcher<string>(faker.Random.WordsArray(10000));
            
            _v2Small = new V2.FuzzySearcher<string>(faker.Random.WordsArray(100));
            _v2Large = new V2.FuzzySearcher<string>(faker.Random.WordsArray(10000));
            
            _v3Small = new V3.FuzzySearcher<string>(faker.Random.WordsArray(100));
            _v3Large = new V3.FuzzySearcher<string>(faker.Random.WordsArray(10000));
        }
        
        //v1
        [Benchmark]
        public void V1_Small()
        {
            var _ = _v1Small.Search(Pattern);
        }
        //
        // [Benchmark]
        // public void V1_Middle()
        // {
        //     var _ = _v1Middle.Search(Pattern);
        // }
        
        [Benchmark]
        public void V1_Large()
        {
            var _ = _v1Large.Search(Pattern);
        }
        
        //v2
        [Benchmark]
        public void V2_Small()
        {
            var _ = _v2Small.Search(Pattern);
        }
        
        // [Benchmark]
        // public void V2_Middle()
        // {
        //     var _ = _v2Middle.Search(Pattern);
        // }
        
        [Benchmark]
        public void V2_Large()
        {
            var _ = _v2Large.Search(Pattern);
        }
        
        //v3
        [Benchmark]
        public void V3_Small()
        {
            var _ = _v3Small.Search(Pattern);
        }
        
        // [Benchmark]
        // public void V3_Middle()
        // {
        //     var _ = _v3Middle.Search(Pattern);
        // }
        
        [Benchmark]
        public void V3_Large()
        {
            var _ = _v3Large.Search(Pattern);
        }
    }
}