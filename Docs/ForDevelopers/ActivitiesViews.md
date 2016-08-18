# ACTIVITIES – VIEWS
[Go to Contents](https://github.com/Fr8org/Fr8Core/blob/master/Docs/Home.md)

Activity views are rendered from StandardConfigurationControlsCM crates. Plan Builder assumes that all your configuration controls are inside this crate. By default Plan Builder renders StandardConfigurationControlsCM with "Configuration_Controls" label.

An Activity can be configured to show multiple views to the user. Your activity can create more than one StandardConfigurationControlsCM with different labels. At least one of them should have "Configuration_Controls" label.

When Plan Builder loads, it checks the "view" http parameter, if that parameter is not found default StandardConfigurationControlsCM crate is rendered. If it finds this parameter, it searches for StandardConfigurationControlsCM with specified label in activities and skips rendering of activities which doesn't have required crate.

Currently our AppBuilder activity works with this principle, it creates 2 StandardConfigurationControlsCM crates with different labels. And our kiosk mode opens in "collection" view. Therefore AppBuilder creates 2 StandardConfigurationControlsCM, one with "collection" label and other with "Configuration_Controls" label.

We are aware that our current view rendering system is not sufficient. Only http params are supported to render different crates and an activity has no way of opening plan builder with different parameters. Only users are able to open PlanBuilder with different parameters. There are only 2 static views that Fr8 currently supports, which are "collection" and our default view.

We are working to make it more dynamic and better.

## This part is obsolete

~~ The client’s rendering code looks for the presence of this property on an Activity before rendering it. If it exists and has a value, then the rendering code looks for a Crate with that label, and renders the pane using that Crate.

One common use is to show custom error messages. For example, an Activity like Map Fields normally provides a a Crate of controls that generate a set of mapping drop down list boxes. However, if it doesn’t find source data, it instead needs to show an error message.

So the plugin can create a separate error View by creating a separate Crate of configuration controls with the Label “MapFieldsErrorMessage” and setting the CurrentView property to that. ~~

[Go to Contents](https://github.com/Fr8org/Fr8Core/blob/master/Docs/Home.md)
