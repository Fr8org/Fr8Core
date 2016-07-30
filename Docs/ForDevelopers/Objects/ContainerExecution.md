# CONTAINER EXECUTION
[Go to Contents](https://github.com/Fr8org/Fr8Core/blob/master/Docs/Home.md) 

When a Plan is Run, a Container is created to hold the Fr8 data related to the run and the Container’s CurrentActivity property is set to point to the first Activity in the Plan.  Then the Activity gets Processed, which means that the Hub makes an HTTP request to that Activity’s Terminal /run endpoint.

### Processing the ActivityResponse

When the Terminal returns, it hands the Hub an [ActivityResponse](https://github.com/Fr8org/Fr8Core/blob/master/Docs/ForDevelopers/ActivitiesCommunication.md).  Some ActivityResponses result in a change to the Container’s State.

The most common response is simply “Success”. Other values are shown in the table above. If Container.State is anything other than “Executing”, the Hub will break out of its processing loop and processing of the Container will be over. The Hub will return a response to the Client and wait for the next user action.

Otherwise, the Hub figures out what to process next.

[Go to Contents](https://github.com/Fr8org/Fr8Core/blob/master/Docs/Home.md) 
