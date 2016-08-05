# Validation
[Go to Contents](/Docs/Home.md)

## Summary

Every time when Hub configures activity or activates it prior to running a plan which results in respective calls to terminal's `activities\configure` and `activities\activate` endpoints, activity gets a chance to report Hub any validation errors it has.

Information about validation errors are represented in the form of **ValidationResultCM** crate. If activity wants to report about any validation errors it should place **ValidationResultCM** crate in the activity crate storage. While processing `activities\configure` or `activities\activate` responses the Hub will check for presence of **ValidationResultCM** crate. If this crate is found the Hub will instruct the client to display validation errors in the UI. In addition, if validation errors are returned within `activities\activate` response the Hub will stop related plan activation that effectively prevents faulty plan to execute. See more about plan activation and execution [here](/Docs/ForDevelopers/Objects/PlansActivationAndRunning.md).

## ValidationResultCM manifest structure

Fr8 supports several types of validation errors:  
* **Control** - errors that are related to the certain control's value
* **Multi-controls** - errors spanning for several controls at time (cross-checks between controls value)
* **Activity wide** - error that are related to the entire activity in its current state

Any number of errors of any type and in any combinations can be reported using **ValidationResultCM**. Here is an example content of **ValidationResultCM**:  

```javascript
 {  
      "validationErrors":[  
         {  
            "controlNames":[  
               "Control1",
			   "Control2"
            ],
            "errorMessage":"Multi-controls validation message"
         },
		 {  
            "controlNames":[  
               "Control3"
            ],
            "errorMessage":"Control validation message"
         },
 		 {  
            "controlNames":[],
            "errorMessage":"Activity wide validation message"
         }
      ]
   }
```
**validationErrors** array contains one or more validation errors. Each validation error is represented by the JSON with the following properties:
* **errorMessage** - error message that will be displayed to the user.
* **controlNames** - the list of controls names which values are responsible for the validation error. Control names should be exactly the same that are used in **Configuration Controls** crate.
	* If this array contains the only element then the error is considered to be **Control** error.
    * If this array contains more then one element the error is considered to be **Multi-controls** error.
    * If this array contains no elements then the error is considered to be **Activity wide** error.

## Example usages

### Control error
Lets see practical example of how validation error is reported and displayed in *Publish to Slack* activity when no Message to publish was set:  

![ControlError](/Docs/img/Validation.ControlError.png)  

And here is how this activity crate storage looks like (unrelated properties are omitted for clarity):   
```javascript
{
    "crates": [
      {
        "manifestType": "Standard UI Controls",
        "manifestId": 6,
        "manufacturer": null,
        "manifestRegistrar": "www.fr8.co/registry",
        "id": "10af10ae-d369-4503-82e2-dd17c19869d0",
        "label": "Configuration_Controls",
        "contents": {
          "Controls": [
            {
              "name": "DropDownList0",
			  "type": "DropDownList",
              "value": "C07JDTU82",
              "label": "Select Slack Channel",
            },
            {
              "name": "MessageSource",
			  "type": "TextSource",
              "upstreamSourceLabel": null,
              "textValue": null,
              "valueSource": "specific",
            }
          ]
        },
        "parentCrateId": null,
        "createTime": "",
        "sourceActivityId": null
      },
      {
        "manifestType": "Validation Results",
        "manifestId": 39,
        "manufacturer": null,
        "manifestRegistrar": "www.fr8.co/registry",
        "id": "a230e4ec-1db5-43c1-8553-eb4a637be05d",
        "label": "Validation Results",
        "contents": {
          "validationErrors": [
            {
              "controlNames": [
                "MessageSource"
              ],
              "errorMessage": "Can't post empty message to Slack"
            }
          ]
        },
        "parentCrateId": null,
        "createTime": "",
        "sourceActivityId": null
      }
    ]
}
```

## Dealing with upstream values

It is important to remember that some controls, like **TextSource** can has two possible sources of value:  
* Explicit value that was typed by a user
* Upstream value - information (crate label, field key, etc) that tells how to resolve the value from the container crate storage during execution.

Validation logic should take care of these two possible cases. In case of upstream value source the only thing that can be validated is the fact that some information about run-time value resolution has been set. You can't validate the value itself, because it is not available during the design time. Lets see how it looks like:  

Here is an example of the TextSource that has explicit value set (unrelated properties are omitted for clarity):  
```javascript
{
  "selectedItem": null,
  "textValue": "Explicit value",
  "valueSource": "specific"
}
```

Note that **textValue** is not empty and contains the value that was typed by the user and **valueSource** has value "specific".  

Here is an example of the TextSource that has upstream value set (unrelated properties are omitted for clarity):   
```javascript
{
  "textValue": null,
  "valueSource": "upstream",
  "selectedItem": {
	"key": "text",
	"label": "text",
	"fieldType": null,
	"isRequired": false,
	"availability": 2,
	"sourceCrateLabel": "Slack Message",
	"sourceActivityId": "57ab1ae4-83b9-42d0-b0dd-c9db1986e144"
  }
}
```
Note that **textValue** is empty but **valueSource** has value "upstream" and **selectedItem** contains information about the upstream.