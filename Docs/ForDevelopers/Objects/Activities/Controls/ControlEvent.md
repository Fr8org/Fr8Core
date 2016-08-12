#ControlEvent

The ControlEvent class is used in [ControlDefinitionDTO](../DataTransfer/ControlDefinitionDTO.md) class in order to specify a list of JavaScript events to which the control should respond.

##Fields
__name__: the name of the JavaScript event to which the control should respond. Examples are _onClick_ and _onChange_

__handler__: the name of the handler method that will be executed when this event is triggered. The handler must be defined in a JavaScript file that is loaded on the same page as the control.
