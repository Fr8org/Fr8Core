# EVENTS

[Go to Contents](https://github.com/Fr8org/Fr8Core/blob/master/Docs/Home.md)

## Summary

Terminals can generate events and post them to the Hub. Event data is crated using the crate manifest Standard Event Report.  The Terminal then POSTs the Event Report crate to the main event receiving endpoint on the Hub, which is at `/event`. The Hub then inspects all of the running plans to see if any of them are monitoring for this event. To monitor an event, an Activity defines an EventSubscription crate at design-time. When there’s a match, if a plan starts with an action that has a matching EventSubscription crate, that plan will be launched, and the data in the EventReport will be added to the CrateStorage of the Container that is newly created as part of the plan launch.

Separately, Terminals will often communicate with their corresponding web services and register for events to be sent directly to the Terminal. For example, the DocuSign terminal will register with DocuSign to receive notification of events affecting certain Fr8 users that have DocuSign accounts.

## Facts and Incidents

Terminals can use events endpoint to report Facts and Incidents to the Hub. In order to do that EventReport crate has to have ExternalAccountId set to "system1@fr8.co" and to have LoggingData crates in its crate storage.
LoggingData crates with "Terminal Fact" name are processed as Facts, and crates with "Terminal Incident" name are processed as Incidents.
Facts are supposed to represent data that is useful for bussines analysis, while Incidents are supposed to represent critical issues in the work of the terminal.

## At Design-Time

Activities can “subscribe” to an event by adding to themselves a Crate of manifest “Standard Event Subscriptions” and loading it with the names of the events they want to subscribe to.

So, for example, the Create “Wait For DocuSign Event” Trigger Activity for DocuSign Plugin, after receiving the user’s configuration selections and settings, creates a Crate of Class “Standard Event Subscriptions” with strings like “DocuSign Envelope Sent”, and saves the Crate on itself.

## At Activation-Time

It is common for terminals to register for events with their corresponding web services in response to a plan activation POST.

Activities that request notifications should also remove the notification requests upon the receipt of a `/deactivate` request.

Whenever Hub receives call to `/event` endpoint with paylod containing crate of Standard Event Report manifest it tries to determine which plans should be run in response to this event. It checks `externalDomainId` and `externalAccountId` properties of the manifest, identifies the authorizations that bear the same values of these properties, get the owners of these authorizations and check their active plans that contains Standard Event Subscription manifest inside its activities storage which matches `eventNames` property of incoming event.

If terminal posts manifest with empty `externalAccountId` property but with nonempty `externalDomainId` property that eventually means event that relates to all accounts that belong to specified external domain. 

Terminals should avoid posting manifests with both `externalAccountId` and `externalDomainId` as such events won't be processed by Hub

Values assigned to `externalDomainId` and `externalAccountId` should match the same values terminal assigns to authorization tokens retrieved from `/authentication/token` endpoint for the respective user authorizations

[Go to Contents](https://github.com/Fr8org/Fr8Core/blob/master/Docs/Home.md)
