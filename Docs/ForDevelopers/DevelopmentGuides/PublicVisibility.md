[Back](https://github.com/Fr8org/Fr8Core/blob/master/Docs/ForDevelopers/DevelopmentGuides/PlatformIdependentTerminalDeveloperGuide.md)

## Making Your Terminal Visible on the Public Web (Connecting your Terminal to a Hub)

NOTE: This is only necessary if you choose, as your [development approach](./ChoosingADevelopmentApproach.md), to test against the public Fr8 Hub, or if you want to go into production and host your own production Terminal.

The process for plugging your Terminal into Fr8.co is:

1) Figure out a way for it to be visible via a public URL

2) Register it with The Fr8 Company's dev server at dev.fr8.co

3) Get it approved

Make Your Terminal Visible via a Public URL
-------------------------------------------

To work with the Hubs at [dev.fr8.co](http://dev.fr8.co) or [fr8.co](http://fr8.co) you must provide an HTTP endpoint that the Fr8 Hub can reach. Ideally, the DNS address of this endpoint should not change, because each time you change it, you'll have to modify your Terminal registration. 

So here are a few options to go with:

* Hosting providers
* Dynamic DNS services
* ngrok (this is what the core Fr8 team uses. It's free and easy, although we actually pay for the pro service so we can have permanent public ips)
* Fr8 doesn't restrict terminal endpoints to have DNS name. So a publically accessible static IP will work too.

Fr8 doesn't currently restrict terminal endpoints to be HTTPS but of course strongly recommends it. We'll probably require it soon for Terminals that want to be connected to the production Hub at fr8.co but will probably leave it optional for the dev hub at dev.fr8.co.

Register your Terminal
------------------------------

Before your terminal can be used with any Hub, it has to be registered. In order to register your terminal, do the following: 
* From the Developer menu of the Plan Dashboard, select My Terminals (If you don't see a Developer menu, turn it on in the Tools menu.)  
* Click on the **Add Terminal** button. 
* Paste the public URL you've established for your Terminal into the "Your Terminal's Public Endpoint" text field.

Example endpoints:
myterminal.mydomain.com:2940
http://inconshreveable.ngrok.io

When you click save, the Hub will _immediately_ attempt to call that endpoint with /discover, and will expect to get a proper [discovery response.](/Docs/ForDevelopers/DevelopmentGuides/Guide-TerminalDiscovery.md), so don't bother trying to register your Terminal until you've verified locally that you can shoot it a discover request. Our [Swagger tools](http://dev-terminals.fr8.co:25923/swagger/ui/index#!/Terminal/Terminal_Get) might help here.

If the terminal is available and can be successfully discovered, it will be registered with the Hub in an Unapproved state. This just means you haven't been approved for production deployment. You should be able to immediately use an account on the development server at dev.fr8.co and see any Activities that you've provide ActivityTemplates for in your /discover response.

 However, other Fr8.co users won't be able to use the terminal (or even see it), even on the dev server until it is approved by the Fr8 team.
 
 In order to get your terminal approved, contact Fr8 Support and provide the production URL for the terminal (it can be the same as the development URL). The terminal should propertly respond to the basic requests from the Hub according to the Fr8 specifications. 

As soon as your terminal is approved, it becomes accessible for all Fr8 users.

If you're running the Hub locally and logged in as Fr8 Administrator, you can approve your terminal by clicking on the newly added terminal on the **My Terminal page** and checking the **Approve terminal** checkbox. You will need to confirm approval when you click on Save. 
