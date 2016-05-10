# CONFIGURATION CONTROLS

When a Terminal needs to expose UI to a user to enable configuration of an Activity, it defines the UI using a set of predefined json-driven widgets. This control JSON is packed into a Crate of [Manifest: Standard UI Controls](https://maginot.atlassian.net/wiki/display/SH/Manifest%3A+Standard+UI+Controls), which is then added to the [Activity’s CrateStorage](https://maginot.atlassian.net/wiki/display/SH/Data+Model%3A+ActionDO).

Fr8-compliant client applications know how to process this json and render the correct on-screen UI. When the user fills in values, such as by typing text into a text field, the values are added to the JSON (typically into elements called “value”) and saved back into the same Crate. Later, at Run-Time, the Terminal will have access to these values.

## Control Definitions

### Defined Fields

Field |	Is required? |	 Details
--- | --- | ---
type |	yes |	one of the values shown below
name |	yes |	should be unique among the controls added to a single Crate.
label |	no |	intended for friendly ui-visible labels. If not present, the client will use the name field
required |	yes |	true or false. if set to true, the client or server will validate that a value has been set before allowing the field
events |	no |	allows the plugin to request that action take place upon events triggering. Discussed [here](https://maginot.atlassian.net/wiki/display/SH/Supported+Configuration+Control+Events).
source |	no |	instructs some controls where to find data.
showDocumentation |	no |	 used to provide documentation for activities
errorMessage |	no |	![ErrorMessage](https://github.com/Fr8org/Fr8Core/blob/master/Docs/img/ErrorMessage.png) used to ward/inform a user

 

### Defined Controls

The complete list of 22 controls currently used is presented below.

#### TextBox 	
```javascript
{
      "type": "TextBox",
      "name": "Address",
      "required": false,
      "selected": false,
      "value": null,
      "label": "Address",
      "events": [
        {
          "name": "onSelect",
          "handler": "requestConfig"
        }
      ]
}
```   
#### Checkbox   
```javascript
{
      "type": "CheckBox",      
      "name": "Event_Envelope_Sent",
      "value": null,
      "label": "Envelope Sent",
      "events": [
        {
          "name": "onSelect",
          "handler": "requestConfig"
        }
      ]
}
```   
#### DropDownList  
```javascript
{
       "type": "DropDownList",      
       "listItems": [
        {
          "selected": true,
          "key": "Item key 1",
          "value": "Item value 1"
        },
        {
          "selected": false,
          "key": "Item key 2",
          "value": "Item value 2"
        },
        {
          "selected": false,
          "key": "Item key 3",
          "value": "Item value 3"
        }
      ],
      "name": "target_docusign_template",
      "required": true,
      "value": null,
      "label": "target_docusign_template",
      "events": [
        {
          "name": "onSelect",
          "handler": "requestConfig"
        }
      ],
      "source": {
        "manifestType": "Standard Design-Time Fields",
        "label": "Available Templates"
      }
}
```   
#### Button
```javascript
{
      "type": "Button",
      "name": "Continue",
      "required": false,
      "value": null,
      "label": "Prepare Mail Merge",
      "clicked": false,
      "events": [
        {
          "name": "onClick",
          "handler": "requestConfig"
        }
      ]
}
```
#### [RadioButtonsGroup](https://maginot.atlassian.net/wiki/display/SH/Control%3A+RadioGroup)
```javascript
{
      "type": "RadioButtonGroup",
      "groupName": "Recipient",
      "name": "Recipient",
      "value": null,
      "label": "Recipient",
      "source": null,
      "radios": [
        {
          "selected": true,
          "name": "specific",
          "value": "This specific value",
          "controls": [
            {
              "listItems": [ ],
              "name": "Select Upstream Crate",
              "required": false,
              "value": null,
              "label": "",
              "type": "DropDownList",
              "selected": false,
              "source": {
                "manifestType": "Standard Design-Time Fields",
                "label": "Upstream Plugin-Provided Fields"
              }
            }
          ]
        },
        {
          "selected": false,
          "name": "crate",
          "value": "A value from an Upstream Crate",
        }
      ]
}
```
#### FieldList
```javascript
{
      "type": "FieldList",      
      "name": "Selected_Fields",
      "required": true,
      "selected": false,
      "value": [
          {"Key":"",
           "Value":""
          }
       ],
      "label": "Fill the values for other actions",
      "events": [
        {
          "name": "onSelect",
          "handler": "requestConfig"
        }
      ]
}
```
[Control: Field List](https://maginot.atlassian.net/wiki/display/SH/Control%3A+Field+List)

#### [FilterPane](https://maginot.atlassian.net/wiki/display/SH/Filter+Control)
```javascript
{ 
    "type": "FilterPane", 
    "name": "Selected_Filter", 
    "label": "Execute Actions If:", 
    "source": { 
         "manifestType": "Standard Design-Time Fields", 
         "label": "Queryable Criteria" 
              } 
}
```
a widget that produces UI which generates a query. Looks like this: ![FilterPane](https://github.com/Fr8org/Fr8Core/blob/master/Docs/img/FilterPane.png)

#### TextBlock
```javascript
{
      "type": "TextBlock",      
      "class": "well well-lg",
      "value": "This Action doesn't require any configuration.",
      "label": "DocuSign Envelope"
}
```
#### FilePicker
```javascript
{
      "name": "file_picker", 
      "type": "FilePicker"
}
```
[Create File Upload Control for Dockyard Frontend](https://maginot.atlassian.net/wiki/display/SH/Create+File+Upload+Control+for+Dockyard+Frontend)

#### TextArea   
This is a multiline text field that is generated using the [TextAngular rich text directive](https://github.com/fraywing/textAngular).
See [Create new TextArea control](https://maginot.atlassian.net/wiki/display/SH/Create+new+TextArea+control)
```javascript
{
	"type": "TextArea",
	"name": "Body",
        "label": "Body"
	"isReadOnly": false,
}
```
#### Duration
```javascript
{ 
         "days": 0, 
         "hours": 1, 
         "minutes": 59, 
         "innerLabel":"Wait this long",
         "name": "TimePeriod", 
         "required": false, 
         "value": "00:00:00", 
         "label": "After you send a Tracked Envelope, Fr8 will wait.", 
         "type": "Duration" 
}
```
![Duration](https://github.com/Fr8org/Fr8Core/blob/master/Docs/img/Duration.png)

[Configuration Control – Duration](https://maginot.atlassian.net/wiki/display/SH/Configuration+Control+-+Duration)

#### TextSource	 
A complex control enabling user either to type specific text or to select a value source from an upstream crate.
![TextSource](https://github.com/Fr8org/Fr8Core/blob/master/Docs/img/TextSource.png)
```javascript
{ 
    "initialLabel": "For the Email Address Use", 
    "valueSource": null, 
    "name": "Recipient", 
    "required": false,
    "selected": false,
    "value": null, 
    "type": "TextSource", 
    "listItems": []
    "source": { 
        "filterByTag": null,
        "manifestType": "Standard Design-Time Fields", 
        "label": "Upstream Plugin-Provided Fields" 
    } 
}
```
**valueSource** property will contact user’s selected option: either specific or upstream.The value property will contain either    **specific** text or a value selected in the drop down list.   

 

#### MappingPane
```javascript
{
      "type": "MappingPane",      
      "name": "Selected_Mapping",
      "label": "Configure Mapping"
}
```
#### QueryBuilder
```javascript
{
           "errorMessage": null,
           "events": [],
           "label": "Meeting which conditions?",
           "name": "SelectedQuery",
           "required": true,
           "selected": false,
           "showDocumentation": null,
           "source": {
                 "manifestType": "Standard Query Fields", 
                 "label": "Queryable Criteria", 
                 "filterByTag": null
              },
          "type": "QueryBuilder",
          "value": "[]"
}
```
#### RunRouteButton
```javascript
{
           "errorMessage": null,
           "events": [],
           "label": "Run Plan",
           "name": "RunRoute",
           "required": false,
           "selected": false,
           "showDocumentation": null,
           "source": null,
          "type": "RunRouteButton",
          "value": null
}
```
#### DatePicker
```javascript
{
           "errorMessage": null,
           "events": [],
           "label": "null",
           "name": "QueryField_CreateDate",
           "required": false,
           "selected": false,
           "showDocumentation": null,
           "source": null,
          "type": "DatePicker",
          "value": null
}
```
#### CrateChooser
```javascript
{
           "crateDescription": []
           "errorMessage": null,
           "events": [],
           "label": "This Loop will process the data inside of",
           "name": "Available_Crates",
           "required": false,
           "selected": false,
           "singleManifestOnly": true,
           "showDocumentation": null,
           "source": null,
           "type": "CrateChooser",
           "value": null
}
```
#### Routing	
```javascript
{
           "errorMessage": null,
           "events": null,
           "label": "Manager Plan",
           "name": "ManageRoute",
           "required": false,
           "selected": false,
           "showDocumentation": null,
           "source": null,
           "type": "Routing",
           "value": null
}
```
#### ManagerRoute
```javascript
{
           "errorMessage": null,
           "events": null,
           "label": "Manager Plan",
           "name": "ManageRoute",
           "required": false,
           "selected": false,
           "showDocumentation": null,
           "source": null,
           "type": "ManagerRoute",
           "value": null
}
```
#### UpstreamDataChooser
```javascript
{
           "errorMessage": null,
           "events": [
           {
              "name":"onChange",
              "handler": "requestConfig"
           }
           ],
           "label": "Display which table?",
           "name": "ReportSelector",
           "required": false,
           "selected": false,
           "selectedManifest": null,
           "showDocumentation": null,
           "source": null,
           "type": "UpstreamDataChooser",
           "value": null
}
```
#### UpstreamFieldChooser	 
In development	
#### UpstreamCrateChooser	
```javascript
{
           "multiSelection": true,
           "errorMessage": null,
           "events": [],
           "label": "Store wich Crates?",
           "name": "UpstreamCrateChooser",
           "required": false,
           "selected": false,
           "selectedCrates": [],
           "showDocumentation": null,
           "source": null,
           "type": "UpstreamCrateChooser",
           "value": null
}
```
#### ControlList    
When an activity needs N set of controls it should use ControlList.   
Let’s assume your activity needs two TextBoxes named “key” and “value”. ControlList allows user to add more than one key-value pair.   
To do this one should create a control list and configure it’s template. Template accepts a list of ControlDefinitionDTO. every added control to template will be structured as a whole. when user click add controlGroup button ControlList will add one of each control in template.In our example we need to add two textboxes with key and value names to template.
```javascript
{
	 "controlGroups": [ 
		[{"type":"TextBox","name":"key"..},{"type":"TextBox","name":"value"..}],
		[{"type":"TextBox","name":"key"..},{"type":"TextBox","name":"value"..}]
	 ],
	 "templateContainer": {
		 "template": [
			 { "type": "TextBox", "name": "key" ... },
			 { "type": "TextBox", "name": "value" ...}
		 ],
		 "name": "key_value_pair"
	 },
	 "addControlGroupButtonText": "Add Key/Value Pair",
	 "noDataMessage": "No field is selected",
	 "name": "kay_value_pairs",
	 "required": false,
	 "value": null,
	 "label": "Select key-value pairs",
	 "type": "ControlList"
 }
 ```
Nesting is also supported.

## Supported Events

[Supported Configuration Control Events](https://github.com/Fr8org/Fr8Core/blob/master/Docs/ForDevelopers/ConfigurationControlEvents.md) 
