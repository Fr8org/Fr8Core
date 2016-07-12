### Responding to the /configure Request

A detailed introduction to Activity Configuration can be found [here.](https://github.com/Fr8org/Fr8Core/blob/master/Docs/ForDevelopers/OperatingConcepts/ActivityConfiguration.md). 

In this guide we'll go over a practial example showing how activity configuration can be implemented. 
When a user adds one of your Terminal's Activities to their Plan, the Hub will POST to the **/activities/configure** endpoint for your Terminal: 

		http://terminal.com/activities/configure


Here is what the body of the first configuration request to your Activity will look like:

```javascript
{  
   "ActivityDTO":{  
      "Label":null,
      "Name":"My first activtiy",
      "activityTemplate":{  
         "id":"87ab869a-9573-4554-b5b1-4bcaea7064a9",
         "name":"My_fist_activity",
         "label":"My first activtiy",
         "version":"1",
         "terminal":{  
            "name":"MyTerminal",
      		"label":"My Teriminal",
      		"version":"1",
      		"endpoint":"http://terminal.com",
         },
         "tags":null,
         "category":"Processors",
         "type":"Standard",
         "minPaneWidth":330,
         "needsAuthentication":false,
         "webService":{
            "name":"My Terminal",
            "iconPath":"http://terminal.com/my-terminal-icon.png"
         },
      },
      "RootPlanNodeId":"4a0e2fa4-0422-4cc2-b308-089720f2dd5c",
      "ParentPlanNodeId":"4554d028-9955-4121-97e0-2fb9a1e40e80",
      "CurrentView":null,
      "Ordering":1,
      "Id":"8c3f4b59-a169-408e-a0aa-9b01c6faf092",
      "CrateStorage":null,
      "ChildrenActivities":[  

      ],
      "AuthTokenId":null,
      "AuthToken":{  
         "Id":null,
         "Token":null,
         "ExternalAccountId":null,
         "ExternalAccountName":null,
         "ExternalDomainId":null,
         "ExternalDomainName":null,
         "UserId":"76de71f2-f346-4bc9-96e0-f7bd1c87a575",
         "ExternalStateToken":null,
         "AdditionalAttributes":null,
         "Error":null,
         "ExpiresAt":null,
         "AuthCompletedNotificationRequired":false,
         "TerminalID":0
      },
      "documentation":null
   },
   "ContainerId":"00000000-0000-0000-0000-000000000000",
   "ExplicitData":null
}
```
The following HTTP headers will be sent with this request:
* **Fr8HubCallbackSecret**: 4b54d12f7f834648be28aa247f523e21
* **Fr8HubCallBackUrl**: http://dev.fr8.co/
* **Content-Type**: application/json; charset=utf-8
	
> **Note**: Fr8HubCallbackSecret and Fr8HubCallBackUrl headers are sent with each request from the Hub to the terminal.

Fr8HubCallbackSecret and Fr8HubCallBackUrl are discussed more below.

In the above example, CrateStorage is null, indicating that this is an initial configuration request. Often, though, the Client may send your Terminal
a followup configuration request that includes an Activity element with a populated CrateStorage.

As you may noticed the Hub sends activity template information back to you. Each terminal can manage several activities. Obviously, that each activity behaves differently. So the terminal should have a way to understand what activity should handle particular request. Information about activity template is a great way to accomplish this goal. In general, you should write some switching logic inside the terminal that will process **/activities/configure** requests differently based on activity template. In our sample, terminal has the only activty, so we need no switching logic. 
(Currently it's necessary to send back the entire ActivityTemplate, but we're working on an improvement so that only the ID need be sent.)

### Processing the Request

Activity configuration data is persisted inside the activty's JSON **CrateStorage**. Crate storage is basically an array of JSON crates. (look [here](https://github.com/Fr8org/Fr8Core/blob/master/Docs/ForDevelopers/DataModel.md) for information about crates). When activity is created, crate storage is empty. The main thing the Hub is expecting from the activity is that activity will intialize itself by filling crate storage with some data and especially some with information about UI controls. UI controls are very important because without them there is nothing to show to the user. So:
	
	Upon receiving a /configure request, you should inspect the CrateStorage.
   * If it's empty, you should carry out  **Initial configuration**, which generally means generating a Crate of UI controls and sticking it in the CrateStorage.
   * If it's not empty, you should carry out **Follow-up configuration**. (See below).

### Crating a Crate of UI Controls (Initial Configuration)
The UI Controls we generate will be rendered by the Client for the User, giving the User a chance to provide configuration information needed by the Activity.
Here we'll build a very simple UI, pack it into activity's Crate storage and return overall result to the Hub. 
Detailed information about UI controls and their JSON representation can be found [here](https://github.com/Fr8org/Fr8Core/blob/master/Docs/ForDevelopers/DevelopmentGuides/ConfigurationControls.md).
Our UI will consist of two TextBox controls. 

Here is how our UI controls look like when being packed into crate storage:
```javascript
{
    "crates": [
      {
        "manifestType": "Standard UI Controls",
        "manifestId": 6,
        "manifestRegistrar": "www.fr8.co/registry",
        "id": "{generate some GUID value here}",
        "label": "Configuration_Controls",
        "contents": {
          "Controls": [
            {
              "type": "TextBox",
              "name": "UserName",
              "required": false,
              "selected": false,
              "value": null,
              "label": "Enter your name:",
              "events": [
                {
                  "name": "onChange",
                  "handler": "requestConfig"
                }
              ]
			},
            {
              "type": "TextBox",
              "name": "GreetingText",
              "required": false,
              "selected": false,
              "value": null,
              "label": "Greeting:",
              "events": [
                {
                  "name": "onChange",
                  "handler": "requestConfig"
                }
              ]
			}
          ]
        },
      }
    ]
}
```

And here is the complete JSON that your terminal should return in respose to intial configuration request:
```javascript
{
  "label": null,
  "name": "My first activtiy",
  "activityTemplate":{  
    "id":"87ab869a-9573-4554-b5b1-4bcaea7064a9",
    "name":"My_fist_activity",
    "label":"My first activtiy",
    "version":"1",
    "terminal":{  
    "name":"MyTerminal",
    "label":"My Teriminal",
    "version":"1",
    "endpoint":"http://terminal.com",
  },
  "RootPlanNodeId":"4a0e2fa4-0422-4cc2-b308-089720f2dd5c",
  "ParentPlanNodeId":"4554d028-9955-4121-97e0-2fb9a1e40e80",
  "ordering": 1,
  "id": "1cfdba78-9a86-47bb-8bc9-2422528220ac",
  "crateStorage": {
    "crates": [
      {
        "manifestType": "Standard UI Controls",
        "manifestId": 6,
        "manifestRegistrar": "www.fr8.co/registry",
        "id": "{generate some GUID value here}",
        "label": "Configuration_Controls",
        "contents": {
          "Controls": [
            {
              "type": "TextBox",
              "name": "UserName",
              "required": false,
              "selected": false,
              "value": null,
              "label": "Enter your name:",
              "events": [
                {
                  "name": "onChange",
                  "handler": "requestConfig"
                }
              ]
			},
            {
              "type": "TextBox",
              "name": "GreetingText",
              "required": false,
              "selected": false,
              "value": null,
              "label": "Greeting:",
              "events": [
                {
                  "name": "onChange",
                  "handler": "requestConfig"
                }
              ]
			}
          ]
        },
      }
    ]
  },
  "childrenActivities": [],
}
```

Note that your activtiy should not change **id**, **ordering** **RootPlanNodeId**, **ParentPlanNodeId**. You should preserve the values that were passed to your terminal. These properties are defining the location of your activity inside the plan, that user has created:
* **id** - unique identifier of the activity. This id is assigned by the Hub when user creates new activity in the fr8.
* **ParentPlanNodeId** - This property points to the parent node for your activity. Plans are hierarchical. Activities can be children of other activities. 
* **ordering** - each activity can have more that one child. Order of activities within the parent node is very important, because it affects the ordering of exectution. This property controls ordering within the parent node. 
* **RootPlanNodeId** - this is the identifier of the plan that your activity is belong to. 

Note that the SDK's being developed for different languages all seek to provide helper methods that eliminate the need for your code to directly manipulate JSON. This is more developed in some SDK's than others, but it's always getting better. For more information, look at the docs for your particular platform. 

After completing this step, restart your Terminal and add your activity to the plan. You should see the UI with two labeled TextBoxes.


### Followup configuration

Let's add some [DesignTime](https://github.com/Fr8org/Fr8Core/blob/master/Docs/ForDevelopers/OperatingConcepts/Fr8Modes.md) logic to our activity. As it was mentioned in the previous section you should implement some simple switching logic: you should check **crateStorage** property value. If it is empty or null you should run Initial Configuration logic. What to do if **crateStorage** is not empty is the subject of this section. 

So, the user added your activity to the plan. He or she enjoyed the activity UI for a while and than decided to change some configuration value. When this has happend Hub will call **/activities/configure** endpoint again and pass information about the activity with accordingly updated values of configuration controls. Lets immagine that our user filled out his name in the corresponding text box and the name of our user is: **Dave Bowman**. Here is what the Hub will send to our terminal:


```javascript
{  
   "ActivityDTO":{  
      "Label":null,
      "Name":"My first activtiy",
      "activityTemplate":{  
         "id":"87ab869a-9573-4554-b5b1-4bcaea7064a9",
         "name":"My_fist_activity",
         "label":"My first activtiy",
         "version":"1",
         "terminal":{  
            "name":"MyTerminal",
      		"label":"My Teriminal",
      		"version":"1",
      		"endpoint":"http://terminal.com",
         },
         "tags":null,
         "category":"Processors",
         "type":"Standard",
         "minPaneWidth":330,
         "needsAuthentication":false,
         "webService":{
            "name":"My Terminal",
            "iconPath":"http://terminal.com/my-terminal-icon.png"
         },
      },
      "RootPlanNodeId":"4a0e2fa4-0422-4cc2-b308-089720f2dd5c",
      "ParentPlanNodeId":"4554d028-9955-4121-97e0-2fb9a1e40e80",
      "CurrentView":null,
      "Ordering":1,
      "Id":"8c3f4b59-a169-408e-a0aa-9b01c6faf092",
      "CrateStorage": {
          "crates": [
            {
              "manifestType": "Standard UI Controls",
              "manifestId": 6,
              "manifestRegistrar": "www.fr8.co/registry",
              "id": "{generate some GUID value here}",
              "label": "Configuration_Controls",
              "contents": {
                "Controls": [
                  {
                    "type": "TextBox",
                    "name": "UserName",
                    "required": false,
                    "selected": false,
                    "value": "Dave Bowman",
                    "label": "Enter your name:",
                    "events": [
                      {
                        "name": "onChange",
                        "handler": "requestConfig"
                      }
                    ]
                  },
                  {
                    "type": "TextBox",
                    "name": "GreetingText",
                    "required": false,
                    "selected": false,
                    "value": null,
                    "label": "Greeting:",
                    "events": [
                      {
                        "name": "onChange",
                        "handler": "requestConfig"
                      }
                    ]
                  }
                ]
              },
            }
          ]
      },
      "ChildrenActivities":[  
      ],
      "AuthTokenId":null,
      "AuthToken":{  
         "Id":null,
         "Token":null,
         "ExternalAccountId":null,
         "ExternalAccountName":null,
         "ExternalDomainId":null,
         "ExternalDomainName":null,
         "UserId":"76de71f2-f346-4bc9-96e0-f7bd1c87a575",
         "ExternalStateToken":null,
         "AdditionalAttributes":null,
         "Error":null,
         "ExpiresAt":null,
         "AuthCompletedNotificationRequired":false,
         "TerminalID":0
      },
      "documentation":null
   },
   "ContainerId":"00000000-0000-0000-0000-000000000000",
   "ExplicitData":null
}
```
Note that the **value** property of the text box with **name** *"UserName"* is containing out user's name. That's how follow-up configuration works. User makes changes and updated controls values are send to your terminal. Your terminal can read updated values and execute some logic, if it is required. For example: user fills connection string to the DB, activity reads this connection string and fills out drop-down list with tables. Sometimes it is not neccessary to perform any specific logic during the follow-up configuration. In this case your terminal should return the same data data, that was passed to the terminal with **/activities/configure** request. So the general rule for the follow-up configuration: 

	You should not re-create crates in acitivty's crate storage. You should only update their contents accordingly to your requirements.  
    
In our sample terminal we will update the value of the text box with **"name": "GreetingText"** based on user name.
 

You should write some logic in your activity that will do the following:
1. Find the crate with **label** *"Configuration_Controls"*
2. Get the Controls array from that crate.
3. Find control with **name** *"UserName"*
4. Get value of **value** property of this control. Compose greeting text using extracted value. Something like *"Hi, {user name goes here}"*.
5. Find control with **name** *"GreetingText"*
6. Update **value** property of this control with the greeting text from step 4.

In our example you should end with the following response to the Hub: 

```javascript
{
  "label": null,
  "name": "My first activtiy",
  "activityTemplate":{  
    "id":"87ab869a-9573-4554-b5b1-4bcaea7064a9",
    "name":"My_fist_activity",
    "label":"My first activtiy",
    "version":"1",
    "terminal":{  
    "name":"MyTerminal",
    "label":"My Teriminal",
    "version":"1",
    "endpoint":"http://terminal.com",
  },
  "RootPlanNodeId":"4a0e2fa4-0422-4cc2-b308-089720f2dd5c",
  "ParentPlanNodeId":"4554d028-9955-4121-97e0-2fb9a1e40e80",
  "ordering": 1,
  "id": "1cfdba78-9a86-47bb-8bc9-2422528220ac",
  "crateStorage": {
    "crates": [
      {
        "manifestType": "Standard UI Controls",
        "manifestId": 6,
        "manifestRegistrar": "www.fr8.co/registry",
        "id": "{generate some GUID value here}",
        "label": "Configuration_Controls",
        "contents": {
          "Controls": [
            {
              "type": "TextBox",
              "name": "UserName",
              "required": false,
              "selected": false,
              "value": "Dave Bowman",
              "label": "Enter your name:",
              "events": [
                {
                  "name": "onChange",
                  "handler": "requestConfig"
                }
              ]
			},
            {
              "type": "TextBox",
              "name": "GreetingText",
              "required": false,
              "selected": false,
              "value": "Hi, Dave Bowman",
              "label": "Greeting:",
              "events": [
                {
                  "name": "onChange",
                  "handler": "requestConfig"
                }
              ]
			}
          ]
        },
      }
    ]
  },
  "childrenActivities": [],
}
```

You can test your updated activity. If everything is working correctly, after you enter your name or name of your friend and once *Enter your name* text box loose its focus, you should see updated *Greeting*.
