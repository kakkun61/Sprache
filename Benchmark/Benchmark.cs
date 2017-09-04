extern alias sprache_current;
extern alias sprache_c2cf535;

using System.Text;
using System.IO;
using BenchmarkDotNet.Attributes;
using BenchmarkDotNet.Configs;
using BenchmarkDotNet.Diagnosers;
using BenchmarkDotNet.Exporters;
using BenchmarkDotNet.Jobs;
using BenchmarkDotNet.Running;
using SpracheCurrent = sprache_current::Sprache;
using SpracheC2cf535 = sprache_c2cf535::Sprache;

namespace XmlExample
{
    class BenchmarkConfig : ManualConfig
    {
        public BenchmarkConfig()
        {
            Add(MarkdownExporter.GitHub);
            Add(MemoryDiagnoser.Default);

            Add(Job.ShortRun);
        }
    }

    [Config(typeof(BenchmarkConfig))]
    public class XmlParserBenchmark
    {
        string input;

        [GlobalSetup]
        public void Setup()
        {
            input = File.ReadAllText("TestFile.xml", Encoding.UTF8);
        }

        [Benchmark(Baseline = true)]
        public object C2cf535()
        {
            return SpracheC2cf535.ParserExtensions.Parse(XmlParserC2cf535.Document, input);
        }

        [Benchmark]
        public object Current()
        {
            return SpracheCurrent.ParserExtensions.Parse(XmlParser.Document, input);
        }
    }

    class Benchmark
    {
        static void Main()
        {
            var switcher = new BenchmarkSwitcher(new[] { typeof(XmlParserBenchmark) });

            switcher.Run(new[] { "0" });
        }
    }
}
