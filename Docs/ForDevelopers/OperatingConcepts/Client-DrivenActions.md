Client-Driven Actions
=====================
NOTE: As of this writing, this functionality is only partially implemented

Fr8 has a mechanism that allows a Plan to be configured in such a way that, when the Plan is done executing, the Client executes a specific action that it knows how to do.

The original use case for this was to use a Fr8 Plan to generate an on-screen report. The Plan would query a database and generate a table of
results data in the Payload Container. But now we want to display that information on screen for the user who has just triggered running 
of the Plan. 

To do this, we defined the following mechanism:
The Client has some pieces of functionality that are considered Client Actions. These are published in documentation. Any Activity that wants
to trigger one of these Client Actions can generate an [Activity Response](/Docs/ForDevelopers/Objects/Activities/ActivityResponses.md) of ExecuteClientAction. When it responds to the Hub with this ActivityResponse,
the Hub changes the ContainerState to Pending and passes the response on to the Client, and the Client executes the associated functionality.

As of this writing, we don't have much defined. The first Client Action is "Show Table Report". 


When the client has completed executing the client code it should again post to the Hub to run the container, and the Hub should continue execution. If the Client Action is the final action in the route, then the Hub will just carry out normal end of Plan processing. 
The Hub will need to know whether or not the client action was successfully completed.
This is a good use case for the Logging Crate mechanism we've built but not really used yet. The client should add a Log entry to the Logging Crate that's part of the payload. Then the Hub can inspect this and take appropriate action
