# Terminal Authentication

[Go to Contents](/Docs/Home.md)

## Summary

Terminals often needs to communicate with the Hub. Some examples include getting payload, running a plan or creating an activity. These communications requires a terminal to be authenticated by the Hub.

## Authentication with Hub

Each request from the Hub to your Terminal includes these 2 properties:

* **TerminalKey**: 4b54d12f7f834648be28aa247f523e21
* **CurrentHubUrl**: http://dev.fr8.co/

### Terminal Key

The Hub generates this when it learns about a new Terminal. It functions in most respects as your Terminal's ID value but can be changed if it has been compromised.  All of your Http requests need to include it in the header: 

Authorization: FR8 terminal_key=2db48191-cda3-4922-9cc2-a636e828063f

### HubUrl

This is the endpoint of the Hub that is making the request. 

Fr8 is a distributed environment. Your terminal might be in use by many Hubs. This property lets you know which Hub to respond to.



### Generating your Authentication Header

When your terminal needs to make a request to the Hub, it needs to provide an authorization header with the terminal key

Example:

	Authorization: FR8-TOKEN key=2db48191-cda3-4922-9cc2-a636e828063f


###Note. It was previously also necessary to put the ID of the current user session into the authorization header, but that has been removed. 

[Go to Contents](/Docs/Home.md)
