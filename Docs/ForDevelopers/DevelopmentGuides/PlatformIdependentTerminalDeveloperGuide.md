
[Return to Terminal Development Guide Home](https://github.com/Fr8org/Fr8Core/blob/master/Docs/ForDevelopers/DevelopmentGuides/TerminalDevelopmentGuide.md)

### [Choosing a Development Approach](https://github.com/Fr8org/Fr8Core/blob/master/Docs/ForDevelopers/DevelopmentGuides/ChoosingADevelopmentApproach.md)


### [Making Your Terminal Visible on the Public Web](https://github.com/Fr8org/Fr8Core/blob/master/Docs/ForDevelopers/DevelopmentGuides/PublicVisibility.md)

	
For the rest of this document we will assume that you've published the terminal using endpoint: 

	http://terminal.com
    
### [Responding to /discover Requests](https://github.com/Fr8org/Fr8Core/blob/master/Docs/ForDevelopers/DevelopmentGuides/Guide-TerminalDiscovery.md)


### [Terminal registration](https://github.com/Fr8org/Fr8Core/blob/master/Docs/ForDevelopers/DevelopmentGuides/Guide-TerminalRegistration.md)

### [Responding to /configure Requests](https://github.com/Fr8org/Fr8Core/blob/master/Docs/ForDevelopers/DevelopmentGuides/Guide-ActivityConfiguration.md)

### Making the activity runnable

Activity execution is a bit more complex than configuration. There are three endpoints you have to implement to make activity ready for execution:
1. Activation endpoint: **/activities/activate**
2. Execution endpoint: **/activities/Run**
3. Deactivation endpoint: **/activities/deactivate**
4. 
### [Responding to /activate and /deactivate](https://github.com/Fr8org/Fr8Core/blob/master/Docs/ForDevelopers/DevelopmentGuides/Guide-ActivateDeactivate.md)



#### Execution
When the Hub wants to run the activity it sends HTTP POST request to **/activities/Run** endpoint

	http://terminal.com/activities/Run
    
Here is how the body of the request looks like:

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
   "ContainerId":"4cf36775-7a8a-4b8f-9e6c-cafed895207a",
   "ExplicitData":null
}
```

The following HTTP headers will be sent with this request:
* **Fr8HubCallbackSecret**: 4b54d12f7f834648be28aa247f523e21
* **Fr8HubCallBackUrl**: http://dev.fr8.co/
* **Content-Type**: application/json; charset=utf-8

The data that is processed by the particular running plan is stored within the container in the form of crate storage. The Hub expects that activity will use and possibly modify container data upon execution. You may notice that **ContainerId** property is not an empty GUID now. We will use this value to retrive the content of the Container. Our sample activity will not modify the container data, but to sucessfully process **/Run** request we neen to return container data as-is otherwise the Hub will decide that our activtiy decided to delete all data from the container. Let's see how we can do this. To get container data we have to ask the Hub for it. To do this we need an URL of the Hub. Where can get get it? You may recall HTTP header **Fr8HubCallBackUrl** and you are right. To get the data we need to make HTTP GET request to the HUB using the following URL:
	
    http://dev.fr8.co/api/v1/containers/payload?id=4cf36775-7a8a-4b8f-9e6c-cafed895207a
    
* **/api/v1** is our API versioning prefix. We are going to use v1 API here.
* You should pass actual **ContainerId** value as **id** parameter.

Almost all Hub endpoints are secured with HMAC authorization, so you can't just make that GET request and look at the data. You have to authorize to the Hub. And this is were we need the second HTTP header -  **Fr8HubCallbackSecret**. You should use the value this header, request URL and some other data that was passed to you with the request from the Hub to build a correct HMAC header and pass this header with your request to the Hub. Here is how HMAC header built:
	
* Header name is **Authorization**
* Header values is created using the following format (without quotes): "hmac {TerminalPublicIdentifier}:{Hash}:{RandomValue}:{UnixTimeStamp}:{UserId}"
* **{TerminalPublicIdentifier}** is the value that your terminal returns during **/discover** request as **Definition.Id** property value.
* **{RandomValue}** is GUID represented by 32 hexadecimal digits separated by hyphens: 00000000-0000-0000-0000-000000000000: 
* **{UnixTimeStamp}** is the machine's UTC local time represented as Unix time stamp. Note, that your machine's time should be correctly set. 
* **{UserId}** is the value of **AuthToken.UserId** property from the request data.
* **{Hash}** hash of the message. We'll discuss its calculation separately.

Hash is computed using SHA512 cryptographic hash function (more details [here](https://msdn.microsoft.com/en-us/library/system.security.cryptography.hmacsha512(v=vs.110).aspx)).  ASCII byte representation of **Fr8HubCallbackSecret** string is used as a key for SHA512. Hash is computed for UTF8 bytes of the following string:
	
    {TerminalPublicIdentifier}{URL}{UnixTimeStamp}{RandomValue}{UserId}
    
where **{TerminalPublicIdentifier}**, **{RandomValue}**, **{UnixTimeStamp}** and **{UserId}** have the same meaning that is described above. **URL** is the URL of the request. Computed has is then represented as base64 string that is used as the part of HMAC header. Here is a an example text of required request headers:

	Authorization: hmac 2db48191-cda3-4922-9cc2-a636e828063f:/Nn+izpVCQUtd0ExslNR1pHrcg77ruGoB280DWCC+05BMAyK263+FkZrpZ15lPwoJxnQlIJ9fjcGCHoNqeK1Og==:9ad793bd-a99c-42ef-b6a9-1a653248d103:1468283631:76de71f2-f346-4bc9-96e0-f7bd1c87a575


The following data will be returned but the Hub in the response to your request:

```javascript
{
  "container": {
    "crates": [
      {
        "manifestType": "Operational State",
        "manifestId": 27,
        "manufacturer": null,
        "manifestRegistrar": "www.fr8.co/registry",
        "id": "9b0a5ca1-e3f8-45ea-8eba-9e754383c342",
        "label": "Operational state",
        "contents": {
          "CallStack": [
            {
              "NodeId": "4554d028-9955-4121-97e0-2fb9a1e40e80",
              "NodeName": "Starting subplan",
              "CurrentActivityExecutionPhase": 1,
              "CurrentChildId": "8c3f4b59-a169-408e-a0aa-9b01c6faf092",
              "LocalData": null
            },
            {
              "NodeId": "8c3f4b59-a169-408e-a0aa-9b01c6faf092",
              "NodeName": "My first activtiy",
              "CurrentActivityExecutionPhase": 0,
              "CurrentChildId": null,
              "LocalData": null
            }
          ],
          "History": [],
          "CurrentActivityErrorCode": null,
          "CurrentActivityErrorMessage": null,
          "CurrentClientActivityName": null,
          "BypassData": null,
          "CurrentActivityResponse": null
        },
        "parentCrateId": null,
        "createTime": "",
        "availability": null,
        "sourceActivityId": null
      }
    ]
  },
  "containerId": "4cf36775-7a8a-4b8f-9e6c-cafed895207a"
}
```

The most important part here is **crates** property. This is where container's crate storage stored. When your activity is the first activity in the plan, payload crate storage is likely to contain the only crate - **Operational state**. It represents current execution state of the container. Look [here](https://github.com/Fr8org/Fr8Core/blob/master/Docs/ForDevelopers/OperatingConcepts/PlanExecution.md) for in-depth explanation of the internals of plan execution.

Activity is free to put some new crates to the container's crate storage, but our sample activity will leave crate storage untouched and simply return its contents to the Hub. Response of **/activities/Run** will be identical to the the response of **http://dev.fr8.co/api/v1/containers/payload?id=4cf36775-7a8a-4b8f-9e6c-cafed895207a** request.

### Final words
In this document we discussed the process of creation of the nearly most simple terminal and the activity possible. But despite the simplicity of the resulting activtiy, we've learned about the most fundamental concepts of Fr8 terminal development and underlying architectural principles. Actual business-oriented activities and terminals wil much more complex: they have to support authentication, validation, perform complex business logic and consume the majority of the Hub API. Almost everything is possible to accomplish with the help of Fr8 powerfull API and your imagination!
