Notification Services
=====================

[Go to Contents](/Docs/Home.md)

Notifications are messages that inform users about ongoing operations.


## Details

The primary mechanism for delivering Notifications is the Activity Stream which is generally inform user about Plan execution phases or messages published by terminals. Notification messages are either displayed in the Activity Stream or with a toast notification. Fr8 User Interface decides which one to use based on user browser.

![ActivityStream](/Docs/img/ActivityStream.png)


## Generating a Notification Message

Notification messages coming from Terminal requests are classified under "Terminal Event" type and displayed with bolt icon on the screen. They can be generated with this call:


*Url*

	{{Fr8HubCallBackUrl}}/api/{{Fr8HubApiVersion}}/notifications

*Method*

    POST

*Request Body*
```javascript
{
	"ActivityName" : "App_Builder",
	"ActivityVersion": "1",
	"Collapsed": false,
	"Message": "This is a plan message/description",
	"Subject": "This is a custom (optional) header for message",
	"TerminalName": "terminalFr8Core",
	"TerminalVersion" : "1"
}
```

[Go to Contents]( /Docs/Home.md)