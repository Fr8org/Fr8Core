### Responding to the /configure Request

A detailed introduction to Activity Configuration can be found [here.](https://github.com/Fr8org/Fr8Core/blob/master/Docs/ForDevelopers/OperatingConcepts/ActivityConfiguration.md).

[Learn about Signaling](/Docs/ForDevelopers/OperatingConcepts/Signaling.md), a key element of Activity Configuration.

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
* **Fr8UserId**: d4991c09-77ee-42de-9ae7-15c1b6c2d3ca
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
Our UI will consist of two TextBox controls and one DropDownList. Initial configuration is also the best place to fill your controls with values retrieved from external web-services. Lets assume we'll fill the values of DropDownList with list of cities that we retrieve from some imaginary service calling its `/GET https://allthecities.com/api/cities` endpoint.

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
					},
					{
						"type": "DropDownList",      
						"listItems": [
							{
								"selected": false,
								"key": "New-York",
								"value": "NYC"
							},
							{
								"selected": false,
								"key": "London",
								"value": "LDN"
							},
							{
								"selected": false,
								"key": "Tokio",
								"value": "TKY"
							}
						],
						"name": "city",
						"required": true,
						"value": null,
						"label": "City",
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
							},
							{
								"type": "DropDownList",      
								"listItems": [
									{
										"selected": false,
										"key": "New-York",
										"value": "NYC"
									},
									{
										"selected": false,
										"key": "London",
										"value": "LDN"
									},
									{
										"selected": false,
										"key": "Tokio",
										"value": "TKY"
									}
								],
								"name": "city",
								"required": true,
								"value": null,
								"label": "City",
								"events": [
									{
										"name": "onChange",
										"handler": "requestConfig"
									}
								]
							}
						]
					}
				}
			]
		},
		"childrenActivities": []
	}
}
```

Note that your activtiy should not change **id**, **ordering** **RootPlanNodeId**, **ParentPlanNodeId**. You should preserve the values that were passed to your terminal. These properties are defining the location of your activity inside the plan, that user has created:
* **id** - unique identifier of the activity. This id is assigned by the Hub when user creates new activity in the fr8.
* **ParentPlanNodeId** - This property points to the parent node for your activity. Plans are hierarchical. Activities can be children of other activities.
* **ordering** - each activity can have more that one child. Order of activities within the parent node is very important, because it affects the ordering of exectution. This property controls ordering within the parent node.
* **RootPlanNodeId** - this is the identifier of the plan that your activity is belong to.

Note that the SDK's being developed for different languages all seek to provide helper methods that eliminate the need for your code to directly manipulate JSON. This is more developed in some SDK's than others, but its always getting better. For more information, [look at the docs for your particular platform](https://github.com/Fr8org/Fr8Core/blob/master/Docs/ForDevelopers/SDKHome.md).

After completing this step, restart your Terminal and add your activity to the plan. You should see the UI with two labeled TextBoxes.


### Followup configuration

Let's add some [Design-Time](https://github.com/Fr8org/Fr8Core/blob/master/Docs/ForDevelopers/OperatingConcepts/Fr8Modes.md) logic to our activity. As discussed above, your Terminal should inspect the **crateStorage** property value. If it is empty or null you should run Initial Configuration logic. Let's examine a scenario where its not empty.

Suppose the user added your activity to the plan. He or she returned the next day, logged in and decided to change a configuration value. When this has happend Hub will call **/activities/configure** endpoint again and pass information about the activity with accordingly updated values of configuration controls. Lets imagine that our user filled out his name in the corresponding text box and the name of our user is: **Dave Bowman**. Also he selected **London**  from the DropDownList. Here is what the Hub will send to our terminal:


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
							},
							{
								"type": "DropDownList",      
								"listItems": [
									{
										"selected": false,
										"key": "New-York",
										"value": "NYC"
									},
									{
										"selected": true,
										"key": "London",
										"value": "LDN"
									},
									{
										"selected": false,
										"key": "Tokio",
										"value": "TKY"
									}
								],
								"name": "city",
								"required": true,
								"value": "LDN",
								"selectedKey" : "London",
								"label": "City",
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
Note that the **value** property of the text box with **name** *"UserName"* is populated with the value the User entered and the **selected** property of one of the list items inside DropDownList is set to `true` and DropDownList itself have selected value stored in **value** and **selectedKey** properties. Your treatment of User-provided configuration inputs can be passive or active. In a passive situation, the User might change the value of a text field, and you might never find out about it until run-time, because the Client had no reason to make an additional `/configure` call to you.

Active treatment is used for scenarios where you want to update the UI presented to the UI in response to the user's actions. For example, they start by providing a connection string to a db. You want to force the Client to pass that to you (via another `/configure` call) so that you can connect to the DB, extract the tables and columns, and make them availble in a query widget. You can specify events that should trigger a /configure call using [Configuration Control Events](https://github.com/Fr8org/Fr8Core/blob/master/Docs/ForDevelopers/ConfigurationControlEvents.md).

Just because you receive a `/configure` call, don't feel that you have to take action. There are a bunch of other factors, outside of your control, that might cause the Client to want to update all activities by calling their `/configure` endpoints. In this case your terminal should return the same data that was passed to the terminal with **/activities/configure** request.

You should not re-create or duplicate crates in Activity CrateStorage without a good reason. In general, use DRY principles. If you're modifying a Crate of Data, modify it in place.  

In our sample terminal we will update the value of the text box with **"name": "GreetingText"** based on user name and city selected.

You should write some logic in your activity that will do the following:
1. Find the crate with **label** *"Configuration_Controls"*
2. Get the Controls array from that crate.
3. Find control with **name** *"UserName"*
4. Find control with **name** *"city"*
5. Get value of **value** property of textbox and value of **selectedKey** property of DropDownList. Compose greeting text using extracted values. Something like *"Hi, {user name goes here} from {city name goes here}"*.
6. Find control with **name** *"GreetingText"*
7. Update **value** property of this control with the greeting text from step 5.

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
								"value": "Hi, Dave Bowman from London",
								"label": "Greeting:",
								"events": [
									{
										"name": "onChange",
										"handler": "requestConfig"
									}
								]
							},
							{
								"type": "DropDownList",      
								"listItems": [
									{
										"selected": false,
										"key": "New-York",
										"value": "NYC"
									},
									{
										"selected": true,
										"key": "London",
										"value": "LDN"
									},
									{
										"selected": false,
										"key": "Tokio",
										"value": "TKY"
									}
								],
								"name": "city",
								"required": true,
								"value": "LDN",
								"selectedKey" : "London",
								"label": "City",
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
}
```

You can test your updated activity. If everything is working correctly, after you enter your name or name of your friend and once *Enter your name* text box loose its focus, you should see updated *Greeting*. The same should happen when you select different value from DropDownList with cities
