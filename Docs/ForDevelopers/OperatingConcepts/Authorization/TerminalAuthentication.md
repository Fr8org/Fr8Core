# Terminal Authentication

[Go to Contents](/Docs/Home.md)

## Summary

Terminals often needs to communicate with the Hub. Some examples include getting payload, running a plan or creating an activity. These communications requires a terminal to be authenticated by the Hub.

## Authentication your Terminal

Each request from the Hub to your Terminal includes these 3 headers:

* **Fr8HubCallbackSecret**: 4b54d12f7f834648be28aa247f523e21
* **Fr8HubCallBackUrl**: http://dev.fr8.co/
* **Fr8UserId**: d4991c09-77ee-42de-9ae7-15c1b6c2d3ca

### Fr8HubCallbackSecret

The Hub generates this when it learns about a new Terminal. It functions in most respects as your Terminal's ID value but can be changed if it has been compromised.  All of your Http requests need to include it in the header.

### Fr8HubCallBackUrl

This is the endpoint of the Hub that is making the request.

Fr8 is a distributed environment. Your terminal might be in use by many Hubs. This property lets you know which Hub to respond to.

### Fr8UserId

This header contains the id of the user. Current request to your terminal is made on behalf of this user.

### Generating your Authentication Header

When your terminal needs to make a request to the Hub, it needs to provide an authorization header with the terminal key and user id

Example:

	Authorization: FR8-TOKEN key=2db48191-cda3-4922-9cc2-a636e828063f, user=76de71f2-f346-4bc9-96e0-f7bd1c87a575

[Go to Contents](/Docs/Home.md)
