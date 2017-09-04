extern alias sprache_current;

using System;
using System.Text;
using sprache_current::Sprache;
using System.IO;

namespace XmlExample
{
    class Program
    {
        static void Main()
        {
            string input = File.ReadAllText("TestFile.xml", Encoding.UTF8);
            var parsed = XmlParserCurrent.Document.Parse(input);
            Console.WriteLine(parsed);
        }
    }
}
