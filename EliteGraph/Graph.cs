using System;
using System.Collections.Generic;
using System.Text;
using System.Xml;
using System.IO;
using System.Text.RegularExpressions;

namespace EliteGraph
{
    public class Graph
    {
        private static uint AutoKeyCount = 0;

        private const string str_to = "to";
        private const string str_from = "from";

        private HashSet<string> PersonDetails { get; set; }
        private HashSet<string> CorporationDetails { get; set; }

        private HashSet<string> AutoCreateOrGetTypes { get; set; }

        private Regex DateRegex { get; set; }
        private Regex PeriodRegex { get; set; }

        internal Dictionary<string, string> NodeData { get; set; }
        internal Dictionary<string, Node> Nodes { get; set; }
        internal HashSet<Edge> Edges { get; set; }

        public Graph()
        {
            AutoCreateOrGetTypes = new HashSet<string>();
            CorporationDetails = new HashSet<string>();
            PersonDetails = new HashSet<string>();
            NodeData = new Dictionary<string, string>();
            Nodes = new Dictionary<string, Node>();
            Edges = new HashSet<Edge>();

            DateRegex = new Regex(@"\d{4}-\d{2}-\d{2}", RegexOptions.Compiled);
            PeriodRegex = new Regex(@"(\d{4}$)|(\d{4}-\d{2})$", RegexOptions.Compiled);

            NodeData.Add("labels", "string");
            NodeData.Add("key", "string");
            NodeData.Add("name", "string");
        }

        public void Load(string path)
        {
            string filename = path + @"\EDData.xml";
            var doc = new XmlDocument();
            var settings = new XmlReaderSettings();
            settings.IgnoreComments = true;
            XmlReader reader = XmlReader.Create(filename, settings);
            doc.Load(reader);

            LoadAutoCreateOrGetTypes(doc.SelectSingleNode(@"Data/AutoCreateOrGetTypes"));
            LoadValues(doc.SelectSingleNode(@"Data/Values"));
            LoadEntities(doc.SelectSingleNode(@"Data/Entities"));
            LoadEntities(doc.SelectSingleNode(@"Data/Topography/Entities"));

            // load hierarchies
            LoadHierarchy(doc.SelectSingleNode(@"Data/Topography/Systems"));

            // load relationships next
            LoadActionsOrActivities(doc.SelectSingleNode(@"Data /Actions"));
            LoadActionsOrActivities(doc.SelectSingleNode(@"Data/Activities"));
            LoadCorporations(doc.SelectSingleNode(@"Data/Corporations"));
            LoadMemberships(doc.SelectSingleNode(@"Data/Memberships"));
            LoadPersons(doc.SelectSingleNode(@"Data/Persons"));
            LoadCombinedStatements(doc.SelectSingleNode(@"Data/ShipTypes"));
            LoadCombinedStatements(doc.SelectSingleNode(@"Data/Ships"));
            LoadStatements(doc.SelectSingleNode(@"Data/Statements"));

            System.Console.WriteLine("Loaded {0} nodes", Nodes.Count);
            System.Console.WriteLine("Loaded {0} eges", Edges.Count);
        }

        private Edge CreateEdge(Node from, string kind, Node to)
        {
            var edge = new Edge(from, kind, to);
            Edges.Add(edge);
            return edge;
        }

        private Node CreateNode(XmlNode xml_node)
        {
            var kind = xml_node.Name;
            var key = xml_node.RequiredAttr("key");
            var name = xml_node.RequiredAttr("name");
            var node = new Node(key, kind, name);
            Nodes.Add(key, node);
            return node;
        }

        private Node GetOrAutoCreateTargetNode(string key, string input_kind)
        {
            // Lookup has first prioriy, only create new nodes if node is not there yet.
            if ((key != null) && (Nodes.ContainsKey(key)))
            {
                return Nodes[key];
            }

            // Convert anything with date in it to date, so we will end up with one date key for each date 
            // instead of FromDate, ToData, BirthDate etc..
            var kind = input_kind.Contains("Date") ? "Date" : input_kind; 
            var auto_key = key ?? kind;
            var name = key ?? kind;

            if (AutoCreateOrGetTypes.Contains(kind))
            {
                // Dates with only a year, or year-month will be automically converted to a period
                if (PeriodRegex.Match(auto_key).Success)
                {
                    kind = "Period";
                }

                if ((kind == "Date") && !DateRegex.Match(key).Success)
                {
                    throw new InvalidDataException("Invalid date format used, use YYYY-MM-DD");
                }

                if (key != null)
                {
                    auto_key = kind + "_" + key.Replace("-", "_");
                }
                else
                {
                    AutoKeyCount++;
                    auto_key = String.Format("{0}_{1}", kind, AutoKeyCount);
                }

                if (!Nodes.ContainsKey(auto_key))
                {
                    var node = new Node(auto_key, kind, name);
                    Nodes.Add(auto_key, node);

                    if ((kind == "Date") || ((kind == "Period") && (key.Length > 4)))
                    {
                        // First create a month group then a year group
                        string period = (kind == "Date") ? key.Substring(0, 7) : key.Substring(0, 4);
                        var period_node = GetOrAutoCreateTargetNode(period, "Period");
                        CreateEdge(node, "Period", period_node);
                    }
                }
            }
            return Nodes[auto_key];
        }

