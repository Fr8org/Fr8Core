# CONFIGURATION CONTROL EVENTS
[Go to Contents](https://github.com/Fr8org/Fr8Core/blob/master/Docs/Home.md)  

Events can be attached to individual controls. This allows an action to signal to the client that an action should be taken. The most important use case of this is enabling the action to update the UI in response to an initial user action. For example, the action shows a drop down list box that gives the user a choice of apples or oranges. If it chooses to add an OnSelect event that triggers a RequestConfig behavior, then the action will be called as soon as the user makes that initial choice. The action can then add, in this case, a second control called color that is prepopulated with either “red”, “green”, “gold”   or “orange”.

A more relevant example can be seen in the Select DocuSign Envelope activity. The user is initially presented with a list of their DocuSign Templates. When they select one, the onSelect event fires, an a new call is made to the activity’s /configure endpoint. The activity promptly cracks open the template and extracts its standard and custom fields, which is returns to the user to enable mapping.

The  “Events” element consists of an array of one or more Events, each of which is a json key value pair.

Currently the only supported action that can be taken in response to a UI Control Event is “requestConfig”.

## Supported Events

Event Name | | |
--- | --- | --- 
onExitFocus	|	 | 
onSelect		| |
 
# Details

When the client  parses the Configuration crate so that it can render the UI, it looks for the presence of an “events” element in a control. If it finds one, it then reads the contents

At rendering time, the client will look for the presence of this string, if it has a value, it will try to process each element in the string. If one of the elements has a key of “onChange”, then the PB will register a callback with the field that should be triggered when focus leaves that field. (Presumably, an [ngBlur](https://docs.angularjs.org/api/ng/directive/ngBlur)) The callback should include the argument in the value of the control (in this case, it should be “requestConfig”).

[Go to Contents](https://github.com/Fr8org/Fr8Core/blob/master/Docs/Home.md)  
