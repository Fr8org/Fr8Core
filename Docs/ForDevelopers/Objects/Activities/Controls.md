#Control Objects
Some terminal activities require interaction with users at both design-time and runtime. In order to facilitate this interaction, several "control" classes are defined. These classes are serialized to JSON then sent over HTTP as payload data in a [Crate](https://github.com/Fr8org/Fr8Core/tree/master/Docs/ForDevelopers/OperatingConceptsk/Crates.md). In the Plan Builder interface the JSON data is translated by the Hub in to typical HTML elements such as select drop down lists, text fields, and radio buttons.

This document outlines the available controls. it is important to note that all controls extend the [ControlDefinitionDTO](../DataTransfer/ControlDefinitinDTO.md) and have access to the properties defined by the parent class, including events.

##Controls
*[BuildMessageAppender](Controls/BuildMessageAppender.md)*

The BuildMessageAppender control is used by the [BuildMessage PLACEHOLDER](BuildMessage.md) activity. This control extends the [TextBox](Controls/TextBox.md) control and has a type of ControlType.BuildMessageAppender.

*[Button](Controls/Button.md)*

The Button control generates an HTML button element which is generally used to trigger a form action.

*[CheckBox](Controls/Checkbox.md)*

The Checkbox control generates an HTML checkbox element. The Checkbox controls can be used in a group or individually.

*[ContainerTransition](Controls/ContainerTransition.md)*

The ContainerTransition control is a composite element which is used to connect distinct plans to each other via its _transitions_ property, which contains a list of [ContainerTransitionField](../ContainerTransitionField.md) objects.

*[ControlList](Controls/ControlList.md)*

The ControlList is a meta object which contains one or more controls and defines a [ListTemplate](../ListTemplate.md) property to describe the controls.

*[CrateChooser](Controls/CrateChooser.md)*

The CrateChooser control generates a form element which is used to select a crate of data to be used by an activity.

*[DatePicker](Controls/DatePicker.md)*

The DatePicker control generates a form element for selecting a date.

*[DropDownList](Controls/DropDownList.md)*

The DropDownList control generates a select form element.

*[Duration](Controls/Duration.md)*

The Duration control generates a form element which allows a user to select days, hours, and minutes. This element can be used to specify a period of time that can act as a delay.

*[ExternalObjectChooser](Controls/ExternalObjectChooser.md)*

The ExternalObjectChooser control generates a form element which will allow a user to select data from a data structure that is developed and maintained outside the Fr8 ecosystem.

*[FieldList](Controls/FieldList.md)*

The FieldList control generates a form element which displays fields generated from external data.

*[FilePicker](Controls/FilePicker.md)*

The FilePicker control generates a form element for choosing a file from the user's hard drive.

*[FilterPane](Controls/FilterPane.md)*

The FilterPane control generates a form for applying conditions which can create a subset of data.

*[Generic](Controls/Generic.md)*

The Gnereic control is a TextBox.

*[MappingPane](Controls/MappingPane.md)*

The MappingPane control is a composite control which generates a form with two select dropdown HTML elements. The first select element represents a set of meta data that can be assigned to a set of target metadata in the second select element.

*[QueryBuilder](Controls/QueryBuilder.md)*

The QueryBuilder control is a composite control which generates a form to apply conditional operators to a set of data. The form can have many queries using "and" and "or" operators.

*[RadioButtonGroup](Controls/RadioButtonGroup.md)*

The RadioButtonGroup control is a metadata container control which is designed to hold one or more [RadioButtonOption](Controls/RadioButtonOption.md) elements.

*[RadioButtonOption](Controls/RadioButtonOption.md)*

The RadioButtonOption control renders an HTML radio button element. This control is generally used in groups and is assigned to a RadioButtonControl object.

*[SelectData](Controls/SelectData.md)*

The SelectData control is a composite control which generates HTML buttons that are used to choose specific data from an activity template.

*[TextArea](Controls/TextArea.md)*

The TextArea control renders a textarea HTML element.

*[TextBlock](Controls/TextBlock.md)*

The TextBlock control renders a span element with a text-block CSS class.

*[TextBox](Controls/TextBox.md)*

The TextBox control renders an input tag with type = "text"

*[TextBoxBig](Controls/TextBoxBig.md)*

The TextBoxBig control renders a textarea tag.

*[TextSource](Controls/TextSource.md)*

The TextSource control renders a text-aource tag.

*[UpstreamCrateChooser](Controls/UpstreamCrateChooser.md)*

The UpstreamCrateChooser is a composite control which allows a user to select a crate from an upstream activity.

*[UpstreamDataChooser](Controls/UpstreamDataChooser.md)*

The UpstreamDataChooser is a composite control which allows a user to select data from a crate from an upstream activity.

*[UpstreamFieldChooser](Controls/UpstreamFieldChooser.md)*

The UpstreamFieldChooser is a composite control which allos a user to select dynamically generated fields