        private void LoadAutoCreateOrGetTypes(XmlNode xml_types_node)
        {
            foreach (XmlNode xmlNode in xml_types_node)
            {
                var name = xmlNode.RequiredAttr("name");
                AutoCreateOrGetTypes.Add(name);
            }
        }

        private void LoadValues(XmlNode values_node)
        {
            var xml_person_details = values_node.SelectSingleNode("PersonDetails");
            var xml_corporation_details = values_node.SelectSingleNode("CorporationDetails");
            var xml_verbs_node = values_node.SelectSingleNode("Verbs");
            var xml_nouns_node = values_node.SelectSingleNode("Nouns");

            foreach (XmlNode xml_node in xml_person_details)
            {
                var name = xml_node.RequiredAttr("name");
                PersonDetails.Add(name);
            }

            foreach (XmlNode xml_node in xml_corporation_details)
            {
                var name = xml_node.RequiredAttr("name");
                CorporationDetails.Add(name);
            }

            foreach (XmlNode xml_noun_node in xml_nouns_node)
            {
                xml_noun_node.RequiredAttr("name");
                CreateNode(xml_noun_node);
            }

            foreach (XmlNode xml_verb_node in xml_verbs_node)
            {
                xml_verb_node.RequiredAttr("name");
                CreateNode(xml_verb_node);
            }
        }

        private void LoadEntities(XmlNode xml_entities_node)
        {
            foreach (XmlNode xml_entity_node in xml_entities_node)
            {
                // ensure all sub entities share the same name, finds types
                string entity = null;
                foreach (XmlNode xml_node in xml_entity_node)
                {
                    if (entity == null) entity = xml_node.Name;
                    if (entity != xml_node.Name)
                    {
                        throw new InvalidDataException(String.Format("entity {0} does not match {1}", xml_node.Name, entity));
                    }

                    if (entity == "System")
                    {
                        var key = xml_node.RequiredAttr("key");
                        var name = xml_node.RequiredAttr("name");
                        System.Diagnostics.Trace.WriteLine(String.Format("  <System key=\"{0}\" name=\"{1}\">", key, name));
                        System.Diagnostics.Trace.WriteLine("    <!--<Attribute to=\"\"/>-->");
                        System.Diagnostics.Trace.WriteLine("    <!--<Region key=\"\"/>-->");
                        System.Diagnostics.Trace.WriteLine("    <!--<Star key=\"\"/>-->");
                        System.Diagnostics.Trace.WriteLine("    <!--<Body key=\"\" name=\"\">-->");
                        System.Diagnostics.Trace.WriteLine("      <!--<Base key=\"\" name=\"\"/>-->");
                        System.Diagnostics.Trace.WriteLine("      <!--<City key=\"\" name=\"\"/>-->");
                        System.Diagnostics.Trace.WriteLine("      <!--<Orbits key=\"\" name=\"\"/>-->");
                        System.Diagnostics.Trace.WriteLine("      <!--<Location key=\"\" name=\"\"/>-->");
                        System.Diagnostics.Trace.WriteLine("      <!--<Station key=\"\" name=\"\"/>-->");
                        System.Diagnostics.Trace.WriteLine("    <!--</Body>-->");
                        System.Diagnostics.Trace.WriteLine("  </System>");
                    }

                    CreateNode(xml_node);
                }
            }
        }

        private void LoadStatements(XmlNode statements_node)
        {
            foreach (XmlNode node in statements_node)
            {
                var source_key = node.Attr(str_from);
                var link_key = node.Attr("link");
                var target_key = node.Attr(str_to);

                // lookup and validate correctness
                var source_node = Nodes[source_key];
                var link_node = Nodes[link_key];
                var target_node = Nodes[target_key];

                if (!((link_node.Kind == "Verb") || (link_node.Kind == "Noun")))
                {
                    throw new System.Xml.XmlException("link must be a verb or noun");
                }

                CreateEdge(source_node, link_node.Data["name"], target_node);
            }
        }

