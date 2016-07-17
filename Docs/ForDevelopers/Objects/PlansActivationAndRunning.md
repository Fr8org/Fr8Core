# PLANS – ACTIVATION AND RUNNING
[Go to Contents](https://github.com/Fr8org/Fr8Core/blob/master/Docs/Home.md)  

Plans are created by the Hub in response to client requests. Initially, Plans are inactive.

## General Plan Execution

A run call might be generated as the result of a user activity (i.e. clicking "Run" in the Plan Dashboard) or in response to an external trigger event.   Please see [Monitor Activities](/Docs/MonitorActivities.md) to understand external events.

When a plan is run, the Hub creates a json structure called a Container (sometimes "Payload Container"). The Hub works its way down the Plan, calling each activity's /run function and passing it the Payload Container. This container is often initially empty, although if the Plan is triggered by an external event, it will usually contain a Crate of data about the event (see [Events](https://github.com/Fr8org/Fr8Core/blob/master/Docs/ForDevelopers/OperatingConcepts/Events.md)) The ordering is depth-first: children activities are run before sibling activities. 

Activities are expected to process the incoming data and add to the Payload Container any new data. Depending on the situation, an Activity might want to modify an existing Crate of data or create one or more new Crates. Then the modified Payload Contianer is passed back to the Hub. 

## Standard vs. Monitor Plans

Plans fall into two groups:

1. “Standard” plans Execute immediately in response to the User clicking on "Run". While they are executing, they can be seen in the Running Plans list. When the Hub completes execution, the Plan is moved from the Running Plans list to the Inactive Plans list.   
2. “Monitor” plans are designed to listen for external triggers and then Execute. When the user runs a Plan that starts with a Monitor type Activity, the Plan is visually moved to the Running Plans list but does not actually Execute. Instead it subscribes to the Event it's designed to trigger on. When the event is received, an instance of the Plan is executed.

If a Plan’s *initial Activity* has Category = “Monitor” or Activity crate storage contains non empty EventSubscriptionCM crate then it is treated as a “Monitor” Plan. Otherwise it is treated as a “Standard” plan.

When the user clicks Run on a Plan,  the Hub  calls /activate on each Activity, whether or not it intends to immediately follow that up with an execution of the plan. This allows the Activity a chance to carry out validation and set up necessary mechanisms. The Hub then calls /run and the Plan’s State changes to “Running”. If any of the Activities returns an error during Activation, the Plan never runs, and the error messages are returned and displayed in the Client as in-situ error messages.
After the Plan runs, the Plan’s State is changed back to “Inactive” unless it's a Monitor Plan, in which case it remains in the Running Plans list.  



##Plan Activation and Deactivation


In general, Activity should do all global initialization that are not dependent on particular container (such as registering with Slack to receive messages notification) during /activate and do all unitialization during /deactivate. Like plans, activities can be in several states: 'Activated' and 'Deactivated'. For each activity Hub tracks its internal activation state. Initially all activities are 'Deactivated'. During plan activation Hub examines activity state. If it is 'Deactivated' /activate is called for this activity. If /activate call is sucessfull and returns no validation errors activity chanages state to 'Activated'. When user changes Activity configuration and Activity state is 'Activated' Hub calls /deactivate first to give Activity a chance to correctly uninitialize, Activity state changes to 'Deactivated' and only then /configure is called. At the same time, Plan also becomes 'Inactive'. Use has to click 'Run' on edited plan again to make it working after edits.

Hub will call /activate and /deactivate for each activity at least once. This means, that /activate and /deactivate can be called several times for a particular configuration. So activity should take care of duplicate initialization and uninitialization.

[Go to Contents](https://github.com/Fr8org/Fr8Core/blob/master/Docs/Home.md)  
