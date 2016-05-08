# PLANS – ACTIVATION AND RUNNING
[Go to Contents](https://github.com/Fr8org/Fr8Core/blob/master/Docs/Home.md)  

Plans are created by the Hub in response to client requests. Initially, Plans are inactive.

Plans fall into two groups:

1. “Standard” plans run one time and then stop
2. “Monitor” plans are designed to listen for external triggers, and remain in Running mode until and unless they’re stopped manually.
If a Plan’s *initial Activity* has Category = “Monitor” then it is treated as a “Monitor” Plan. Otherwise it is treated as a “Standard” plan.

When the user clicks Run on a Run One Time plan the Hub first calls /activate on each Activity, which allows the Activity a chance to carry out validation and set up necessary mechanisms. The Hub then calls /run and the Plan’s State changes to “Running”. If any of the Activities returns an error during Activation, the Plan never runs, and the error messages are returned and displayed in the Client as in-situ error messages.

After the Plan runs, the Plan’s State is changed back to “Inactive”.  It will remain in the Plan Library, where it can be run manually at any time.

When the user clicks Run on a Monitor plan, the behavior is the same with the following exceptions. The Plan State remains “Running”, and the client renders the Plan in the “Running Plans” section of the Plan Dashboard instead of in the “Plan Library.”

In general, when a “Monitor” Activity receives a /run call, it will typically inspect the Container passed to it with the /run command to see whether the current run call represents either a) an initial activation call or b) an actual received event that the Monitor activity had registered interest in.  For example, the Monitor DocuSign Envelopes Activity inspects the Container for the presence of a DocuSign envelopeId. If it doesn’t find one, it  carries out Activation activities (such as registering with DocuSign to receive event notification) and quickly returns. As described above, it’s Status remains “Active” and it will continue to receive /run calls when matching events are received.

[Go to Contents](https://github.com/Fr8org/Fr8Core/blob/master/Docs/Home.md)  
