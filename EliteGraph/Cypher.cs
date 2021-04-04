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
                // CALL apoc.cypher.runFile("EDData.cypher",{statistics: false})
                //stream.WriteLine("// Run this script using CALL apoc.cypher.runFile(\"EDData.cypher\")");
                //stream.WriteLine("// for now clean the database first");
                stream.WriteLine("MATCH (n) DETACH DELETE n");
                //stream.WriteLine();
                //stream.WriteLine("// Add all nodes from the graph");

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
                }

                //stream.WriteLine("// edges");

                foreach (var edge in g.Edges)
                {
                    if (readable_keys)
                    {
                        stream.WriteLine("CREATE ({0})-[:{1}]->({2})", edge.From.Key, edge.Kind, edge.To.Key);
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
