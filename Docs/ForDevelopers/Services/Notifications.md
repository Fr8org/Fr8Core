Notification Services
=====================


Notifications are messages intended to be seen by end users, as distinct from [Events]().

The primary mechanism for delivering Notifications to users is the [Activity Stream](), which is implemented based on [Pusher]().

Terminals and Hubs generate Activity Stream messages by ________________________

The Client generates Activity Stream messages by ___________________________


App Builder Apps are optimized visually for mobile screen sizes, so they don't use a visible Activity Stream. Instead, _____________________________


Some Activity Stream messages are generated on behalf of the Terminal as a result of [Activity Reponses](). This is generally how a Terminal reports an
error of Validation or Execution back to the end user. They configure the ActivityResponse this way: _______________


FE Details
==============
The client uses an Angular service called UINotificationService.....

The client uses ngToast as well. We are migrating the ngToasts to UI NotificationService.......
