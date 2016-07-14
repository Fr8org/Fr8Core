# ACTIVITIES – VIEWS
[Go to Contents](https://github.com/Fr8org/Fr8Core/blob/master/Docs/Home.md) 

An Activity can be configured to show multiple views to the user via a property called CurrentView.
 

The client’s rendering code looks for the presence of this property on an Activity before rendering it. If it exists and has a value, then the rendering code looks for a Crate with that label, and renders the pane using that Crate.

One common use is to show custom error messages. For example, an Activity like Map Fields normally provides a a Crate of controls that generate a set of mapping drop down list boxes. However, if it doesn’t find source data, it instead needs to show an error message.

So the plugin can create a separate error View by creating a separate Crate of configuration controls with the Label “MapFieldsErrorMessage” and setting the CurrentView property to that.

[Go to Contents](https://github.com/Fr8org/Fr8Core/blob/master/Docs/Home.md) 
