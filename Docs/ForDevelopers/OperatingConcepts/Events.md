# EVENTS

[Go to Contents](https://github.com/Fr8org/Fr8Core/blob/master/Docs/Home.md)

## Summary

Terminals can generate events and post them to the Hub. The primary uses of this are:

1) to report loggable facts (analytics) and incidents (problems) so the Hub can provide a centralized view of what's happening
2) to report events, callbacks, webhooks and other kinds of data that have been received at the Terminal by some external Web Service.

Event data is packaged using the crate manifest Standard Event Report.  The Terminal then POSTs the Event Report crate to the main event receiving endpoint on the Hub, which is at `/event`. The Hub then inspects all of the activated plans to see if any of them are monitoring for this event. To monitor an event, an Activity creates an EventSubscription crate at design-time.  If a Plan starts with an Activity that has a matching EventSubscription crate, then a match is made and that plan will be executed, and the data in the EventReport will be added to the CrateStorage of the Container that is newly created as part of the plan launch, so the Terminals receive the information.

Example: The Monitor DocuSign Activity invites users to create plans that trigger when a DocuSign envelope is sent or signed. When a Plan with this Activity is created, the Activity code creates an Event Subscription crate and adds it to the Activity's CrateStorage. Shortly thereafter, the user clicks Run, and an /activate call is sent to the DocuSign Terminal. It takes steps to make sure that it will learn about these events. In the case of DocuSign, this is relatively complex, because some DocuSign users have service levels that entitle them to real-time webhook notification callbacks, and some do not. For the ones that do not, the DocuSign Terminal has to use a polling solution. The user subsequently sends a DocuSign envelope, and the Terminal detects this, either by receiving a POSTed notification or by using polling. It packages the information received from DocuSign into an Event Report crate and POSTs it to the Hub. The Hub detects that the Plan created by the user has an Event Subscription matching the Event Report, and triggers execution of the Plan, passing it the Event Report data as part of the newly created Payload Container.

## Facts and Incidents

Terminals can use events endpoint to report Facts and Incidents to the Hub. In order to do that EventReport crate has to have ExternalAccountId set to "system1@fr8.co" and to have LoggingData crates in its crate storage.
LoggingData crates with "Terminal Fact" name are processed as Facts, and crates with "Terminal Incident" name are processed as Incidents.
Facts are supposed to represent data that is useful for business analysis, while Incidents are supposed to represent critical issues in the work of the terminal.

## At Design-Time

Activities can subscribe to an event by adding to themselves a Crate of manifest “Standard Event Subscriptions” and loading it with the names of the events they want to subscribe to.

So, for example, the Create “Wait For DocuSign Event” Trigger Activity for DocuSign Plugin, after receiving the user’s configuration selections and settings, creates a Crate of Class “Standard Event Subscriptions” with strings like “DocuSign Envelope Sent”, and saves the Crate on itself.

## At Activation-Time

It is common for terminals to register for events with their corresponding web services in response to a plan activation POST.

Activities that request notifications should also remove the notification requests upon the receipt of a `/deactivate` request.

Whenever Hub receives call to `/event` endpoint with payload containing crate of Standard Event Report manifest it tries to determine which plans should be run in response to this event. It checks `externalDomainId` and `externalAccountId` properties of the manifest, identifies the authorizations that bear the same values of these properties, get the owners of these authorizations and check their active plans that contains Standard Event Subscription manifest inside its activities storage which matches `eventNames` property of incoming event.

If terminal posts manifest with empty `externalAccountId` property but with nonempty `externalDomainId` property that eventually means event that relates to all accounts that belong to specified external domain.

Terminals should avoid posting manifests with both `externalAccountId` and `externalDomainId` as such events won't be processed by Hub

Values assigned to `externalDomainId` and `externalAccountId` should match the same values terminal assigns to authorization tokens retrieved from `/authentication/token` endpoint for the respective user authorizations


## Walkthrough

Before starting: Read [Activity Development Guide](/Docs/ForDevelopers/DevelopmentGuides/ActivityDevelopmentGuide.md)

In this example we are going to show you how to create a Monitoring Activity.

##### Step 1. Listening For Events

- You need to include an EventSubscriptionCM inside your CrateStorage on a configure response.

With this crate inside your activity storage, you are telling hub to run containing plan when this event happens. Here is an example configure response to subscribe your activity to events.

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

With this json response you are creating an EventSubscriptionCM crate which is listening for "Event1" and "Event2". Whenever one of those events happens, hub will run activated plans which have EventSubscriptionCM and listening to one of those events.

##### Step 2. Triggering Events

It is your terminal's responsiblity to create an endpoint for external system's events and listen to them. Every system has different methods for registering to their events. Most of them use [Webhooks](https://en.wikipedia.org/wiki/Webhook) for this purpose.

Here are some samples to help you understand this concept. [Monitor All Docusign Events Plan](/Docs/ForDevelopers/Samples/MADSEPlan.md).

Assuming that you have correctly configured external system to notify your terminal on events, you need to parse incoming event data and create an EventReportCM crate according to incoming event.

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
            }
          }
        ]
    }
  },
}
```


- After preparing EventReportCM, you need to post this crate to /events endpoint of all subscribed hubs.

Hub will run related plans but this time it will be different from regular run. When event triggers a plan execution, hub will add Standard Event Report crate to the payload of container. Therefore all activities will be able to access event data.

- Generally your monitor activity should extract data from EventPayload and publish this data.

With your published data on container payload, downstream activities will be able to use your event data.


[Go to Contents](/Docs/Home.md)
