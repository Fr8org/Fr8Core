[SDK Home](./Home.md)

Overview of the .NET SDK
=======================

The bulk of the SDK is found in the TerminalBase project, which is intended to contain all of the tools and classes useful to Terminal builders. 




Understanding Ports
==================

HubWeb's port is 30643. To learn what port a terminal has you can check it's Web.config file and find a key "terminalName.TerminalEndpoint". Make sure that you have set the same port number in project properties ( Web tab, Project URL ).

Base Classes
=============

BaseConfiguration.cs
--------------------

BaseTerminalWebApiConfig.cs	
---------------------------

DefaultActivityController.cs
----------------------------

DefaultTerminalController.cs
----------------------------

ExplicitTerminalActivity.cs	
----------------------------

OAuthApiIntegrationBase.cs
----------------------------

TerminalActivity.cs	
----------------------------

TerminalActivityBase.cs	
----------------------------
