
[Return to Terminal Development Guide Home](https://github.com/Fr8org/Fr8Core/blob/master/Docs/ForDevelopers/DevelopmentGuides/TerminalDevelopmentGuide.md)

[Choosing a Development Approach](https://github.com/Fr8org/Fr8Core/blob/master/Docs/ForDevelopers/DevelopmentGuides/ChoosingADevelopmentApproach.md)

Terminal can be written using virtually any platfrom that supports proccessing of HTTP requests. Fr8 communication protocol is JSON based, so your platfrom must be capable of working with JSON. 

[Making Your Terminal Visible on the Public Web](https://github.com/Fr8org/Fr8Core/blob/master/Docs/ForDevelopers/DevelopmentGuides/PublicVisibility.md)

	
For the rest of this document we will assume that you've published the terminal using endpoint: 

	http://terminal.com
    
### Getting ready to be discovered
For Fr8 the terminal is mainly a container with activities. The first thing the Hub will do when you register new terminal is sending HTTP GET request to it's **/discover** endpoint:

	http://terminal.com/discover
    
In the response to this request your terminal must return information about the terminal itself and acitivies this terminal is managing. Here is an example of response your terminal should return in response to **/discover** request:
```javascript
{
   "Definition":{
      "id":"{generate some GUID value here}",
      "name":"MyTerminal",
      "label":"My Teriminal",
      "version":"1",
      "endpoint":"http://terminal.com",
      "authenticationType":1
   },
   "Activities":[
      {
         "name":"My_fist_activity",
         "label":"My first activtiy",
         "version":"1",
         "webService":{
            "name":"My Terminal",
            "iconPath":"http://terminal.com/my-terminal-icon.png"
         },
         "Category":"Processors",
         "Type":"Standard",
         "minPaneWidth":330,
         "NeedsAuthentication":false,
      }
   ]
}
```

There are few important notes here:
* All properties it the above JSON are mandatory.
* You should generate some GUID that will uniquely identify the terminal. This GUID should be returned as **Definition.Id** propery value. This GUID should never change over the time. GUID is represented by 32 hexadecimal digits separated by hyphens: 00000000-0000-0000-0000-000000000000.
* Asign **Definition.name** and **Definition.label** to anything you want. Note, that **Definition.label** is the text that is shown to users in fr8 UI in activity selection pane.
* **Definition.version** is needed for your terminal evolution. Set it to "1" at the beginning. 
* **Definiton.endpoint** is very important property. This property should represent the address of the termninal's publically accessible HTTP endpoint. 
* **Definition.authenticationType** defines what kind of authentication your terminal is going to use. We will not use authentication for now. Read about possible values here [link to the page describing terminal authentication]
* For each activity you have to supply **name** and **label**. Name should be unique across your terminal. Label is shown to users in fr8 UI in activity selection pane. 
* Activtiy **version** is needed for your activity evolution. Set it to "1" at the beginning.</i>
* Don't forget to supply **webService** information.
* **iconPath** should be an absolute URL. 
* **NeedsAuthentication** flag shows if your particular activity wants to use authentication. 

Consult [Activity Templates specs](https://github.com/Fr8org/Fr8Core/blob/master/Docs/ForDevelopers/Objects/ActivityTemplates.md) for more information on possible properties value.

After you have correctly implemented response to **/discover** request you are ready to be discovered by the Hub. Let's register your terminal now!

### Terminal registration
Now you can go to [dev.fr8.co](http://dev.fr8.co) and follow the instructions here [link to the page with terminal registration instructions]. For registration process you must use exactly the same endpoint that your terminal returns in the response to **/discover** request.

After terminal registration succeeds you can open Plan Builder. Your new activity can be found in activity selection pane. You can even try to add your activity to the plan, but unless you complete some additional steps, activity configuration will fail. 


### Making your activity configurable

Detailed explanation of how activtiy configuration is wokring can be found [on this page](https://github.com/Fr8org/Fr8Core/blob/master/Docs/ForDevelopers/OperatingConcepts/ActivityConfiguration.md). In this section we'll discuss practial example how activity configuration can be implemented. When user adds user activity to the plan, the Hub will send HTTP POST request to **/activities/configure** endpoint for the corresponding terminal: 

		http://terminal.com/activities/configure


Here is how the body of the first configuration request to your activity will look like:
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

We will discuss the meaning of Fr8HubCallbackSecret and Fr8HubCallBackUrl headers later in this document.

As you may noticed the Hub sends activity template information back to you and here is the reason why. Each terminal can manage several activities. Obviously, that each activity behaves differently. So the terminal should have a way to understand what activity should handle particular request. Information about activity template is a great way to accomplish this goal. In general, you should write some switching logic inside the terminal that will process **/activities/configure** requests differently based on activity template. In our sample, terminal has the only activty, so we need no switching logic. 

Lets discuss what the Hub is expecting from the terminal to do now. 


You should know, that activity configuration is stored inside activty's **CrateStorage**. Crate strorage is basically an array of crates. (look [here](https://github.com/Fr8org/Fr8Core/blob/master/Docs/ForDevelopers/DataModel.md) for information about crates). When activity is created, crate storage is empty. The main thing the Hub is expecting from the activity is that activity will intialize itself by filling crate storage with some data and especially some with information about UI controls. UI controls are very important because without them there is nothing to show to the user. So:
	
   * If activity receives empty crate storage during /configuration request it should fill crate storage with some initial data including UI controls. This is called **Initial configuration**
   * If activity recieves non empty crate storage during /configration call it is called **Follow-up configuration**. We'll discuss it later.

For the intial configuration we are going to build a very simple UI, pack it into activity's crate storage and return overall result to the Hub. Detailed information about UI controls and their JSON representation can be found [here](https://github.com/Fr8org/Fr8Core/blob/master/Docs/ForDevelopers/DevelopmentGuides/ConfigurationControls.md). Our UI will consist of two TextBoxes control. 

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

Note that generally your activtiy should not change **id**, **ordering** **RootPlanNodeId**, **ParentPlanNodeId**. You should preserve the values that were passed to your terminal. These properties are defining the location of your activity inside the plan, that user has created:
* **id** - unique identifier of the activity. This id is assigned by the Hub when user creates new activity in the fr8.
* **ParentPlanNodeId** - Actually plan has tree-like structure. Activities can be placed as children of other activities. This property points to the parent node for your activity.
* **ordering** - each activity can have more that one child. Order of activities within the parent node is very important, because it affects the ordering of exectution. This property controls ordering within the parent node. 
* **RootPlanNodeId** - this is the identifier of the plan that your activity is belong to. 

After completing this step you can try to add your activity to the plan. You should see the UI with two labeled TextBoxes.


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


### Making the activity runnable

Activity execution is a bit more complex than configuration. There are three endpoints you have to implement to make activity ready for execution:
1. Activation endpoint: **/activities/activate**
2. Execution endpoint: **/activities/Run**
3. Deactivation endpoint: **/activities/deactivate**

The general idea is the following. Before your activity is executed the Hub gives it a chance to validate configurtaion values and makes some initializations. This is called *activation*. If activation succeed, the Hub will execute activity. When user stops plan execution or activity in its current state is no longer needed, the Hub gives it a chance to make some uninitialization. This is called *deactivation*.
Delailed infomration about activation and deactivation you can find [here](https://github.com/Fr8org/Fr8Core/blob/master/Docs/ForDevelopers/Objects/PlansActivationAndRunning.md). 

You have to implement activation and deactivation endpoints in any case. If actvity has no activation and deactivation logic you can end with simple pass-trough implementation. This simple implementation is exactly what we are going to do in our sample. Lets start. 

#### Activation

When user of our activtiy will trigger related plan exectuion the Hub will send POST HTTP request to **/activities/activate** endpoint
	
	http://terminal.com/activities/activate
    
The body and the headers of the request are absolutely identical to the body and headers that Hub sends with **/activities/configure** request. The format of the response is also identical to the response your activity returns to **/activities/configure** request. General rules of processing of this request are the same as for **follow-up configuration** request processing. It means that by the default, you return the same data, that were send to you. You would never return empty response for the *activation* request. 

You may notice that *activation* is not triggered every time you run the plan. It is normal operation of the Hub. Read more about the reasons of such behavior [here](https://github.com/Fr8org/Fr8Core/blob/master/Docs/ForDevelopers/Objects/PlansActivationAndRunning.md).

#### Deactivation
In our sample **/activities/deactivate** is likely to be called only when user run the plan than change our sample activity configuration and then run the plan again. *Deactivation* will be triggered just before the first call to **/activties/configure**. *Deactivation* endpoint signature, corresponding request/response and processing rules are identical to **/activities/activate**. Our activity needs no uninitialization so just implement processing of **/activities/deactivate** in the same way you done it for **/activities/activate**.

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
