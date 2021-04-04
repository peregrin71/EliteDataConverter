using System;
using System.Collections.Generic;
using System.Text;
using System.Xml;
using System.IO;

namespace EliteGraph
{
    public class Graph
    {
        internal Dictionary<string, string> Keywords { get; private set; }
        internal Dictionary<string, string> NodeData { get; set; }
        internal Dictionary<string, Node> Nodes { get; set; }
        internal HashSet<Edge> Edges { get; set; }

        public Graph()
        {
            Keywords = new Dictionary<string, string>();
            NodeData = new Dictionary<string, string>();
            Nodes = new Dictionary<string, Node>();
            Edges = new HashSet<Edge>();

            NodeData.Add("labels", "string");
            NodeData.Add("key", "string");
            NodeData.Add("name", "string");
        }

        public void LoadEDData(string filename)
        {
            var doc = new XmlDocument();
            XmlReaderSettings settings = new XmlReaderSettings();
            settings.IgnoreComments = true;
            XmlReader reader = XmlReader.Create(filename, settings);
            doc.Load(reader);

            LoadValues(doc.SelectSingleNode(@"Data/Values"));
            LoadEntities(doc.SelectSingleNode(@"Data/Entities"));
            LoadPersons(doc.SelectSingleNode(@"Data/Persons"));
            LoadActions(doc.SelectSingleNode(@"Data/Actions"));
            LoadActivities(doc.SelectSingleNode(@"Data/Activities"));
            LoadStatements(doc.SelectSingleNode(@"Data/Statements"));
            LoadMemberships(doc.SelectSingleNode(@"Data/Memberships"));
        }

        private void LoadValues(XmlNode values_node)
        {
            foreach (XmlNode collection_node in values_node.ChildNodes)
            {
                foreach (XmlNode value_node in collection_node)
                {
                    var key = value_node.Attr("key");
                    var name = value_node.Attr("name");

                    var new_node = new Node(key, value_node.Name, name);
                    new_node.IsKeyword = true;
                    Nodes.Add(key, new_node);
                }
            }
        }

        private void LoadEntities(XmlNode entities_node)
        {
            foreach (XmlNode entity_node in entities_node)
            {
                foreach (XmlNode node in entity_node)
                {
                    var entity = node.Name;
                    var key = node.Attr("key");
                    var name = node.Attr("name");
                    var output_node = new Node(key, entity, name);
                    Nodes.Add(key, output_node);
                    Console.WriteLine("Added {0}, key=\"{1}\", name=\"{2}\"", entity, key, name);
                }
            }
        }

        private void LoadStatements(XmlNode statements_node)
        {
            foreach (XmlNode node in statements_node)
            {
                var source_key = node.Attr("source");
                var link_key = node.Attr("link");
                var target_key = node.Attr("target");

                // lookup and validate correctness
                var source_node = Nodes[source_key];
                var link_node = Nodes[link_key];
                var target_node = Nodes[target_key];

                if (!((link_node.Kind == "Verb") || (link_node.Kind == "Noun")))
                {
                    throw new System.Xml.XmlException("link must be a verb or noun");
                }

                var edge = new Edge(source_node, link_node.Data["name"], target_node);
                Edges.Add(edge);
            }
        }

        private string MakeDateKey(string date)
        {
            return "Date_" + date.Replace('-', '_');
        }

        private void LoadActions(XmlNode xml_actions_node)
        {
            foreach (XmlNode xml_action_node in xml_actions_node)
            {
                var key = xml_action_node.Attr("key");

                string actor = "unknown";
                string target = "unknown";
                string verb = "unknown";

                bool make_new_node = (key == null);
                var action_node = make_new_node ? new Node("Action") : Nodes[key];

                if (make_new_node)
                {
                    Nodes.Add(action_node.Key, action_node);
                }

                foreach (XmlNode node in xml_action_node.ChildNodes)
                {
                    var edge_type = node.Name;
                    var source_node_key = node.Attr("source");
                    var target_node_key = node.Attr("target");

                    // automatically create Date nodes if needed
                    if (edge_type == "Date")
                    {
                        var date = node.Attr("target");
                        target_node_key = MakeDateKey(date);

                        if (!Nodes.ContainsKey(target_node_key))
                        {
                            var new_date_node = new Node(target_node_key, "Date", date);
                            Nodes.Add(target_node_key, new_date_node);
                        }
                    }

                    Edge edge = null;

                    if (source_node_key != null)
                    {
                        var source_node = Nodes[source_node_key];
                        edge = new Edge(source_node, edge_type, action_node);
                    }

                    if (target_node_key != null)
                    {
                        var target_node = Nodes[target_node_key];
                        edge = new Edge(action_node, edge_type, target_node);
                    }

                    if (edge != null)
                    {
                        Edges.Add(edge);
                        if (edge.Kind == "Verb") verb = edge.To.Name.ToLower();
                        if (edge.Kind == "Actor") actor = edge.From.Name;
                        if (edge.Kind == "Target") target = edge.To.Name;
                    }
                }

                if (key == null)
                {
                    action_node.SetName(actor + " " + verb + " " + target);
                }
            }
        }

