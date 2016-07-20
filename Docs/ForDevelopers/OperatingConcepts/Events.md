# EVENTS

[Go to Contents](https://github.com/Fr8org/Fr8Core/blob/master/Docs/Home.md)

## Summary

Terminals can generate events and post them to the Hub. Event data is crated using the crate manifest Standard Event Report.  The Terminal then POSTs the Event Report crate to the main event receiving endpoint on the Hub, which is at `/event`. The Hub then inspects all of the running plans to see if any of them are monitoring for this event. To monitor an event, an Activity defines an EventSubscription crate at design-time. When there’s a match, if a plan starts with an action that has a matching EventSubscription crate, that plan will be launched, and the data in the EventReport will be added to the CrateStorage of the Container that is newly created as part of the plan launch.

Separately, Terminals will often communicate with their corresponding web services and register for events to be sent directly to the Terminal. For example, the DocuSign terminal will register with DocuSign to receive notification of events affecting certain Fr8 users that have DocuSign accounts.

## Facts and Incidents

Terminals can use events endpoint to report Facts and Incidents to the Hub. In order to do that EventReport crate has to have ExternalAccountId set to "system1@fr8.co" and to have LoggingData crates in its crate storage.
LoggingData crates with "Terminal Fact" name are processed as Facts, and crates with "Terminal Incident" name are processed as Incidents.
Facts are supposed to represent data that is useful for business analysis, while Incidents are supposed to represent critical issues in the work of the terminal.

## At Design-Time

Activities can “subscribe” to an event by adding to themselves a Crate of manifest “Standard Event Subscriptions” and loading it with the names of the events they want to subscribe to.

So, for example, the Create “Wait For DocuSign Event” Trigger Activity for DocuSign Plugin, after receiving the user’s configuration selections and settings, creates a Crate of Class “Standard Event Subscriptions” with strings like “DocuSign Envelope Sent”, and saves the Crate on itself.

## At Activation-Time

It is common for terminals to register for events with their corresponding web services in response to a plan activation POST.

Activities that request notifications should also remove the notification requests upon the receipt of a `/deactivate` request.

Whenever Hub receives call to `/event` endpoint with payload containing crate of Standard Event Report manifest it tries to determine which plans should be run in response to this event. It checks `externalDomainId` and `externalAccountId` properties of the manifest, identifies the authorizations that bear the same values of these properties, get the owners of these authorizations and check their active plans that contains Standard Event Subscription manifest inside its activities storage which matches `eventNames` property of incoming event.

If terminal posts manifest with empty `externalAccountId` property but with nonempty `externalDomainId` property that eventually means event that relates to all accounts that belong to specified external domain.

Terminals should avoid posting manifests with both `externalAccountId` and `externalDomainId` as such events won't be processed by Hub

Values assigned to `externalDomainId` and `externalAccountId` should match the same values terminal assigns to authorization tokens retrieved from `/authentication/token` endpoint for the respective user authorizations


## Walkthrough

Please read [Activity Development Guide](https://github.com/Fr8org/Fr8Core/blob/master/Docs/ForDevelopers/DevelopmentGuides/ActivityDevelopmentGuide.md) before this tutorial.

### Listening For Events

An activity needs to include EventSubscriptionCM inside its CrateStorage on a configure response. With this crate inside activity's storage, activity is telling hub to run containing plan when this event happens. Here is an example configure response to subscribe our activity to events.

```javascript
{
  "label": null,
  "name": "My first activtiy",
  "activityTemplate":{  
    "id":"87ab869a-9573-4554-b5b1-4bcaea7064a9",
    "name":"My_first_activity",
    "label":"My first activity",
    "version":"1",
    "terminal":{  
    "name":"MyTerminal",
    "label":"My Teriminal",
    "version":"1",
    "endpoint":"http://terminal.com",
  },
  "RootPlanNodeId":"4a0e2fa4-0422-4cc2-b308-089720f2dd5c",
  "ParentPlanNodeId":"4554d028-9955-4121-97e0-2fb9a1e40e80",
  "ordering": 1,
  "id": "1cfdba78-9a86-47bb-8bc9-2422528220ac",
  "crateStorage": {
    "crates": [
      {
        "manifestType": "Standard UI Controls",
        "manifestId": 6,
        "manifestRegistrar": "www.fr8.co/registry",
        "id": "{generate some GUID value here}",
        "label": "Configuration_Controls",
        "contents": {
          "Controls": [
            {
              "type": "TextBlock",
              "name": "MyFirstMonitorActivityTextblock",
              "required": false,
              "selected": false,
              "value": "This is my first monitor activity",
              "label": null,
              "events": []
	         }
          ]
        },
      },
      {
        "manifestType": "Standard Event Subscription",
        "manifestId": 8,
        "manufacturer": null,
        "manifestRegistrar": "www.fr8.co/registry",
        "id": "{generate some GUID value here}",
        "label": "Standard Event Subscriptions",
        "contents": {
          "Subscriptions": [
            "Event1", "Event2"
          ],
          "Manufacturer": "MyExternalSystem"
        }
      }
    ]
  },
  "childrenActivities": [],
}
```

With this json response we are creating an EventSubscription crate which is listening for "Event1" and "Event2". Whenever one of those events happens hub will run activated plans which have EventSubscriptionCM and listening to one of those events.

### Triggering Events

It is your terminal's responsiblity to create an endpoint for external system's events and listen to them. Every system has different methods for registering to their events. Most of them use webhooks for this purpose.

Here are some samples to help you understand this concept. [Docusign Event Mechanism](https://github.com/Fr8org/Fr8Core/blob/master/Docs/ForDevelopers/Samples/DocusignEventGeneration.md) and [Facebook Event Mechanism](https://github.com/Fr8org/Fr8Core/blob/master/Docs/ForDevelopers/Samples/FacebookEventGeneration.md).

Assuming that you have correctly configured external system to notify your terminal on events. Terminal needs to parse incoming event data and create an EventReportCM crate according to incoming event.

Below is a sample EventReportCM

```javascript
{
  "manifestType": "Standard Event Report",
  "manifestId": 7,
  "manifestRegistrar": "www.fr8.co/registry",
  "id": "{generate some GUID value here}",
  "label": "Standard Event Report",
  "contents": {
    "EventNames": ["Event1", "Event3"],
    "Manufacturer": "MyExternalSystem",
    "ExternalAccountId": "whichUserThisEventWasCreated@domain.com",
    "ExternalDomainId": null,
    "EventPayload": {
        "crates": [
          {
            "manifestType": "Custom Data Manifest",
            "manifestId": --,
            "manifestRegistrar": "www.fr8.co/registry",
            "id": "{generate some GUID value here}",
            "label": "External Payload Data",
            "contents": {
              //custom data created by your external system
              //this data will be processed by your monitor activity
            },
          }
        ]
    }
  },
}
```


After preparing this EventReport, terminal needs to post this crate to /events endpoint of all subscribed hubs. Hub will run related plans but this time it will be different from regular run. When event triggers a plan execution hub will add EventPayload to the payload of container. Therefore all activities will be able to access event data.





[Go to Contents](https://github.com/Fr8org/Fr8Core/blob/master/Docs/Home.md)
