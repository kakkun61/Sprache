extern alias sprache_current;
extern alias sprache_c2cf535;
using System.Collections.Generic;
using System.Linq;
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
    public class Document
    {
        public Node Root;

        public override string ToString()
        {
            return Root.ToString();
        }
    }

    public class Item { }

    public class Content : Item
    {
        public string Text;

        public override string ToString()
        {
            return Text;
        }
    }

    public class Node : Item
    {
        public string Name;
        public IEnumerable<Item> Children;

        public override string ToString()
        {
            if (Children != null)
                return string.Format("<{0}>", Name) +
                    Children.Aggregate("", (s, c) => s + c) +
                    string.Format("</{0}>", Name);
            return string.Format("<{0}/>", Name);
        }
    }

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
            return SpracheCurrent.ParserExtensions.Parse(XmlParserCurrent.Document, input);
        }
    }

    class Program
    {
        static void Main()
        {
            var switcher = new BenchmarkSwitcher(new[] { typeof(XmlParserBenchmark) });

            switcher.Run(new[] { "0" });
        }
    }
}
