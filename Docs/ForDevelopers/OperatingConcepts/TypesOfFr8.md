# HOW FR8 MANIPULATES DATA  
[Go to Contents](https://github.com/Fr8org/Fr8Core/blob/master/Docs/Home.md)   

Fr8 is data that is stored in [Crates](https://github.com/Fr8org/Fr8Core/blob/master/Docs/ForDevelopers/Objects/Crate.md).  When a Fr8 Hub executes a Fr8 Plan, it creates a Container that can be thought of as similar to a real-world shipping container. 1 or more Crates of data are stored inside that Container 

### Going Deeper
 Type | Description
 --- | ----   
 Design-Time Crates |  Used to design and link Actions together and is mostly generated at design-time and stored on fr8 Routes. Common examples: Crates of Controls that are used to render configuration UI on the client. Crates of Fields that allow users to map data from one service to another
 Payload Crates |  Data that’s generated at run-time and carried from Terminal to Terminal by a Fr8 Container. Examples include rows of excel data, field information from inside a DocuSign envelope, and values pulled from a Sql Server
### Design-Time Crates
 Design-Time Crates are stored on Activities and generally consist of two types:

a) **Controls**. Information in Controls Crates consists of one or more defined controls.  Controls Crates are used to render UI in the Plan Builder on the client, and then send the info provided by the user back to the server.

b) **Fields**.  Activities publish and expose crates of field information so other Activities (usually downstream) can dynamically adapt and incorporate the upstream information. These Crates do not carry run-time payload itself.

Examples: A Terminal is provided with an authentication token and retrieves all of the DocuSignTemplates associated with this particular DocuSign account. This list of fields will be stored in a Crate with Label = “AvailableDocuSignTemplates” and Type = “PluginDesignTimeData” and Terminal=”Docusign”. Then when the client goes to render the drop down list box, and it notices that the control is marked to be populated with “AvailableDocuSignTemplates”, it uses data from this list.  

[Go to Contents](https://github.com/Fr8org/Fr8Core/blob/master/Docs/Home.md)  
