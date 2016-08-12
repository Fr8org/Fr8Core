#UI Controls

Fr8's UI philosophy is that Activity designers should be able to tap a rich set of UI controls for purpose of passing information to users and collecting configuration information from users, but should never have to think about layout or FE issues. So we emulated the basic idea of HTML (declare the UI you want in more-or-less English) and created a mechanism for the settings input by the user to be passed back to the Activity when it needs them. 

Like the rest of Fr8 UI structures are defined in JSON and passed around in [Crates](/Docs/ForDevelopers/OperatingConceptsk/Crates.md). UI Controls Crates are added to the Crate Storage of Activities. The Client parses the data and renders the appropriate structures on screen, and when users type in or select values, those are saved into the same Crates.

In practice, Activity designers aren't expected to want to engage in actual JSON manipulation, so one of the core services provided by the growing set of [Fr8 Platform SDK's](/Docs/ForDevelopers/SDKHome.md) is a set of UI classes that can be used in native code and handle the JSON quietly in the background. 

As a result, the JSON docs here are a useful reference, but you'll want to look at your SDK and at examples of existing Activity code to see the best practices for your particular platform.

Note that all controls extend the [ControlDefinitionDTO](/Docs/ForDevelopers/Objects/DataTransfer/ControlDefinitionDTO.md) base set.

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

*[FieldList](Controls/FieldList.md)*

The FieldList control generates a form element which displays fields generated from external data.

*[FilePicker](Controls/FilePicker.md)*

The FilePicker control generates a form element for choosing a file from the user's hard drive.

*[FilterPane](Controls/FilterPane.md)*

The FilterPane control generates a form for applying conditions which can create a subset of data.

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

The TextBox control renders an input tag with type | "text"

*[TextBoxBig](Controls/TextBoxBig.md)*

The TextBoxBig control renders a textarea tag.

*[TextSource](Controls/TextSource.md)*

The TextSource control renders a text-aource tag.

*[UpstreamFieldChooser](Controls/UpstreamFieldChooser.md)*

The UpstreamFieldChooser is a composite control which allos a user to select dynamically generated fields

##<a name="control-types"></a>Consolidated List of ControlType names

ControlType | Name
:---:|:---:
BuildMessageAppender | "BuildMessageAppender"
Button | "Button"
CheckBox | "CheckBox"
ContainerTransition | "ContainerTransition"
ControlList | "ControlList"
CrateChooser | "CrateChooser"
DatePicker | "DatePicker"
DropDownList | "DropDownList"
Duration | "Duration"
FieldList | "FieldList"
FilePicker | "FilePicker"
FilterPane | "FilterPane"
QueryBuilder | "QueryBuilder"
RadioButtonGroup | "RadioButtonGroup"
SelectData | "SelectData"
TextArea | "TextArea"
TextBlock | "TextBlock"
TextBox | "TextBox"
TextBoxBig | "TextBoxBig"
TextSource | "TextSource"
UpstreamFieldChooser | "UpstreamFieldChooser"

