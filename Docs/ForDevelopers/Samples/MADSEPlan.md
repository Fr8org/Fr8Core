# Plan Example: Monitor All Docusign Events (MADSE)

[Go to Contents](https://github.com/Fr8org/Fr8Core/blob/master/Docs/Home.md)

## Summary

 This plan uses the Fr8 event mechanism to trigger plan executions and uses the Fr8 Warehouse to store data. The MADSE Plan is built with 2 activities, which are "Prepare DocuSign Events For Storage" and "Save To Fr8 Warehouse".
 
 [INSERT IMAGE]

The MADSE Plan is automatically created by terminalDocusign on successful user authorization and is an internal plan. Which means normal users won't be able to see this plan on their plan list. See [Internal Plans](/Docs/ForDevelopers/OperatingConcepts/InternalPlans.md), [Internal Events](/Docs/ForDevelopers/OperatingConcepts/InternalEvents.md).

Upon successful authentication to Docusign, the Hub generates an internal event. When terminal docusign receives this internal event it tries to create a docusign connect profile for terminal event endpoint. Docusign connect notifies the given endpoint when user-related events happen. In our case Docusign will post new events to the events endpoint of terminalDocusign. See [Create Docusign Connect](https://github.com/Fr8org/Fr8Core/blob/dev/terminalDocuSign/Services/DocuSignPlan.cs#L71-L138).

Note: Docusign Connect requires a premium account on Docusign. If terminalDocusign fails to create a Connect profile it uses polling method as a fallback mechanism. See [Polling With Hub](/Docs/ForDevelopers/OperatingConcepts/PollingWithHub.md)

### Prepare DocuSign Events For Storage

This is a monitoring activity and as all monitoring activities it includes an EventSubscriptionCM in its configure response. Which subscribes this activity to external events. See [Events](https://github.com/Fr8org/Fr8Core/blob/master/Docs/ForDevelopers/OperatingConcepts/Events.md) for more information on Monitoring Activities.

Since purpose of this activity is to prepare all events for logging, it creates an EventSubscriptionCM with all docusign events.  [Event Subscription](https://github.com/Fr8org/Fr8Core/blob/dev/terminalDocuSign/Activities/Prepare_DocuSign_Events_For_Storage_v1.cs#L60-L61).

On runtime this activity extracts DocuSignEnvelopeCM_v2 from Event Payload and publishes this crate. [Code](https://github.com/Fr8org/Fr8Core/blob/dev/terminalDocuSign/Activities/Prepare_DocuSign_Events_For_Storage_v1.cs#L80-L85).

### Save To Fr8 Warehouse

This activity stores selected crate in Fr8 Warehouse.


[Go to Contents](/Docs/Home.md)
