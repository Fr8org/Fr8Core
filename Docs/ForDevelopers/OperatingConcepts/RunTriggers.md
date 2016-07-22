Run Triggers
=============

This functionality has been mostly superseded by App Builder, but may still prove useful. 

The idea here is that Fr8 Plans, in Design-Mode, are a lot like dialog boxes. The user fills in some information and clicks a button that's typically
labelled "Continue" or "Submit" or "Ok", and something happens. By default though, Fr8 doesn't have a button mechanism like that. In order 
to get something to "happen", the user has to know to go to the upper right part of the Plan Builder, find the Run button, and click it.

So we added a Run Plan Button UI Control. An Activity can add this to their UI Controls crate, and it will be displayed by the client. 
When the user clicks the button, its value is set by the client to True, and Followup Configuration is triggered via standard Configuration Control Events.
The Activity can read the value and determine that the user has just clicked the button. At this point, the Activity wants to signal the Hub 
to (in this case) immediately execute the Plan. It does this indirectly, by adding the [ActivityResponse]() with settings of [UPDATE THIS]. 

When the client does its normal parsing of the returned Configuration Response, it looks for the presence of this message in the Operational State Crate. If
it finds it, it triggers a Run immediately.

