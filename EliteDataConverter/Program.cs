using System;
using EliteGraph;

namespace EliteDataConverter
{
    class Program
    {
        static void Main(string[] args)
        {
            var g = new EliteGraph.Graph();
            g.LoadEDData(@"D:\Data\EDGraph\EDData.xml");
            g.SaveAsCypher(@"D:\Data\Neo4j\relate-data\dbmss\dbms-023f0fa0-71f0-4541-a9e7-c69aed59ef87\import\EDData.cypher");
        }
    }
}

