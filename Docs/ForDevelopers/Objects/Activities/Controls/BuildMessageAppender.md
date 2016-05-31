#BuildMessageAppender Control

The BuildMessageAppender is a composite control which includes a rich text editor and allows for substitution of upstream data.

##Fields

The BuildMessageAppender extends the [TextArea](TextArea.md) class which includes the fields in the [ControlDefinitionDTO](../DataTransfer/ControlDefinitinDTO.md) class.

 In addition to the inherited fields, the BuildMessageAppender class includes the following fields:

__IsReadOnly__: boolean field with getter and setter which is used to set the "disabled" attribute on the fendered HTML element.

![alt text](images/build_message_appender.PNG "Build Message Appender UI")