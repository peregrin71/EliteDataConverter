# EliteDataConverter

This project reads an input xml file (custom format) to generate a cypher script that can be run in neo4j.
The input data can be found here : </br>
https://github.com/peregrin71/EliteDataConverter/blob/master/EliteGraph/Data/EDData.xml </br>
</br>
To setup a neo4j database check this website : https://neo4j.com/developer/neo4j-desktop/ </br>
</br>
To fill the neo4j database with data :
1) Open: https://github.com/peregrin71/EliteDataConverter/blob/master/EliteGraph/Data/EDData.cypher
2) Copy the first command "MATCH (n) DETACH DELETE n" into the neo4j browser window to clear the database.
3) run the command
4) copy from line 4 till the end into the neo4j browser
5) run the set of commands

Now you can start running queries, like:
match (c:Corporation)-[e]-(n) where (c.name="Azimuth Biochemicals") return * </br>
match (c:Person)-[e]-(n) where (c.name="Kahina Tijani Loren") return * </br>
</br>
Have fun commanders :) </br>

//---------------------------------------------------------------------------------------------------------------------</br>
The input database EDData.xml</br>
//-----------------------</br>
</br>
Definitions:</br>
**Values**			: A list of values to be later used as valid strings in the graph </br>
					  (not all the values used in the graph need to be here, typically relations can be given any name)</br>
**Enitities**		: A list of {key,name} pairs, these will represent things you can refer to later</br>
**Relations**		: Relation definitions more details later</br>

**Action**</br>
Something happening in a moment of time
- You can predefine named actions in the entities section
- Actions will typically look like this</br>
<pre>
&lt;Action&gt;
  &lt;Actor from="actor key"/&gt;          // The entity doing the action
  &lt;Verb to="verb key"/&gt;              // The verb of this action, must be from the verbs list
  &lt;Target to="target key"/&gt;          // Target of the action
  &lt;Date to="date string"/&gt;           // Optional, date of the action. 
                                     // Valid date strings are "YYYY", "YYYY-MM", "YYYY-MM-DD"  (e.g. "3300", "3300-01-07")
  &lt;YourRelation to="relation key"    // any optional to relation you want to add to the action (.e.g Note to="")
&lt;/Action&gt;
</pre>

- **Activity**			
Something happening over a longer time, eg. a war
- You can predefine named activities in the entities section. e.g. "Galactic Summit"
- Activities will typically look like this</br>
<pre>
&lt;Activity&gt;
  &lt;Actor from="actor key"/&gt;          // The entity doing the activity
  &lt;Verb to="verb key"/&gt;              // The verb of this action, must be from the verbs list
  &lt;Target to="target key"/&gt;          // Target of the action
  &lt;Date to="date string"/&gt;           // Optional, period of the activity. Ususally a year or year/mont
  &lt;FromDate to="date string"/&gt;       // Optional, start date of the activity. 
  &lt;ToDate to="date string"/&gt;         // Optional, end date of the activity. 
  &lt;YourRelation to="relation key"    // any optional to relation you want to add to the activity
&lt;/Action&gt;
</pre>

**Membership**
Kind of an activity too, but it denotes a person being a member of an organization


- Complex entities : Hierarhical data structures
						- First you can define some normal entities to be used in the hierarchy in an entities child node
						- Then you define a collections child node that will host the collection
							- Within this node you can define new childnodes they either define a
								- simple statement they have a "to" attribute 
								  These create an relation with the name of the childnode to the node with given key
								- a new item in the collection they will have a "key" and a "name" attribute
								  and create a new subcollection (which may be empty in which case it will be an element in the collection)
								  By default a relation from the parent to the child node will be made which is called "contains"
								  however the item can also have an optional "rel" attribute. If present the "contians"
								  relation from parent to child will not be made but a relation with the value of the attribute
								  will be made from child to parent
						- For an example look at the "Systems" key in the database which is used to build up
						  "solar" systems.






