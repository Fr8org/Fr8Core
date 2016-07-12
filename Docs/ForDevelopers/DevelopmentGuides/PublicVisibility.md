## Making Your Terminal Visible on the Public Web

NOTE: This is only necessary if you choose, as your [development approach](./ChoosingADevelopmentApproach.md), to test against the public Fr8 Hub, or if you want to go into production and host your own production Terminal.

To work with the Hubs at [dev.fr8.co](http://dev.fr8.co) or [fr8.co](http://fr8.co) you must provide publically accessible HTTP endpoint. Ideally, DNS address of this endpoint should not change through the time, otherwise you'll have to register your terminal everytime endpoint is changed. Fr8 doesn't restrict terminal endpoint to be HTTPS.

So here is few options to go with:

* Hosting providers
* Dynamic DNS services
* ngrok (this is what the core Fr8 team uses. It's free and easy, although we actually pay for the pro service so we can have permanent public ips)
* Fr8 doesn't restrict terminal endpoints to have DNS name. So a publically accessible static IP will work too.
