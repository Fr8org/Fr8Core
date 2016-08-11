# Terminal Authentication

[Go to Contents](/Docs/Home.md)

## Summary

Terminals often needs to communicate with the Hub. Some examples include getting payload, running a plan or creating an activity. These communications requires a terminal to be authenticated by the Hub.

## Authentication with Hub

When the Hub makes a request to a terminal, it includes 3 headers which are required to authenticate with the Hub. Those headers are;

* **Fr8HubCallbackSecret**: 4b54d12f7f834648be28aa247f523e21
* **Fr8HubCallBackUrl**: http://dev.fr8.co/
* **Fr8UserId**: d4991c09-77ee-42de-9ae7-15c1b6c2d3ca

### Fr8HubCallbackSecret

This header contains the secret key for your terminal. When you need to communicate back with the Hub you will need this secret as your terminal identifier.

### Fr8HubCallBackUrl

This header contains the url of the Hub which is making request to your terminal. All your communications should be made with this Hub.

Fr8 is a distributed environment. Your terminal might be in use by many Hubs. Therefore this header contains url of the current Hub which is making the request.

### Fr8UserId

This header contains the id of the user. Current request to your terminal is made on behalf of this user.

### Authentication

When your terminal needs to make a request to the Hub, it needs to add FR8-TOKEN Authorization header to it's request.

Header value is created using the following format (without quotes): "FR8-TOKEN key={Fr8HubCallbackSecret}, user={Fr8UserId}"

Here is a an example text of required request headers:

	Authorization: FR8-TOKEN key=2db48191-cda3-4922-9cc2-a636e828063f, user=76de71f2-f346-4bc9-96e0-f7bd1c87a575


[Go to Contents](/Docs/Home.md)
