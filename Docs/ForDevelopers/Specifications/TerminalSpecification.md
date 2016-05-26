# TERMINAL SPECIFICATION
[Go to Contents](https://github.com/Fr8org/Fr8Core/blob/master/Docs/Home.md)
Terminals must respond to the following requests:

Requent |	Parameters |	Notes
--- | --- | ---
/configure |	ActionDTO |	The terminal mainly just routes this to the appropriate Action class
/activate |	ActionDTO |	The terminal mainly just routes this to the appropriate Action class, although many activation behaviors will be shared between actions (such as authenticating against an underlying web service like DocuSign) and so much of the code should be shared
/execute |	ActionDTO |	The terminal mainly just routes this to the appropriate Action class
/discover |	none |	The terminal responds with information about itself and its ActivityTemplates
/polling_notifications | [See Scheduling](https://github.com/Fr8org/Fr8Core/blob/master/Docs/ForDevelopers/Services/Scheduling.md) | The endpoint is required to enable scheduling through /alarms/polling

[Go to Contents](https://github.com/Fr8org/Fr8Core/blob/master/Docs/Home.md)
