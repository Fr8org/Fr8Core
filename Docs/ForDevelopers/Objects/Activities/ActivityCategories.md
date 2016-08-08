Activity Categories
===================

Categories are applied to an ActivityTemplate like tags. The Activity designer can specify as many categories as make sense.

For example, 
Anyone can register a new Category with a corresponding icon. We probably have an approval workflow internally to keep it clean. We call this the Category Registry
Our instructions to Activity designers are: 
1) Determine what the useful categories are to attach to your Activity
2) If they already exist in our Category Registry, your Activity Template should simply reference them either by their name or their id, using the existing categories element, but populating it with simple key value pairs:
"categories" : [
      "id" : "3",
      "name" : "Forwarders" 
] 

Categories field specifies a list of Activity Categories to which current Activity belongs to.

By default, following four categories are available:

Category |	Description | JSON sample	
--- | --- | ---
Monitors |	Activities that wait for an external event notification to arrive |	{ "iconPath": "/Content/icons/monitor-icon-64x64.png", "name": "Monitor" }
Receivers |	Also known as “Getters”. Activities that attempt to load or acquire a piece of data from an external source | { "iconPath": "/Content/icons/get-icon-64x64.png", "name": "Get" } 	
Processors |	Activities that focus on processing and transforming Container data at run-time. They are not forbidden from connecting to external sources, but they’re encouraged to focus on carrying out a single transform of the payload data | { "iconPath": "/Content/icons/process-icon-64x64.png", "name": "Process" }
Senders |	Activities that push or send data to an external service or another Fr8 Container/Plan | { "iconPath": "/Content/icons/forward-icon-64x64.png", "name": "Forward" } 

However, it is possible to specify more than one Category for single Activity.

##### webService

WebService shows us which external service does this activity belongs to.
Please note, this attribute might get replaced with "categories" attribute in nearest future.