        private void LoadPersons(XmlNode persons_xml_node)
        {
            foreach (XmlNode person_xml_node in persons_xml_node)
            {
                var person_key = person_xml_node.Key();
                var person_node = Nodes[person_key];

                foreach (XmlNode xml_node in person_xml_node)
                {
                    var edge_type = xml_node.Name;
                    var target_node_key = xml_node.Attr("target");
                    var source_node_key = xml_node.Attr("source");

                    // Automatically generate date nodes if needed
                    if (edge_type.Contains("Date"))
                    {
                        var date = target_node_key;
                        target_node_key = MakeDateKey(target_node_key);

                        if (!Nodes.ContainsKey(target_node_key))
                        {
                            var new_date_node = new Node(target_node_key, "Date", date);
                            Nodes.Add(target_node_key, new_date_node);
                        }
                    }

                    if (target_node_key != null)
                    {
                        var target_node = Nodes[target_node_key];
                        var edge = new Edge(person_node, edge_type, target_node);
                        Edges.Add(edge);
                    }

                    if (source_node_key != null)
                    {
                        var source_node = Nodes[source_node_key];
                        var edge = new Edge(source_node, edge_type, person_node);
                        Edges.Add(edge);
                    }
                }
            }
        }
        private void LoadMemberships(XmlNode memberships_xml_node)
        {
            foreach (XmlNode membership_xml_node in memberships_xml_node)
            {
                var new_node = new Node("Membership");
                string person = "Unknown";
                string organization = "Unknown";

                Nodes.Add(new_node.Key, new_node);

                foreach (XmlNode xml_node in membership_xml_node)
                {
                    var edge_type = xml_node.Name;
                    var target_node_key = xml_node.Attr("target");

                    // Automatically generate date nodes if needed
                    if (edge_type.Contains("Date"))
                    {
                        var date = target_node_key;
                        target_node_key = MakeDateKey(target_node_key);

                        if (!Nodes.ContainsKey(target_node_key))
                        {
                            var new_date_node = new Node(target_node_key, "Date", date);
                            Nodes.Add(target_node_key, new_date_node);
                        }

                    }

                    var target_node = Nodes[target_node_key];
                    var edge = new Edge(new_node, edge_type, target_node);
                    Edges.Add(edge);

                    if (edge_type == "Member") person = target_node.Name;
                    if (edge_type == "Organization") organization = target_node.Name;
                }

                new_node.SetName(organization + " has member " + person);
            }
        }
        private void LoadActivities(XmlNode activities_xml_node)
        {
            foreach (XmlNode activity_xml_node in activities_xml_node)
            {

                var key = activity_xml_node.Attr("key");

                string actor = "unknown";
                string target = "unknown";
                string noun = "unknown";

                bool make_new_node = (key == null);
                var activity_node = make_new_node ? new Node("Activity") : Nodes[key];

                if (make_new_node)
                {
                    Nodes.Add(activity_node.Key, activity_node);
                }

                foreach (XmlNode xml_node in activity_xml_node)
                {
                    var edge_type = xml_node.Name;
                    var target_node_key = xml_node.Attr("target");
                    var source_node_key = xml_node.Attr("source");

                    // Automatically generate date nodes if needed
                    if (edge_type.Contains("Date"))
                    {
                        var date = target_node_key;
                        target_node_key = MakeDateKey(target_node_key);

                        if (!Nodes.ContainsKey(target_node_key))
                        {
                            var new_date_node = new Node(target_node_key, "Date", date);
                            Nodes.Add(target_node_key, new_date_node);
                        }
                    }

                    if (source_node_key != null)
                    {
                        var source_node = Nodes[source_node_key];
                        var edge = new Edge(source_node, edge_type, activity_node);
                        Edges.Add(edge);

                        if (edge_type == "Actor") actor = source_node.Name;
                    }

                    if (target_node_key != null)
                    {
                        var target_node = Nodes[target_node_key];
                        var edge = new Edge(activity_node, edge_type, target_node);
                        Edges.Add(edge);

                        if (edge_type == "Verb") noun = target_node.Name;
                        if (edge_type == "Target") target = target_node.Name;
                    }
                }

                if (key == null)
                {
                    activity_node.SetName(actor + " " + noun + " " + target);
                }
            }
        }
    }
}
