Activity Categories
===================

Categories are applied to an ActivityTemplate like tags. The Activity designer can specify as many categories as make sense.

Categories are exposed to the user:
1) as selectable choices in the main Activity Chooser UI in the client
2) as static Category Pages that are generated to provide a focus page for a category.

Category Example: Web Service
----------------------------

The most important Category is usually the associated Web Service. For example, if you create an Activity that works with DocuSign, you should add the "DocuSign" Category to its [ActivityTemplate](/Docs/ForDevelopers/Objects/ActivityTemplates.md) so it shows up grouped with the other DocuSign Activities.

Category Example: The Main Buckets
----------------------------------

Activity Designers are encouraged to categorize their Activity into one of the four "Main Buckets":

Category |	Description | JSON sample	
--- | --- | ---
Triggers |	Activities that wait for an external event notification to arrive |	{ "iconPath": "/Content/icons/monitor-icon-64x64.png", "name": "Triggers" }
Get Data |	Also known as “Getters”. Activities that attempt to load or acquire a piece of data from an external source | { "iconPath": "/Content/icons/get-icon-64x64.png", "name": "Get Data" } 	
Processors |	Activities that focus on processing and transforming Container data at run-time. They are not forbidden from connecting to external sources, but they’re encouraged to focus on carrying out a single transform of the payload data | { "iconPath": "/Content/icons/process-icon-64x64.png", "name": "Process Data" }
Ship Data |	Activities that push or send data to an external service or another Fr8 Container/Plan | { "iconPath": "/Content/icons/forward-icon-64x64.png", "name": "Ship Data" } 

It is possible to specify more than one Category for single Activity.


Specifying Categories
----------------------
The Categories field in the ActivityTemplate specifies a list of Activity Categories to which current Activity belongs to.

1) Determine what the useful categories are to attach to your Activity
2) If they already exist in our Category Registry (UPDATE WITH LINK), your Activity Template should simply reference them either by their name or their id, using the existing categories element, but populating it with simple key value pairs:
"categories" : [
      "id" : "3",
      "name" : "Forwarders" 
] 