        bool IsValidPersonalDetail(string detail)
        {
            return PersonDetails.Contains(detail);
        }

        bool IsValidCorporationDetail(string detail)
        {
            return CorporationDetails.Contains(detail);
        }

        void AddNote(Node node, XmlNode xml_node)
        {
            var key = xml_node.Attr("key");
            var note_node = GetOrAutoCreateTargetNode(key, "Note");
            CreateEdge(node, "Note", note_node);
        }

        void AddNotes(Node node, XmlNode xml_notes_node)
        {
            if (xml_notes_node != null)
            {
                foreach (XmlNode xml_note_node in xml_notes_node)
                {
                    AddNote(node, xml_note_node);
                }
            }
        }

        private void LoadCorporations(XmlNode xml_corporations_node)
        {
            foreach (XmlNode xml_corporation_node in xml_corporations_node)
            {
                var corporation_key = xml_corporation_node.Key();
                var corporation_node = Nodes[corporation_key];

                var xml_details_node = xml_corporation_node.SelectSingleNode("Details");
                var xml_notes_node = xml_corporation_node.SelectSingleNode("Notes");
                var xml_statements_node = xml_corporation_node.SelectSingleNode("Statements");

                foreach (XmlNode xml_node in xml_details_node)
                {
                    var detail = xml_node.Name;
                    var target_node_key = xml_node.RequiredAttr(str_to);

                    // avoid creating a whole lot of random relations
                    if (IsValidCorporationDetail(detail))
                    {
                        var target_node = GetOrAutoCreateTargetNode(target_node_key, detail);
                        CreateEdge(corporation_node, detail, target_node);
                    }
                    else
                    {
                        System.Console.WriteLine("Invalid detail '{0}' for corporation '{1}' encountered", detail, corporation_node.Name);
                    }
                }

                AddNotes(corporation_node, xml_notes_node);

                if (xml_statements_node != null)
                {
                    foreach (XmlNode xml_node in xml_statements_node)
                    {
                        var statement = xml_node.Name;
                        var target_node_key = xml_node.Attr(str_to);
                        var source_node_key = xml_node.Attr(str_from);

                        if (Nodes.ContainsKey(target_node_key))
                        {
                            var target_node = Nodes[target_node_key];
                            CreateEdge(corporation_node, statement, target_node);
                        }
                        else
                        {
                            System.Console.WriteLine("Invalid statement '{0}' for corporation '{1}' encounterd", statement, corporation_node.Name);
                        }
                    }
                }
            }
        }

        private void LoadCombinedStatements(XmlNode xml_collection_node)
        {
            foreach (XmlNode xml_item_node in xml_collection_node)
            {
                var from_key = xml_item_node.RequiredAttr("key");
                var from = Nodes[from_key];

                foreach (XmlNode xml_statement_node in xml_item_node)
                {
                    var kind = xml_statement_node.Name;
                    var to_key = xml_statement_node.RequiredAttr(str_to);
                    var to = Nodes[to_key];
                    CreateEdge(from, kind, to);
                }
            }
        }

        private void LoadPersons(XmlNode xml_persons_node)
        {
            foreach (XmlNode xml_person_node in xml_persons_node)
            {
                var person_key = xml_person_node.Key();
                var person_node = Nodes[person_key];

                var xml_Details_node = xml_person_node.SelectSingleNode("Details");
                var xml_statements_node = xml_person_node.SelectSingleNode("Statements");

                foreach (XmlNode xml_node in xml_Details_node)
                {
                    var detail = xml_node.Name;
                    var target_node_key = xml_node.Attr(str_to);
                    var source_node_key = xml_node.Attr(str_from);

                    // avoid creating a whole lot of random relations
                    if (IsValidPersonalDetail(detail))
                    {
                        //target_node_key = CreateDateNodeIfNeeded(xml_node, target_node_key);

                        if (target_node_key != null)
                        {
                            var target_node = GetOrAutoCreateTargetNode(target_node_key, detail);
                            CreateEdge(person_node, detail, target_node);
                        }

                        if (source_node_key != null)
                        {
                            var source_node = Nodes[source_node_key];
                            CreateEdge(source_node, detail, person_node);
                        }
                    }
                    else
                    {
                        System.Console.WriteLine("Invalid detail '{0}' for person '{1}' encountered", detail, person_node.Name);
                    }
                }

                if (xml_statements_node != null)
                {
                    foreach (XmlNode xml_node in xml_statements_node)
                    {
                        var statement = xml_node.Name;
                        var target_node_key = xml_node.Attr(str_to);
                        var source_node_key = xml_node.Attr(str_from);

                        if (Nodes.ContainsKey(target_node_key))
                        {
                            var target_node = Nodes[target_node_key];
                            CreateEdge(person_node, statement, target_node);
                        }
                        else
                        {
                            System.Console.WriteLine("Invalid statement '{0}' for person '{1}' encounterd", statement, person_node.Name);
                        }
                    }
                }
            }
        }

