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
the Hub changes the ContainerState to Pending and passes the response on to the Client.
The Client looks for a property of ActivityResponse called CurrentClientActivityName and executes the procedure associated with the value of that property. 

Currently supported values for CurrentClientActivityName

Value |	What the Hub Does |	Notes
------ | -------- | ------------------
Show Table Report |	Look in the Payload for Table Data and render it in a new window on screen
RunImmediately |	Immediately Execute the current Plan. See ["Run Triggers"](/Docs/ForDevelopers/OperatingConcepts/RunTriggers.md)


When the client has completed executing the client code it should again post to the Hub to run the container, and the Hub should continue execution. If the Client Action is the final action in the route, then the Hub will just carry out normal end of Plan processing. 



