[Back](https://github.com/Fr8org/Fr8Core/blob/master/Docs/ForDevelopers/DevelopmentGuides/PlatformIdependentTerminalDeveloperGuide.md)

# Making Your Terminal Visible on the Public Web 

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

