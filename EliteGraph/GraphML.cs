using System;
using System.Collections.Generic;
using System.Text;
using System.IO;

namespace EliteGraph
{
    public static class GraphML
    {
        public static void Save(this Graph g, string filename)
        {
            using (var stream = new StreamWriter(filename, false))
            {
                stream.WriteLine("<?xml version=\"1.0\" encoding=\"UTF-8\"?>");
                stream.WriteLine("<graphml xmlns = \"http://graphml.graphdrawing.org/xmlns\" xmlns:xsi=\"http://www.w3.org/2001/XMLSchema-instance\" xsi:schemaLocation=\"http://graphml.graphdrawing.org/xmlns http://graphml.graphdrawing.org/xmlns/1.0/graphml.xsd\">");

                foreach (var entry in g.NodeData)
                {
                    stream.WriteLine("<key id=\"{0}\" for=\"node\" attr.name=\"{0}\" attr.type=\"{1}\"/>", entry.Key, entry.Value);
                }

                stream.WriteLine("<graph id=\"data\" edgedefault=\"directed\">");

                foreach (var entry in g.Nodes)
                {
                    var key = entry.Key;
                    var Node = entry.Value;
                    stream.WriteLine("  <node id=\"{0}\" labels=\"{1}\">", key, Node.Kind);

                    foreach (var data in Node.Data)
                    {
                        stream.WriteLine("    <data key=\"{0}\">{1}</data>", data.Key, data.Value);
                    }

                    stream.WriteLine("  </node>");
                }

                stream.WriteLine("</graph>");
                stream.WriteLine("</graphml>");
            }

        }
    }
}
