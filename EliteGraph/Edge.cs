using System;
using System.Collections.Generic;
using System.Text;

namespace EliteGraph
{
    public class Edge
    {
        public Node From {  get; private set; }
        public Node To { get; private set; }
        public String Kind {  get; private set; }

        public Edge(Node from, string kind, Node to)
        { 
            From = from;
            From.IsUsed = true;
            To = to;
            To.IsUsed = true;
            Kind = kind;
        }
    }
}
