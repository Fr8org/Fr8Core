# TERMINALS
[Go to Contents](https://github.com/Fr8org/Fr8Core/blob/master/Docs/Home.md)

Terminals are discrete Web Services that host Action functionality.

The main fr8 Hub runs many of these Terminals itself, but the architecture is decoupled: as far as the Hub is concerned, each Terminal is on the other end of an HTTP request. A fully operational Hub may work with hundreds of different Terminals.

The initial Terminals being built and hosted by The fr8 Company are .Net based Azure Web App Projects, but the only requirement a Terminal has is that it responds to the commands in the V1.0 fr8 Spec. These commands currently include:

See: [Supported Terminals](https://maginot.atlassian.net/wiki/display/SH/Supported+Terminals).

Command		| Description
--- | ---
/configure |	A Hub calls this when a user is trying to configure an Action hosted by the Terminal. The Terminal must return configuration specifications if any are necessary. Called by a Hub at design-time.	
/activate |	A Hub calls this when it is preparing to activate a fr8Line that contains an Action hosted by this Terminal. Used to do things like register for notification with web services	
/run |	Formerly “Execute”. A Hub calls this when it is processing a Pallet through a fr8Line and flow reaches a particular Action hosted by this Terminal	
[/events](https://maginot.atlassian.net/wiki/display/SH/Events) |	Used by a Hub to pass a received external event notification in for parsing	
[/discover](https://github.com/Fr8org/Fr8Core/blob/master/Docs/ForDevelopers/Objects/Terminal/Discovery.md) |	Used by a Hub to request Plugin information and ActivityTemplate data	
Learn About:

[Terminal Authentication](https://maginot.atlassian.net/wiki/display/SH/Terminal+Authentication)

[Terminal Events](https://maginot.atlassian.net/wiki/display/SH/Events)

[Terminal Registration](https://maginot.atlassian.net/wiki/display/SH/Registration+of+Terminals+%28Plugins%29+and+Actions)
[Terminal Discovery](https://github.com/Fr8org/Fr8Core/blob/master/Docs/ForDevelopers/Objects/Terminal/Discovery.md)

[Go to Contents](https://github.com/Fr8org/Fr8Core/blob/master/Docs/Home.md)
