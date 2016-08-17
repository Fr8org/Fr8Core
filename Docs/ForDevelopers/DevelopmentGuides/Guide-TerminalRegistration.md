## Registering your Terminal with a Hub

Before your terminal can be used with any Hub, it has to be registered.Registering your Terminal with a Hub causes the Hub to:
1) send a /discover request to your Terminal immediately, and each time its starts up thereafter
2) use the response to that /discover request to add your Terminal's Activitiees to the set of Activities made available to client users of that Hub.

In order to register your terminal, do the following: 
* From the Developer menu of the Plan Dashboard, select My Terminals (If you don't see a Developer menu, turn it on in the Tools menu.)  
* Click on the **Add Terminal** button. 
* Paste the public URL you've established for your Terminal into the "Your Terminal's Public Endpoint" text field.

Example endpoints:

myterminal.mydomain.com:2940

inconshreveable.ngrok.io

When you click save, the Hub will _immediately_ attempt to call that endpoint with /discover, and will expect to get a proper [discovery response.](/Docs/ForDevelopers/DevelopmentGuides/Guide-TerminalDiscovery.md), so don't bother trying to register your Terminal until you've verified locally that you can shoot it a discover request. Our [Swagger tools](http://dev-terminals.fr8.co:25923/swagger/ui/index#!/Terminal/Terminal_Get) might help here.

If the terminal is available and can be successfully discovered, it will be registered with the Hub in an Unapproved state. This just means you haven't been approved for production deployment. You should be able to immediately use an account on the development server at dev.fr8.co and can now open Plan Builder (found at https://fr8.co/dashboard/plans or at your local equivalent) and see your new activity in activity selection pane. 

You can even try to add your activity to the plan, but unless you've prepared your Activity to respond to /configure,  activity configuration will fail. 

However, other Fr8.co users won't be able to use the terminal (or even see it), even on the dev server until it is approved by the Fr8 team.
 
In order to get your terminal approved, contact Fr8 Support and provide the production URL for the terminal (it can be the same as the development URL). The terminal should propertly respond to the basic requests from the Hub according to the Fr8 specifications. 

As soon as your terminal is approved, it becomes accessible for all Fr8 users.


For Local Hub Administrators
========================
If you're running the Hub locally and logged in as Fr8 Administrator, you can approve your terminal by clicking on the newly added terminal on the **My Terminal page** and checking the **Approve terminal** checkbox. You will need to confirm approval when you click on Save. 
