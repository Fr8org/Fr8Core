Token Considerations
====================

Most of terminals use third-party cloud services like Dropbox, Google and Slack. 
Most of these services rely of OAuth-based security which means that after authorizing Fr8, they return
a string called authorization token. Fr8 saves this string in the secure store and provides it to the service on subsequent requests. 
Everything is nice while the token is valid. However, the token may predictably expire or be invalidated by
certain user actions like disabling the fr8 app explicitly, using the "log out on all devices" command and others. 
Fr8 should be able to handle such cases gracefully in runtime and do not present User with obscure error messages 
(or worse no error messages at all) when they happen.

The Hub has the logic to report authentication and authorization problems in terminals. You only need to add minimum amount of code to enable that for your terminal action. 

More Information:
--[from the .NET SDK](/Docs/ForDevelopers/SDK/.NET/Services/Authorization.md)
