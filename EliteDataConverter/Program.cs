using System;
using EliteGraph;

namespace EliteDataConverter
{
    class Program
    {
        static void Main(string[] args)
        {
            var g = new EliteGraph.Graph();
            g.Load(@"D:\Data\projects\EliteDataConverter\EliteGraph\Data");
            g.SaveAsCypher(@"D:\Data\projects\EliteDataConverter\EliteGraph\Data\EDData.cypher");
        }
    }
}

