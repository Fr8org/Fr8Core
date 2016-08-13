# Design-Time Signaling of Activity Data 
[Go to Contents](https://github.com/Fr8org/Fr8Core/blob/master/Docs/Home.md) 

Signaling is a process that takes place in Design mode. The Terminal signals the types of data that an Activity will generate when executed in Run mode, so that the user can configure connections between Activities.

For example, suppose a User is building a Plan that will notify him via Slack message every time certain Google Form is submitted.

## Singaling about the available data

Lets see how crate signaling is performed by Monitor Google Form activity. After user selects a specific form in Monitor Google Form activity UI, the following sequence happens:

1. The Hub sends **/configure** request for Monitor Google Form activity.
2. Monitor Google Form connects to Google, reads the field names of the selected Google form and puts them into a crate of Crate Description and add this crate to activity's crate storage.

It very important to understand that **all** this process is happening during the Design-Time. No activity has been executed yet. The selected Google Form hasn't been filled and submitted by someone. The only information we have at this point is how many fields the selected form has and what are names of these fields. 

Lets see crate of Crate Description is looks like. Suppose that the user selected this form in Monitor Google Form activity UI:  
![Google Form](/Docs/img/Signaling.GoogleForm.png)

After Monitor Google Form has processed **/configure** request this is how its crate storage looks like (non relevant crates are represented in the reduced form for clarity):  
```JavaScript
 [
  {
	"manifestType": "Standard UI Controls",
	"label": "Configuration_Controls",
	"contents": {}
  },
  {
	"manifestType": "Standard Event Subscription",
	"label": "Standard Event Subscriptions",
	"contents": { }
  },
  {
	"manifestType": "Crate Description",
	"manifestId": 32,
	"manufacturer": null,
	"manifestRegistrar": "www.fr8.co/registry",
	"id": "8a61601a-fb43-47ce-a7ac-f2f450f66f89",
	"label": "Runtime Available Crates",
	"contents": {
	  "CrateDescriptions": [
		{
		  "manifestId": 5,
		  "manifestType": "Standard Payload Data",
		  "label": "Google Form Payload Data",
          "sourceActivityId": "94617d96-32f6-4e28-a095-5b9e1d04820e"
		  "producedBy": "Monitor_Form_Responses",
		  "availability": 2,
		  "fields": [
			{
			  "key": "Full Name",
			  "label": "Full Name",
     		},
			{
			  "key": "TR ID",
			  "label": "TR ID",
			},
			{
			  "key": "Email Address",
			  "label": "Email Address",
			},
			{
			  "key": "Period of Availability",
			  "label": "Period of Availability",
			}
		  ]
		}
	  ]
	},
	"parentCrateId": null,
	"createTime": "",
	"availability": null,
	"sourceActivityId": null
  }
]
```

Crate Description allows activity to signal about both fields and crates that will be available in Run mode. Each Crate Description can describe several crates that will be available. Fields and related manifests are tightly coupled: field can exist only within manifest. So, for each signaled crate activity should provide list of available fields.

Lets take a closer look at the most important elements of **CrateDescriptions** content:  
1.  We can see the only object inside **CrateDescription** array, so after being executed Monitor Google Form will put the single crate into the container.
2. **manifestId** and **manifestType** are telling that this crate will be of type **Standard Payload Data**.
3. **label** tells that this crate will have label *"Google Form Payload Data"*
4. **sourceActivityId** tells about the **Id** of the activity that puts this crate. 
5. List of **fields** describes what fields will be stored inside that **Standard Payload Data**.

Now lets looks at the container payload after user submitted the form and Monitor Google Form processed notification from Google (non relevant crates are represented in the reduced form for clarity):
```javascript
{
   "crates":[
      {
         "manifestType":"Standard Event Report",
	     "contents": {}

      {
         "manifestType":"Operational State",
	     "contents": {}
      },
      {
         "manifestType":"Standard Payload Data",
         "manifestId":5,
         "manufacturer":null,
         "manifestRegistrar":"www.fr8.co/registry",
         "id":"dc4b36b0-8399-49d8-8ded-ecd4f7295a63",
         "label":"Google Form Payload Data",
         "contents":{
            "Name":null,
            "PayloadObjects":[
               {
                  "PayloadObject":[
                     {
                        "key":"Full Name",
                        "value":"Dave Bowman",
                        "tags":null
                     },
                     {
                        "key":"TR ID",
                        "value":"",
                        "tags":null
                     },
                     {
                        "key":"Email Address",
                        "value":"dave@discovery.com",
                        "tags":null
                     },
                     {
                        "key":"Period of Availability",
                        "value":"",
                        "tags":null
                     }
                  ]
               }
            ],
            "ObjectType":"Unspecified"
         },
         "parentCrateId":null,
         "createTime":"",
         "availability":null,
         "sourceActivityId":"94617d96-32f6-4e28-a095-5b9e1d04820e"
      }
   ]
}
```

You may notice that the container payload has the crate that was described in **Crate Description** of Monitor Google Form activity:  

1. It has label "Google Form Payload Data"
2. It is of type "Standard Payload Data"
3. Each **PayloadObject** has key-value pairs with keys matching the fields from **Crate Description**.


## Consuming information about available data in other activities

When the user goes to configure the Publish To Slack activity to use data from upstream activities using TextSource control: 

![Google Form](/Docs/img/Signaling.ConfiguringSlack.png)

the Fr8 client makes "available upstream data" request to the Hub using **/api/v1/plan_nodes/signals** endpoint. To process this request the Hub looks through crate storages of all upstream activities for Publish To Slack and collect all Crate Description crate from them to build one general list of available crates for Publish To Slack activity. 

> **Note**:  TextSource is not the only control that can be used to select data from upstream. There is also CrateChooser that allows to select entire crates from upstream. But all these controls are using the same approach for getting information about available data.

This is how the Hub response looks like:
```javascript
{
  "availableCrates": [
    {
      "manifestId": 5,
      "manifestType": "Standard Payload Data",
      "label": "Google Form Payload Data",
	  "sourceActivityId": "94617d96-32f6-4e28-a095-5b9e1d04820e"
      "producedBy": "Monitor_Form_Responses",
      "selected": false,
      "availability": 2,
      "fields": [
        {
          "key": "Full Name",
          "label": "Full Name",
          "fieldType": null,
          "isRequired": false,
        },
        {
          "key": "TR ID",
          "label": "TR ID",
          "fieldType": null,
          "isRequired": false,
        },
        {
          "key": "Email Address",
          "label": "Email Address",
          "fieldType": null,
          "isRequired": false,
        },
        {
          "key": "Period of Availability",
          "label": "Period of Availability",
          "fieldType": null,
          "isRequired": false,
        }
      ]
    }
  ]
}
```

Note that this information is nearly identical to what Monitor Google Form put into its Crate Description crate.

After user selects appropriate field in the UI, information about this field will be stored in Publish To Slack activity Configuration Controls crate (in the following example non relevant UI controls are omitted):
```javascript
{
	"manifestType": "Standard UI Controls",
	"manifestId": 6,
	"manufacturer": null,
	"manifestRegistrar": "www.fr8.co/registry",
	"id": "d4ebce32-178d-48bf-af83-edf19749ffa3",
	"label": "Configuration_Controls",
	"contents": {
	  "Controls": [
		{
		  "initialLabel": "Message",
		  "upstreamSourceLabel": null,
		  "textValue": null,
		  "valueSource": "upstream",
		  "groupLabelText": "",
		  "HasValue": true,
		  "HasUpstreamValue": true,
		  "HasSpecificValue": false,
		  "ValueSourceIsNotSet": false,
		  "listItems": [],
		  "selectedKey": "Full Name",
		  "hasRefreshButton": false,
		  "selectedItem": {
			"key": "Full Name",
			"label": "Full Name",
			"fieldType": null,
			"isRequired": false,
			"availability": 2,
			"sourceCrateLabel": "Google Form Payload Data",
			"sourceActivityId": "94617d96-32f6-4e28-a095-5b9e1d04820e"
		  },
		  "name": "MessageSource",
		  "required": false,
		  "value": "Full Name",
		  "label": null,
		  "type": "TextSource",
		  "selected": false,
		  "events": [
			{
			  "name": "onChange",
			  "handler": "requestConfig"
			}
		  ],
		  "source": {
			"manifestType": "Field Description",
			"label": "",
			"filterByTag": "",
			"requestUpstream": true,
			"availabilityType": 2
		  },
		  "showDocumentation": null,
		  "isHidden": false,
		  "isCollapsed": true
		}
	  ]
	},
	"parentCrateId": null,
	"createTime": "",
	"availability": 1,
	"sourceActivityId": null
}
```

Take look at **selectedItem** property of *Message* TextSource. You can see, how Publish To Slack is instructed to get data from upstream. No actual value for "Full Name" 
is specified. Instead an exact address of the required data within container payload is set: crate label, activityId, field name. When Publish To Slack is being executed the Hub will use this address to extract corresponding data from the container payload and supply extracted data for the activity.

Such approach allows activities to remain completely decoupled while still enabling easy user mapping of data from one activity to the next.

[Go to Contents](https://github.com/Fr8org/Fr8Core/blob/master/Docs/Home.md) 
