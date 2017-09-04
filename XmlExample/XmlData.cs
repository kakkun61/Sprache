using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

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
}
