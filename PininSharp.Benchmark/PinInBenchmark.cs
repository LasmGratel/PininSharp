using BenchmarkDotNet.Attributes;
using PininSharp.Searchers;
using System;
using System.Collections.Generic;
using System.IO;
using System.IO.Compression;
using System.Linq;
using System.Reflection;
using System.Text;

namespace PininSharp.Benchmark
{
    public class SearcherBuilder
    {
        public readonly Func<ISearcher<int>> Build;
        private readonly string _name;
        public SearcherBuilder(string name, Func<ISearcher<int>> func)
        {
            _name = name;
            Build = func;
        }

        public override string ToString()
        {
            return _name;
        }
    }

    [SimpleJob(launchCount: 1, warmupCount: 3, targetCount: 10)]
    public class PinInBenchmark
    {
        private List<string> _small;
        private List<string> _large;
        private List<string> _searchWords = new List<string>
        {
            "boli", "yangmao", "hongse"
        };

        public IEnumerable<SearcherBuilder> SearcherBuilders()
        {
            yield return new SearcherBuilder("TreeBegin", () => new TreeSearcher<int>(SearcherLogic.Begin, new PinIn()));
            yield return new SearcherBuilder("TreeContain", () => new TreeSearcher<int>(SearcherLogic.Contain, new PinIn()));
            yield return new SearcherBuilder("SimpleBegin", () => new SimpleSearcher<int>(SearcherLogic.Begin, new PinIn()));
            yield return new SearcherBuilder("SimpleContain", () => new SimpleSearcher<int>(SearcherLogic.Contain, new PinIn()));
            yield return new SearcherBuilder("CachedBegin", () => new CachedSearcher<int>(SearcherLogic.Begin, new PinIn()));
            yield return new SearcherBuilder("CachedContain", () => new CachedSearcher<int>(SearcherLogic.Contain, new PinIn()));
        }

        public PinInBenchmark()
        {
            _small = LoadFile("PininSharp.Benchmark.small.gz").ToList();
            _large = LoadFile("PininSharp.Benchmark.large.gz").ToList();
        }

        [Benchmark]
        [ArgumentsSource(nameof(SearcherBuilders))]
        public void BenchSmallBuild(SearcherBuilder searcherFunc)
        {
            var searcher = searcherFunc.Build();
            for (var i = 0; i < _small.Count; i++)
                searcher.Put(_small[i], i);
        }
        /*
        [Benchmark]
        [ArgumentsSource(nameof(SearcherBuilders))]
        public void BenchLargeBuild(SearcherBuilder searcherFunc)
        {
            var searcher = searcherFunc.Build();
            for (var i = 0; i < _large.Count; i++)
                searcher.Put(_large[i], i);
        }*/

        public ISearcher<int> Setup(ISearcher<int> searcher)
        {
            for (var i = 0; i < _small.Count; i++)
                searcher.Put(_small[i], i);
            return searcher;
        }

        public IEnumerable<ISearcher<int>> Searchers()
        {
            return SearcherBuilders().Select(builder => Setup(builder.Build()));
        }

        [Benchmark]
        [ArgumentsSource(nameof(Searchers))]
        public void BenchSearch(ISearcher<int> searcher)
        {
            foreach (var s in _searchWords)
            {
                searcher.Search(s);
            }
        }

        public IEnumerable<string> LoadFile(string resourceName)
        {
            var assembly = typeof(PinInBenchmark).GetTypeInfo().Assembly;

            using var stream = assembly.GetManifestResourceStream(resourceName);
            using var reader = new StreamReader(new GZipStream(stream, CompressionMode.Decompress));
            string line;
            while ((line = reader.ReadLine()) != null)
            {
                yield return line;
            }
        }
    }
}
