#ControlList Control
When an activity needs N set of controls it should use ControlList.   
Let’s assume your activity needs two TextBoxes named “key” and “value”. ControlList allows user to add more than one key-value pair.   
To do this one should create a control list and configure it’s template. Template accepts a list of ControlDefinitionDTO. every added control to template will be structured as a whole. when user click add controlGroup button ControlList will add one of each control in template.In our example we need to add two textboxes with key and value names to template.

##Example Control Payload
```json
{
	 "controlGroups": [
		[{"type":"TextBox","name":"key"},{"type":"TextBox","name":"value"}],
		[{"type":"TextBox","name":"key"},{"type":"TextBox","name":"value"}]
	 ],
	 "templateContainer": {
		 "template": [
			 { "type": "TextBox", "name": "key"  },
			 { "type": "TextBox", "name": "value"}
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
