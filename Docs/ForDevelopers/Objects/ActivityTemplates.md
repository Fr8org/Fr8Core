# ACTIVITY TEMPLATES

[Go to Contents](https://github.com/Fr8org/Fr8Core/blob/master/Docs/Home.md)  

ActivityTemplates are provided by Terminals in response to requests by Hubs. They instruct the Hub as to each Activity that a Terminal is capable of processing.
```javascript
 "template": {
        "id": "12345678-90ab-cdef-1234-567890abcdef",
        "name": "ConnectToSql",
        "label": "Connect To Sql",
        "version": "1",
        "categories": [
            {
                "iconPath": "/Content/icons/forward-icon-64x64.png", 
                "name": "Forward"
            }, 
            {
                "iconPath": "/Content/icons/web_services/twitter-icon-64x64.png", 
                "name": "Twitter"
            }
        ],
        "webService": {
            "id": 1,
            "name": "Built-In Services",
            "iconPath": "/Content/fr8core/icon-64x64.png"
        },
        "terminal" : {
              "name" : "terminalFr8Core",
              "version" : "1",
              "terminalStatus": 1,
              "endpoint" : "terminals.fr8.co:50705"
              "description": "Fr8 Core Activities",
              "authenticationType":1
         }
         "tags": null,
         "category": "Processors",
         "type": "Standard",
         "minPaneWidth": 0,
         "needsAuthentication": false,
   },
```
##### id

Identifier of this activity template. Identifiers are created as guids.

##### name

Activity template name

##### label

Pretty printed name of this activity template. This part is used as a header on PlanBuilder.

##### version

Activity template version

##### categories

See [Activity Categories](/Docs/ForDevelopers/Objects/Activities)

##### terminal

Owner terminal of this activity

##### type

There are three defined types for Activities. The three types are:

##### Type	Description
Standard	These are regular activities which perform single task
Loop	This is a custom activity type reserved for fr8 internal usage.
Solution	These activities create ready to use tasks by using other activities. They generally merge other activities together and create a meaningful plan.
minPaneWidth

This property is used to display activity on PlanBuilder. It defines minimun width of activity.

##### needsAuthentication

This property shows PlanBuilder if this activity needs to be authenticated to the 3rd party system before using it.

##### category

There are four defined categories for Activities. These are used to divide Activities into logical groupings that make sense to users. The four categories are:

Category |	Description	
--- | ---
Monitors |	Activities that wait for an external event notification to arrive	
Receivers |	Also known as “Getters”. Activities that attempt to load or acquire a piece of data from an external source	
Processors |	Activities that focus on processing and transforming Container data at run-time. They are not forbidden from connecting to external sources, but they’re encouraged to focus on carrying out a single transform of the payload data	
Senders |	Activities that push or send data to an external service or another Fr8 Container/Plan

Please note, this attribute might get replaced with "categories" attribute in nearest future.

##### tags

the Tags element is processed as a comma-delimited string. The following values are used by The Fr8 standard:

Tag Name |	Result	
--- | ---
Notifier |	Some Activities want to provide the user with a list of mechanisms for receiving a notification. A typical list might include SMS, Email, HipChat, Slack, Yammer, Chatter. These Activities can filter the available activities to only show those tagged Notifier. An Activity using this Tag should have, as its primary function, a mechanism to notify a user of an event.	
 AggressiveReload |	If present, then each time the client receives a response from a configure call for another action, it will call this activity’s configure action again. Should be used sparingly. In general, well-designed activities do not require this.	
Table Data Generator |	Indicates that an activity generates table data. Used by other activities to query for all activities that generate tables.	
Getter |	Getter activities are activities that fetch an object of data, such as an Excel File, a Google Sheet, or a Quickbooks Online Invoice.

[Go to Contents](https://github.com/Fr8org/Fr8Core/blob/master/Docs/Home.md)  
