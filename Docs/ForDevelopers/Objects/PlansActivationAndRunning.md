# PLANS – ACTIVATION AND RUNNING
[Go to Contents](https://github.com/Fr8org/Fr8Core/blob/master/Docs/Home.md)  

Plans are created by the Hub in response to client requests. Initially, Plans are inactive.

Plans fall into two groups:

1. “Standard” plans run one time and then stop
2. “Monitor” plans are designed to listen for external triggers, and remain in Running mode until and unless they’re stopped manually.
If a Plan’s *initial Activity* has Category = “Monitor” or Activity crate storage contains non empty EventSubscriptionCM crate then it is treated as a “Monitor” Plan. Otherwise it is treated as a “Standard” plan.

When the user clicks Run on a Run One Time plan the Hub first calls /activate on each Activity, which allows the Activity a chance to carry out validation and set up necessary mechanisms. The Hub then calls /run and the Plan’s State changes to “Active”. If any of the Activities returns an error during Activation, the Plan never runs, and the error messages are returned and displayed in the Client as in-situ error messages.
After the Plan runs, the Plan’s State is changed back to “Inactive”. It will remain in the Plan Library, where it can be run manually at any time.

When the user clicks Run on a Monitor plan, the behavior is the same with the following exceptions. Hub will not call /run, Plan State remains “Active”, and the client renders the Plan in the “Running Plans” section of the Plan Dashboard instead of in the “Plan Library.”

In general, Activity should do all global initialization that are not dependent on particular container (such as registering with Slack to receive messages notification) during /activate and do all unitialization during /deactivate. Like plans, activities can be in several states: 'Activated' and 'Deactivated'. For each activity Hub tracks its internal activation state. Initially all activities are 'Deactivated'. During plan activation Hub examines activity state. If it is 'Deactivated' /activate is called for this activity. If /activate call is sucessfull and returns no validation errors activity chanages state to 'Activated'. When user changes Activity configuration and Activity state is 'Activated' Hub calls /deactivate first to give Activity a chance to correctly uninitialize, Activity state changes to 'Deactivated' and only then /configure is called. At the same time, Plan also becomes 'Inactive'. Use has to click 'Run' on edited plan again to make it working after edits.

Hub will call /activate and /deactivate for each activity at least once. This means, that /activate and /deactivate can be called several times for a particular configuration. So activity should take care of duplicate initialization and uninitialization.

[Go to Contents](https://github.com/Fr8org/Fr8Core/blob/master/Docs/Home.md)  
