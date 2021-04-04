using System;
using System.Collections.Generic;
using System.Text;
using System.Xml;

namespace EliteGraph
{
    public static class XmlUtils
    {
        public static string Attr(this XmlNode node, string name)
        {
            var attribute = node.Attributes[name];
            string retval = (attribute == null) ? null : attribute.Value;
            return retval;
        }

        public static string Key(this XmlNode node)
        {
            return node.Attr("key");
        }

    }
}
