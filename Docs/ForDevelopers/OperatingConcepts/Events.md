# EVENTS

## Summary

Terminals can generate events and post them to the Hub. Event data is crated using the crate manifest Standard Event Report.  The Terminal then POSTS the Event Report crate to the main event receiving endpoint on the Hub, which is at /event. The Hub then inspects all of the active routes to see if any of them are monitoring for this event. To monitor an event, an Action defines an EventSubscription crate at design-time. When there’s a match, if a route starts with an action that has a matching EventSubscription crate, that route will be launched, and the data in the EventReport will be added to the CrateStorage of the Container that is newly created as part of the route launch.

Separately, Terminals will often communicate with their corresponding web services and register for events to be sent directly to the Terminal. For example, the DocuSign terminal will register with DocuSign to receive notification of events affecting certain Fr8 users that have DocuSign accounts.

## At Design-Time

Actions can “subscribe” to an event by adding to themselves a Crate of manifest “Standard Event Subscriptions” and loading it with the names of the events they want to subscribe to.

So, for example, the Create “Wait For DocuSign Event” Trigger Action for DocuSign Plugin, after receiving the user’s configuration selections and settings, creates a Crate of Class “Standard Event Subscriptions” with strings like “DocuSign Envelope Sent”, and saves the Crate on itself.

## At Activation-Time

It is common for terminals to register for events with their corresponding web services in response to a route activation POST.

Actions that request notifications should also remove the notification requests upon the receipt of a /deactivate request.
