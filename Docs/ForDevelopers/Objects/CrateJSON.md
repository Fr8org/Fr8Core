# Crate JSON

[Go to Index](https://github.com/Fr8org/Fr8Core/blob/master/Docs/Home.md)  
[Go to Crate Home](https://github.com/Fr8org/Fr8Core/blob/master/Docs/ForDevelopers/Objects/Crate.md)

A Crate is a Json element that can contain arbitrary data.  It creates a standardized way to store different kinds of data in an organized way.

Crates are stored inside of Activities (where they’re used to store design-time information used to drive the client user experience) and Containers (where they contain run-time payload, the “real information” that the user wants to manipulate.

Crates are stored in an array inside the crateStorage element of these parent elements.

Here’s a sample of crates inside of an Activity’s crate storage:

```javascript
"crateStorage": {
            "crates": [
              {
                "id": "a3d8aec5-2f72-4cbd-b83f-13d4163c6c31",
                "label": "Configuration_Controls",
                "contents": {
                  "Controls": [
                    {
                      "isReadOnly": true,
                      "name": null,
                      "required": false,
                      "value": "<img height=\"30px\" src=\"/Content/icons/web_services/DocuSign-Logo.png\"><p>You will be asked to select a DocuSign Template.</p><p>Each time a related DocuSign Envelope is completed, we'll extract the data for you.</p>",
                      "label": "",
                      "type": "TextArea",
                      "selected": false,
                      "events": null,
                      "source": null,
                      "help": null,
                      "errorMessage": null
                    },
                    {
                      "listItems": [
                        {
                          "key": "Write to Azure Sql Server",
                          "value": "88",
                          "tags": null,
                          "availability": 1,
                          "sourceCrateManifest": {
                            "Type": null,
                            "Id": 0
                          },
                          "sourceCrateLabel": null
                        },
                        { }]
                    }
 ```

## Crate Elements

Name |	Type |	Notes   
---|---|---
Id |	GUID |	not required and not currently used as part of processing.   
Label |	string |	an arbitrary name useful for distinguishing crates from each other.   
Contents |	string (JSON) |	The actual data inside of the crate. Everything else can be thought of as metadata taped to the outside of the crate. This distinction is important from a security point of view. Generally, properties other than Contents should be assumed to be publicly visible and insecure. On the other hand, by encrypting the Contents of a crate, it can be considered to be data secure.   
ParentCrateId |	string |	Crates can be put inside of other Crates   
[ManifestType](https://github.com/Fr8org/Fr8Core/blob/master/Docs/ForDevelopers/Objects/CratesManifest.md)  |	string |	A friendly name for different types of Manifests. Not really necessary, but storing it here makes the json more readable   
[ManifestId](https://github.com/Fr8org/Fr8Core/blob/master/Docs/ForDevelopers/Objects/CratesManifest.md)  |	int |	An optional way to signal the schema of the contents.   
Manufacturer |	ManufacturerDTO |	A data structure identifying the Terminal that created the crate   
Mode |	string |	choices “Design-Time”, “Run-Time”. [Review for need]   

[Crate Manifests](https://github.com/Fr8org/Fr8Core/blob/master/Docs/ForDevelopers/Objects/CratesManifest.md)
 
 [Go to Index](https://github.com/Fr8org/Fr8Core/blob/master/Docs/Home.md)  
