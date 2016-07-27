Before You Begin - Logging and Debugging
=======================================


First, read about the [different logging mechanisms](/Docs/ForDevelopers/OperatingConcepts/LoggingIncidentsFacts.md) supported by Fr8. 

Low-level Transient Log Data
------------------------------

Hub log data is currently streamed through log4net to Papertrail. All developers are welcome to view the logstream of the dev server managed by The Fr8 Company
at dev.fr8.co. Right now, this access is simply available by logging onto the papertrail account:


username: _______________ UPDATE THIS

password: _______________


For your own Terminal, we recommend that you create a Papertrail account and take advantage of their free tier of service. 


Error Reporting
----------------------------------

First, make sure you're clear on the difference between the Incident Reports (which are persistent and intended for debugging and developer use, and are not seen by end users)
and end-user error messages. Most of your error message work will be done by returning ActivityResponses of Error, and including a user-friendly message
that will get rendered in the client Activity Stream. You'll also want to generate and Post Incident Reports to the Hub, to make it easier to debug 
problems involving your Terminal. Make sure you activate the Developer menu (using the toggle on the Tools menu of the Plan Builder) and examine
Incident Reports for Plans you build that don't work correctly, to get familiar with Incident Reports in action.
