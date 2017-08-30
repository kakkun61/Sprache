extern alias sprache_current;
extern alias sprache_c2cf535;

using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.IO;
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

    class Program
    {
        static void Main()
        {
            MeasurePerformance("c2cf535", () =>
            {
                string input = File.ReadAllText("TestFile.xml", Encoding.UTF8);
                var parsed = SpracheC2cf535.ParserExtensions.Parse(XmlParserC2cf535.Document, input);
                Console.WriteLine(parsed);
            });
            MeasurePerformance("current", () =>
            {
                string input = File.ReadAllText("TestFile.xml", Encoding.UTF8);
                var parsed = SpracheCurrent.ParserExtensions.Parse(XmlParserCurrent.Document, input);
                Console.WriteLine(parsed);
            });
        }

        static void MeasurePerformance(string tag, Action action)
        {
            var n = 10;
            var m = 10;
            var elapsedTimeSpans = new double[n];
            var usedMemories = new long[n];
            var currentProcess = Process.GetCurrentProcess();

            for (int i = 0; i < n; i++)
            {
                var stopwatch = new Stopwatch();
                stopwatch.Start();

                for (int j = 0; j < m; j++)
                {
                    action();
                }

                stopwatch.Stop();
                elapsedTimeSpans[i] = stopwatch.Elapsed.TotalMilliseconds;
                usedMemories[i] = currentProcess.WorkingSet64;

                GC.Collect();
                GC.WaitForPendingFinalizers();
                GC.Collect();
            }

            {
                Console.WriteLine("{0}", tag);
                Console.WriteLine("mean time [ms]: {0}", TimeSpan.FromMilliseconds(elapsedTimeSpans.Average()));
                Console.WriteLine("max time [ms]: {0}", TimeSpan.FromMilliseconds(elapsedTimeSpans.Max()));
                Console.WriteLine("min time [ms]: {0}", TimeSpan.FromMilliseconds(elapsedTimeSpans.Min()));
                Console.WriteLine("mean memory [B]: {0}", usedMemories.Average());
                Console.WriteLine("max memory [B]: {0}", usedMemories.Max());
                Console.WriteLine("min memory [B]: {0}", usedMemories.Min());
            }
        }
    }
}
