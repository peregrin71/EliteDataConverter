# EliteDataConverter

This project reads an input xml file (custom format) to generate a cypher script that can be run in neo4j.
To setup such a database check this website : https://neo4j.com/developer/neo4j-desktop/

Then open the cypher file 
https://github.com/peregrin71/EliteDataConverter/blob/master/EliteGraph/Data/EDData.cypher

Fill the neo4j database with data
1) Copy the first command "MATCH (n) DETACH DELETE n" into the neo4j browser window to clear the database.
2) run the command
3) copy from line 4 till the end into the neo4j browser
4) run the set of commands

Now you can start running queries, like:

match (c:Corporation)-[e]-(n) where (c.name="Azimuth Biochemicals") return *
match (c:Person)-[e]-(n) where (c.name="Kahina Tijani Loren") return *

Have fun commanders :)
