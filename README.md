# EliteDataConverter

This project reads an input xml file (custom format) to generate a cypher script that can be run in neo4j.
The input data can be found here : https://github.com/peregrin71/EliteDataConverter/blob/master/EliteGraph/Data/EDData.xml


To setup a neo4j database check this website : https://neo4j.com/developer/neo4j-desktop/

To fill the neo4j database with data
1) Open: https://github.com/peregrin71/EliteDataConverter/blob/master/EliteGraph/Data/EDData.cypher
2) Copy the first command "MATCH (n) DETACH DELETE n" into the neo4j browser window to clear the database.
3) run the command
4) copy from line 4 till the end into the neo4j browser
5) run the set of commands

Now you can start running queries, like:

match (c:Corporation)-[e]-(n) where (c.name="Azimuth Biochemicals") return *

match (c:Person)-[e]-(n) where (c.name="Kahina Tijani Loren") return *

Have fun commanders :)
