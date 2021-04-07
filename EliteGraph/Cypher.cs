using System;
using System.Collections.Generic;
using System.Text;
using System.IO;

namespace EliteGraph
{
    public static class Cypher
    {
        public static void SaveAsCypher(this Graph g, string filename)
        {
            // readable keys help debug the cypher output file, but are much slower to import in neo4j
            bool readable_keys = false;

            using (var stream = new StreamWriter(filename, false))
            {
                stream.WriteLine("// Before importing data first run this neo4j command");
                stream.WriteLine("MATCH (n) DETACH DELETE n");
                stream.WriteLine("// Then copy all the following lines into neo4j and run them");

                foreach (var entry in g.Nodes)
                {
                    //var key = entry.Key;
                    var key = entry.Key;
                    var node = entry.Value;
                    bool write_comma = false;

                     // only output nodes used in edges
                    if (node.IsUsed)
                    {
                        if (readable_keys)
                        {
                            stream.Write("CREATE ({0}:{1}", key, node.Kind);
                        }
                        else
                        {
                            stream.Write("CREATE (n{0}:{1}", node.ID, node.Kind);
                        }

                        if (node.Data.Count > 0)
                        {
                            stream.Write("{");
                        }

                        foreach (var data in node.Data)
                        {
                            if (write_comma) stream.Write(",");

                            stream.Write("{0}:\"{1}\"", data.Key, data.Value);
                            write_comma = true;
                        }

                        if (node.Data.Count > 0)
                        {
                            stream.Write("}");
                        }

                        stream.WriteLine(")");
                    }
                    else
                    {
                        System.Diagnostics.Trace.WriteLine(String.Format("Node '{0}' is not used yet", node.Key));
                    }
                }

                foreach (var edge in g.Edges)
                {
                    if (readable_keys)
                    {
                        stream.WriteLine("CREATE (n{0})-[:{1}]->(n{2})", edge.From.Key, edge.Kind, edge.To.Key);
                    }
                    else
                    {
                        stream.WriteLine("CREATE (n{0})-[:{1}]->(n{2})", edge.From.ID, edge.Kind, edge.To.ID);
                    }
                }
            }
        }
    }
}
