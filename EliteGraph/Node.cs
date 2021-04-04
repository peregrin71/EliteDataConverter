using System;
using System.Collections.Generic;
using System.Text;

namespace EliteGraph
{
    public class Node
    {
        private static UInt32 sID { get; set; }

        public UInt32 ID { get; private set;}
        public string Key {  get; private set;}
        public string Kind { get; private set; }
        public string Name { get; private set; }
        public Dictionary<string, string> Data { get; private set; }

        public bool IsUsed { get; set; }
        public bool IsKeyword { get; set; }

        static Node()
        {
            sID = 0;
        }

        public Node(string kind)
        {
            Data = new Dictionary<string, string>();
            ID = ++sID;
            Key = kind + "_" + ID.ToString();
            Kind = kind;
            IsUsed = false;
            IsKeyword = false;
        }

        public Node(string key, string kind, string name)
        {
            Data = new Dictionary<string, string>();
            ID = ++sID;
            Key = key;
            Kind = kind;
            Name = name;
            AddData("name", name);
            IsUsed = false;
            IsKeyword = false;
        }

        public void SetName(string name)
        {
            Name = name;
            AddData("name", name);
        }

        public void AddData(string key, string value)
        {
            Data[key] = value;
        }
    }
}
