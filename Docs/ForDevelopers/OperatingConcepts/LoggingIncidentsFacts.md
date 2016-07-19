Logging, Incidents and Facts
============================


Fr8 creates a couple of different kinds of analytic data that can be useful during development and debugging. 

Activity Stream
----------------
The simples form of feedback is the Activity Stream, which is visible by all users on the right side of the Plan Builder.  Error messages that are returned by Terminals in ActivityResponses 
are pushed to the Activity Stream. The current implementation uses Pusher. 

Logging
-------

Fr8 differentiates between persistent and non-persistent log data. If the information doesn't need to be kept around for more than a couple of days,
Fr8 code posts it to the Papertrail service. As of this writing, access to both the Production logs and the Dev logs is controlled by one group run by The Fr8 Company.
We have a goal of making it more granular, so that external developers can gain immediate access to (at a minimum) the Dev logs. Let the team 
know if you want this prioritized.

Facts and Incidents
-------------------
Facts and Incidents are intended to be persistent time-based data points, the kind of data points that are put into data cubes for analysis. They are very similar, structurall,
both deriving from a base class called HistoryItem. They are distinguished mainly by purpose: Facts are intended to represent activities that
took place that might be interesting from an analytics point of view, while Incidents are intended to represent things that went wrong and facilitate debugging.

Both the Facts and Incidents produced a user's account can be viewed from the Developer menu (which is unlocked from the Tools menu).

![](incidentreport.png)

Generating Incidents
--------------------

Terminal developers are encouraged to Post Incident Reports to the Hub. They will be stored in the Hub's database and integrated into the report of Incidents viewable from the client. In general, the rule of thumb is: if you want end users to see the information, send it in the ActivityResponse as an Error message. If you want developers and admins to see it, post it as an Incident Report. 
