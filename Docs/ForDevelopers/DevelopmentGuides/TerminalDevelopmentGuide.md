 TERMINAL DEVELOPMENT GUIDE
[Go to Contents](https://github.com/Fr8org/Fr8Core/blob/master/Docs/Home.md) 


Terminals are discrete Web Services that host Activity functionality.
The main Fr8 Hub runs many of these Terminals itself, but the architecture is decoupled: as far as the Hub is concerned, each Terminal is on the other end of an HTTP request. A fully operational Hub may work with hundreds of different Terminals.
Terminals can be written in any language, and only need to support a handful of HTTP endpoints. When Hubs startup, they make a /discover call to the terminal endpoints that they know about. The Terminals respond with information about available activities, and users can then start adding those activities to their plans. 
The initial Terminals being built and hosted by The fr8 Company are .Net based Azure Web App Projects


Basic Topics

Before proceeding, make sure you're famililar with the following basic topics:
*  Fr8 Architecture
*  Fr8 Events
*  Fr8 Crates and Containers
*  Fr8 Activities
*  Fr8 Plans
*  Fr8 Authentication

General Information

This is platform-independent.

[API Endpoints](https://github.com/Fr8org/Fr8Core/blob/master/Docs/ForDevelopers/DevelopmentGuides/Terminals/TerminalEndpoints.md)

Platform-Specific Information
=====
*  [.NET](https://github.com/Fr8org/Fr8Core/blob/master/Docs/ForDevelopers/DevelopmentGuides/Terminals/DevGuide_DotNet.md)
*  Java
*  Ruby