        private void LoadMemberships(XmlNode memberships_xml_node)
        {
            foreach (XmlNode membership_xml_node in memberships_xml_node)
            {
                var key = membership_xml_node.Attr("key");
                var new_node = GetOrAutoCreateTargetNode(key, "Membership");

                foreach (XmlNode xml_node in membership_xml_node)
                {
                    var edge_type = xml_node.Name;
                    var target_node_key = xml_node.Attr(str_to);
                    var source_node_key = xml_node.Attr(str_from);

                    if (target_node_key != null)
                    {
                        var target_node = GetOrAutoCreateTargetNode(target_node_key, edge_type);
                        CreateEdge(new_node, edge_type, target_node);
                    }

                    if (source_node_key != null)
                    {
                        var source_node = GetOrAutoCreateTargetNode(source_node_key, edge_type);
                        CreateEdge(source_node, edge_type, new_node);
                    }
                }
            }
        }

        private void LoadActionsOrActivities(XmlNode xml_node)
        {
            foreach (XmlNode xml_period_node in xml_node)
            {
                var period = xml_period_node.Name;
                if (period != "Period")
                {
                    throw new InvalidDataException("Childnode is not a period");
                }

                foreach (XmlNode xml_action_node in xml_period_node)
                {
                    var key = xml_action_node.Attr("key");
                    var name = xml_node.Name;

                    if (!(name == "Actions" || name == "Activities"))
                    {
                        throw new InvalidDataException("Childode is not an action or actity");
                    }

                    var kind = (name == "Activities") ? "Activity" : "Action";
                    var action_node = GetOrAutoCreateTargetNode(key, kind);

                    foreach (XmlNode xml_child_node in xml_action_node.ChildNodes)
                    {
                        var edge_type = xml_child_node.Name;
                        var source_node_key = xml_child_node.Attr(str_from);

                        if (source_node_key != null)
                        {
                            var source_node = Nodes[source_node_key];
                            CreateEdge(source_node, edge_type, action_node);
                        }
                        else
                        {
                            var target_node_key = xml_child_node.RequiredAttr(str_to);
                            var target_node = GetOrAutoCreateTargetNode(target_node_key, edge_type);
                            CreateEdge(action_node, edge_type, target_node);
                        }
                    }
                }
            }
        }

        private void LoadHierarchy(XmlNode xml_node)
        {
            LoadHierarchy(null, xml_node);
        }

        private bool IsCollection(XmlNode xmlNode)
        {
            return xmlNode.HasChildNodes || (xmlNode.Attr("key") != null);
        }

        private void LoadHierarchy(Node parent, XmlNode xml_node)
        {
            foreach (XmlNode xml_child_node in xml_node.ChildNodes)
            {
                if (IsCollection(xml_child_node))
                {
                    var edge_type = xml_child_node.Attr("rel") ?? "contains";
                    var type_key = xml_child_node.Attr("type");
                    var new_node = CreateNode(xml_child_node);

                    if (xml_child_node.HasChildNodes)
                    {
                        LoadHierarchy(new_node, xml_child_node);
                    }

                    if (parent != null)
                    {
                        if (edge_type == "contains")
                        {
                            CreateEdge(parent, edge_type, new_node);
                        }
                        else
                        {
                            CreateEdge(new_node, edge_type, parent);
                        }
                    }

                    if (type_key != null)
                    {
                        var type_node = Nodes[type_key];
                        CreateEdge(new_node, "Type", type_node);
                    }
                }
                else
                {
                    var edge_kind = xml_child_node.Name;
                    var to_key = xml_child_node.RequiredAttr("to");
                    var to = Nodes[to_key];
                    CreateEdge(parent, edge_kind, to);
                }
            }
        }
    }
}
